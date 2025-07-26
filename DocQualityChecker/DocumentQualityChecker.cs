using System;
using SkiaSharp;

namespace DocQualityChecker
{
    /// <summary>
    /// Provides methods to evaluate image quality for identity documents.
    /// This implementation relies solely on managed SkiaSharp APIs to avoid
    /// native OpenCV dependencies.
    /// </summary>
    public class DocumentQualityChecker
    {
        /// <summary>
        /// Computes a simple quality score based on pixel intensity variance.
        /// </summary>
        /// <param name="image">Source image.</param>
        /// <returns>Variance of grayscale intensities.</returns>
        /// <exception cref="ArgumentNullException">Thrown when image is null.</exception>
        public double ComputeBrisqueScore(SKBitmap image)
        {
            if (image == null) throw new ArgumentNullException(nameof(image));

            double sum = 0;
            double sumSq = 0;
            int count = image.Width * image.Height;

            for (int y = 0; y < image.Height; y++)
            {
                for (int x = 0; x < image.Width; x++)
                {
                    var p = image.GetPixel(x, y);
                    double intensity = (p.Red + p.Green + p.Blue) / 3.0;
                    sum += intensity;
                    sumSq += intensity * intensity;
                }
            }

            double mean = sum / count;
            double variance = sumSq / count - mean * mean;
            // normalize to 0-100 range
            return variance / (255.0 * 255.0) * 100.0;
        }

        /// <summary>
        /// Checks if the image is blurry using a simple Laplacian variance metric.
        /// </summary>
        /// <param name="image">Source image.</param>
        /// <param name="threshold">Blur threshold.</param>
        /// <param name="blurScore">Returned blur score.</param>
        /// <returns>True if image is considered blurry.</returns>
        public bool IsBlurry(SKBitmap image, double threshold, out double blurScore)
        {
            if (image == null) throw new ArgumentNullException(nameof(image));

            double sumSq = 0;
            int count = 0;

            int[,] kernel = new int[,] { { 0, 1, 0 }, { 1, -4, 1 }, { 0, 1, 0 } };

            for (int y = 1; y < image.Height - 1; y++)
            {
                for (int x = 1; x < image.Width - 1; x++)
                {
                    double c00 = Intensity(image.GetPixel(x, y - 1));
                    double c10 = Intensity(image.GetPixel(x - 1, y));
                    double c11 = Intensity(image.GetPixel(x, y));
                    double c12 = Intensity(image.GetPixel(x + 1, y));
                    double c20 = Intensity(image.GetPixel(x, y + 1));

                    double lap = kernel[0, 1] * c00 +
                                  kernel[1, 0] * c10 +
                                  kernel[1, 1] * c11 +
                                  kernel[1, 2] * c12 +
                                  kernel[2, 1] * c20;

                    sumSq += lap * lap;
                    count++;
                }
            }

            blurScore = sumSq / count;
            return blurScore < threshold;
        }

        /// <summary>
        /// Generates a heatmap representing the amount of blur for each pixel.
        /// White pixels represent areas with high blur.
        /// </summary>
        public SKBitmap CreateBlurHeatmap(SKBitmap image)
        {
            if (image == null) throw new ArgumentNullException(nameof(image));

            var map = new SKBitmap(image.Width, image.Height, SKColorType.Gray8, SKAlphaType.Opaque);
            int[,] kernel = new int[,] { { 0, 1, 0 }, { 1, -4, 1 }, { 0, 1, 0 } };

            for (int y = 1; y < image.Height - 1; y++)
            {
                for (int x = 1; x < image.Width - 1; x++)
                {
                    double c00 = Intensity(image.GetPixel(x, y - 1));
                    double c10 = Intensity(image.GetPixel(x - 1, y));
                    double c11 = Intensity(image.GetPixel(x, y));
                    double c12 = Intensity(image.GetPixel(x + 1, y));
                    double c20 = Intensity(image.GetPixel(x, y + 1));

                    double lap = kernel[0, 1] * c00 +
                                  kernel[1, 0] * c10 +
                                  kernel[1, 1] * c11 +
                                  kernel[1, 2] * c12 +
                                  kernel[2, 1] * c20;

                    int val = (int)Math.Clamp(Math.Abs(lap), 0, 255);
                    map.SetPixel(x, y, new SKColor((byte)val, (byte)val, (byte)val));
                }
            }

            return map;
        }

        /// <summary>
        /// Finds bounding boxes of blurred regions by thresholding the Laplacian
        /// variance per pixel.
        /// </summary>
        public List<SKRectI> FindBlurRegions(SKBitmap image, double threshold)
        {
            if (image == null) throw new ArgumentNullException(nameof(image));

            var mask = new SKBitmap(image.Width, image.Height, SKColorType.Gray8, SKAlphaType.Opaque);
            int[,] kernel = new int[,] { { 0, 1, 0 }, { 1, -4, 1 }, { 0, 1, 0 } };

            for (int y = 1; y < image.Height - 1; y++)
            {
                for (int x = 1; x < image.Width - 1; x++)
                {
                    double c00 = Intensity(image.GetPixel(x, y - 1));
                    double c10 = Intensity(image.GetPixel(x - 1, y));
                    double c11 = Intensity(image.GetPixel(x, y));
                    double c12 = Intensity(image.GetPixel(x + 1, y));
                    double c20 = Intensity(image.GetPixel(x, y + 1));

                    double lap = kernel[0, 1] * c00 +
                                  kernel[1, 0] * c10 +
                                  kernel[1, 1] * c11 +
                                  kernel[1, 2] * c12 +
                                  kernel[2, 1] * c20;

                    double val = lap * lap;
                    byte b = val < threshold ? (byte)255 : (byte)0;
                    mask.SetPixel(x, y, new SKColor(b, b, b));
                }
            }

            return FindConnectedComponents(mask);
        }

        /// <summary>
        /// Computes the average luminance of the image.
        /// </summary>
        public double ComputeExposure(SKBitmap image)
        {
            if (image == null) throw new ArgumentNullException(nameof(image));

            double sum = 0;
            int count = image.Width * image.Height;

            for (int y = 0; y < image.Height; y++)
            {
                for (int x = 0; x < image.Width; x++)
                {
                    var p = image.GetPixel(x, y);
                    double lum = 0.299 * p.Red + 0.587 * p.Green + 0.114 * p.Blue;
                    sum += lum;
                }
            }

            return sum / count;
        }

        public bool IsWellExposed(SKBitmap image, double min, double max, out double exposure)
        {
            exposure = ComputeExposure(image);
            return exposure >= min && exposure <= max;
        }

        /// <summary>
        /// Computes the standard deviation of luminance to estimate contrast.
        /// </summary>
        public double ComputeContrast(SKBitmap image)
        {
            if (image == null) throw new ArgumentNullException(nameof(image));

            double sum = 0, sumSq = 0;
            int count = image.Width * image.Height;
            for (int y = 0; y < image.Height; y++)
            {
                for (int x = 0; x < image.Width; x++)
                {
                    double lum = 0.299 * image.GetPixel(x, y).Red + 0.587 * image.GetPixel(x, y).Green + 0.114 * image.GetPixel(x, y).Blue;
                    sum += lum;
                    sumSq += lum * lum;
                }
            }
            double mean = sum / count;
            double var = sumSq / count - mean * mean;
            return Math.Sqrt(Math.Max(var, 0));
        }

        public bool HasLowContrast(SKBitmap image, double threshold, out double contrast)
        {
            contrast = ComputeContrast(image);
            return contrast < threshold;
        }

        /// <summary>
        /// Returns the ratio of the dominant RGB channel over the average.
        /// </summary>
        public double ComputeColorDominance(SKBitmap image)
        {
            if (image == null) throw new ArgumentNullException(nameof(image));

            double r = 0, g = 0, b = 0;
            int count = image.Width * image.Height;
            for (int y = 0; y < image.Height; y++)
            {
                for (int x = 0; x < image.Width; x++)
                {
                    var p = image.GetPixel(x, y);
                    r += p.Red;
                    g += p.Green;
                    b += p.Blue;
                }
            }
            r /= count;
            g /= count;
            b /= count;
            double avg = (r + g + b) / 3.0 + 1e-5;
            double max = Math.Max(r, Math.Max(g, b));
            return max / avg;
        }

        public bool HasColorDominance(SKBitmap image, double threshold, out double dominance)
        {
            dominance = ComputeColorDominance(image);
            return dominance > threshold;
        }

        /// <summary>
        /// Estimates noise by computing variance from local mean in a 3x3 window.
        /// </summary>
        public double ComputeNoise(SKBitmap image)
        {
            if (image == null) throw new ArgumentNullException(nameof(image));

            double sum = 0;
            int count = 0;
            for (int y = 1; y < image.Height - 1; y++)
            {
                for (int x = 1; x < image.Width - 1; x++)
                {
                    double center = Intensity(image.GetPixel(x, y));
                    double neighborSum = 0;
                    for (int j = -1; j <= 1; j++)
                        for (int i = -1; i <= 1; i++)
                            if (i != 0 || j != 0)
                                neighborSum += Intensity(image.GetPixel(x + i, y + j));
                    double mean = neighborSum / 8.0;
                    double diff = center - mean;
                    sum += diff * diff;
                    count++;
                }
            }
            return sum / count;
        }

        public bool HasNoise(SKBitmap image, double threshold, out double noise)
        {
            noise = ComputeNoise(image);
            return noise > threshold;
        }

        private static double Intensity(SKColor p)
        {
            return (p.Red + p.Green + p.Blue) / 3.0;
        }

        /// <summary>
        /// Detects presence of glare based on bright pixel area.
        /// </summary>
        /// <param name="image">Source image.</param>
        /// <param name="brightThreshold">Brightness threshold.</param>
        /// <param name="areaThreshold">Minimum glare area.</param>
        /// <param name="glareArea">Total area of detected glare.</param>
        /// <returns>True if glare is detected above the threshold.</returns>
        public bool HasGlare(SKBitmap image, int brightThreshold, int areaThreshold, out int glareArea)
        {
            if (image == null) throw new ArgumentNullException(nameof(image));

            int count = 0;

            for (int y = 0; y < image.Height; y++)
            {
                for (int x = 0; x < image.Width; x++)
                {
                    double intensity = Intensity(image.GetPixel(x, y));
                    if (intensity >= brightThreshold)
                    {
                        count++;
                    }
                }
            }

            glareArea = count;
            return glareArea > areaThreshold;
        }

        /// <summary>
        /// Creates a heatmap highlighting glare pixels.
        /// </summary>
        public SKBitmap CreateGlareHeatmap(SKBitmap image, int brightThreshold)
        {
            if (image == null) throw new ArgumentNullException(nameof(image));

            var map = new SKBitmap(image.Width, image.Height, SKColorType.Gray8, SKAlphaType.Opaque);

            for (int y = 0; y < image.Height; y++)
            {
                for (int x = 0; x < image.Width; x++)
                {
                    double intensity = Intensity(image.GetPixel(x, y));
                    byte val = intensity >= brightThreshold ? (byte)255 : (byte)0;
                    map.SetPixel(x, y, new SKColor(val, val, val));
                }
            }

            return map;
        }

        /// <summary>
        /// Finds bounding boxes of glare regions based on a brightness threshold.
        /// </summary>
        public List<SKRectI> FindGlareRegions(SKBitmap image, int brightThreshold)
        {
            if (image == null) throw new ArgumentNullException(nameof(image));

            var mask = CreateGlareHeatmap(image, brightThreshold);
            return FindConnectedComponents(mask);
        }

        private static List<SKRectI> FindConnectedComponents(SKBitmap mask)
        {
            var visited = new bool[mask.Width, mask.Height];
            var rects = new List<SKRectI>();

            int[] dx = new[] { 1, -1, 0, 0 };
            int[] dy = new[] { 0, 0, 1, -1 };

            for (int y = 0; y < mask.Height; y++)
            {
                for (int x = 0; x < mask.Width; x++)
                {
                    if (visited[x, y]) continue;
                    if (mask.GetPixel(x, y).Red == 0) continue;

                    int minX = x, minY = y, maxX = x, maxY = y;
                    var queue = new Queue<(int X, int Y)>();
                    queue.Enqueue((x, y));
                    visited[x, y] = true;

                    while (queue.Count > 0)
                    {
                        var (cx, cy) = queue.Dequeue();
                        for (int dir = 0; dir < 4; dir++)
                        {
                            int nx = cx + dx[dir];
                            int ny = cy + dy[dir];
                            if (nx < 0 || ny < 0 || nx >= mask.Width || ny >= mask.Height)
                                continue;
                            if (visited[nx, ny])
                                continue;
                            if (mask.GetPixel(nx, ny).Red == 0)
                                continue;
                            visited[nx, ny] = true;
                            queue.Enqueue((nx, ny));
                            if (nx < minX) minX = nx;
                            if (ny < minY) minY = ny;
                            if (nx > maxX) maxX = nx;
                            if (ny > maxY) maxY = ny;
                        }
                    }

                    rects.Add(new SKRectI(minX, minY, maxX + 1, maxY + 1));
                }
            }

            return rects;
        }

        /// <summary>
        /// Performs all quality checks on the provided image.
        /// </summary>
        public DocumentQualityResult CheckQuality(SKBitmap image, QualitySettings settings)
        {
            if (image == null) throw new ArgumentNullException(nameof(image));
            if (settings == null) throw new ArgumentNullException(nameof(settings));

            var result = new DocumentQualityResult
            {
                BrisqueScore = ComputeBrisqueScore(image),
                IsBlurry = IsBlurry(image, settings.BlurThreshold, out var blurScore),
                BlurScore = blurScore,
                HasGlare = HasGlare(image, settings.BrightThreshold, settings.AreaThreshold, out var area),
                GlareArea = area,
                IsWellExposed = IsWellExposed(image, settings.ExposureMin, settings.ExposureMax, out var exposure),
                Exposure = exposure,
                HasLowContrast = HasLowContrast(image, settings.ContrastMin, out var contrast),
                Contrast = contrast,
                HasColorDominance = HasColorDominance(image, settings.DominanceThreshold, out var dom),
                ColorDominance = dom,
                HasNoise = HasNoise(image, settings.NoiseThreshold, out var noise),
                Noise = noise
            };

            if (settings.GenerateHeatmaps)
            {
                result.BlurHeatmap = CreateBlurHeatmap(image);
                result.GlareHeatmap = CreateGlareHeatmap(image, settings.BrightThreshold);
                result.BlurRegions = FindBlurRegions(image, settings.BlurThreshold);
                result.GlareRegions = FindGlareRegions(image, settings.BrightThreshold);
            }

            result.IsValidDocument = result.BrisqueScore <= settings.BrisqueMax &&
                                     !result.IsBlurry &&
                                     !result.HasGlare &&
                                     result.IsWellExposed &&
                                     !result.HasLowContrast &&
                                     !result.HasColorDominance &&
                                     !result.HasNoise;

            return result;
        }
    }
}

