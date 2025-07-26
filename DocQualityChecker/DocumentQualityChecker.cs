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
            var pixels = image.Pixels;
            int count = pixels.Length;

            foreach (var p in pixels)
            {
                double intensity = (p.Red + p.Green + p.Blue) / 3.0;
                sum += intensity;
                sumSq += intensity * intensity;
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
            var pixels = image.Pixels;
            int w = image.Width;

            for (int y = 1; y < image.Height - 1; y++)
            {
                int row = y * w;
                for (int x = 1; x < image.Width - 1; x++)
                {
                    int idx = row + x;
                    double c00 = Intensity(pixels[idx - w]);
                    double c10 = Intensity(pixels[idx - 1]);
                    double c11 = Intensity(pixels[idx]);
                    double c12 = Intensity(pixels[idx + 1]);
                    double c20 = Intensity(pixels[idx + w]);

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
        /// Estimates motion blur by comparing horizontal and vertical gradient energy.
        /// Values far from 1 indicate strong directional blur.
        /// </summary>
        public double ComputeMotionBlurScore(SKBitmap image)
        {
            if (image == null) throw new ArgumentNullException(nameof(image));

            double sumX = 0, sumY = 0;
            var pixels = image.Pixels;
            int w = image.Width;

            for (int y = 1; y < image.Height - 1; y++)
            {
                int row = y * w;
                for (int x = 1; x < image.Width - 1; x++)
                {
                    int idx = row + x;
                    double i1 = Intensity(pixels[idx + 1]);
                    double i2 = Intensity(pixels[idx - 1]);
                    double i3 = Intensity(pixels[idx + w]);
                    double i4 = Intensity(pixels[idx - w]);

                    sumX += Math.Abs(i1 - i2);
                    sumY += Math.Abs(i3 - i4);
                }
            }

            double max = Math.Max(sumX, sumY);
            double min = Math.Min(sumX, sumY) + 1e-5;
            return max / min;
        }

        public bool HasMotionBlur(SKBitmap image, double threshold, out double score)
        {
            score = ComputeMotionBlurScore(image);
            return score > threshold;
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
            var pixels = image.Pixels;
            var span = map.GetPixelSpan(); // Span<byte>
            int w = image.Width;

            for (int y = 1; y < image.Height - 1; y++)
            {
                int row = y * w;
                for (int x = 1; x < image.Width - 1; x++)
                {
                    int idx = row + x;
                    double c00 = Intensity(pixels[idx - w]);
                    double c10 = Intensity(pixels[idx - 1]);
                    double c11 = Intensity(pixels[idx]);
                    double c12 = Intensity(pixels[idx + 1]);
                    double c20 = Intensity(pixels[idx + w]);

                    double lap = kernel[0, 1] * c00 +
                                  kernel[1, 0] * c10 +
                                  kernel[1, 1] * c11 +
                                  kernel[1, 2] * c12 +
                                  kernel[2, 1] * c20;

                    span[idx] = (byte)Math.Clamp(Math.Abs(lap), 0, 255);
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
            var pixels = image.Pixels;
            var span = mask.GetPixelSpan(); // Span<byte>
            int w = image.Width;

            for (int y = 1; y < image.Height - 1; y++)
            {
                int row = y * w;
                for (int x = 1; x < image.Width - 1; x++)
                {
                    int idx = row + x;
                    double c00 = Intensity(pixels[idx - w]);
                    double c10 = Intensity(pixels[idx - 1]);
                    double c11 = Intensity(pixels[idx]);
                    double c12 = Intensity(pixels[idx + 1]);
                    double c20 = Intensity(pixels[idx + w]);

                    double lap = kernel[0, 1] * c00 +
                                  kernel[1, 0] * c10 +
                                  kernel[1, 1] * c11 +
                                  kernel[1, 2] * c12 +
                                  kernel[2, 1] * c20;

                    double val = lap * lap;
                    span[idx] = val < threshold ? (byte)255 : (byte)0;
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
            var pixels = image.Pixels;
            int count = pixels.Length;

            foreach (var p in pixels)
            {
                double lum = 0.299 * p.Red + 0.587 * p.Green + 0.114 * p.Blue;
                sum += lum;
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
            var pixels = image.Pixels;
            int count = pixels.Length;
            foreach (var p in pixels)
            {
                double lum = 0.299 * p.Red + 0.587 * p.Green + 0.114 * p.Blue;
                sum += lum;
                sumSq += lum * lum;
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
            var pixels = image.Pixels;
            int count = pixels.Length;
            foreach (var p in pixels)
            {
                r += p.Red;
                g += p.Green;
                b += p.Blue;
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
            var pixels = image.Pixels;
            int w = image.Width;
            for (int y = 1; y < image.Height - 1; y++)
            {
                int row = y * w;
                for (int x = 1; x < image.Width - 1; x++)
                {
                    int idx = row + x;
                    double center = Intensity(pixels[idx]);
                    double neighborSum = 0;
                    for (int j = -1; j <= 1; j++)
                        for (int i = -1; i <= 1; i++)
                            if (i != 0 || j != 0)
                                neighborSum += Intensity(pixels[idx + i + j * w]);
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

        /// <summary>
        /// Computes a normalized variance of row/column averages to detect banding.
        /// Values above 0.5 generally indicate visible stripes.
        /// </summary>
        public double ComputeBandingScore(SKBitmap image)
        {
            if (image == null) throw new ArgumentNullException(nameof(image));

            int w = image.Width;
            int h = image.Height;
            var rowMeans = new double[h];
            var colMeans = new double[w];
            double sum = 0, sumSq = 0;
            var pixels = image.Pixels;

            for (int y = 0; y < h; y++)
            {
                int row = y * w;
                for (int x = 0; x < w; x++)
                {
                    double val = Intensity(pixels[row + x]);
                    rowMeans[y] += val;
                    colMeans[x] += val;
                    sum += val;
                    sumSq += val * val;
                }
            }

            for (int i = 0; i < h; i++) rowMeans[i] /= w;
            for (int i = 0; i < w; i++) colMeans[i] /= h;

            double mean = sum / (w * h);
            double var = sumSq / (w * h) - mean * mean + 1e-5;

            double rowVar = 0, colVar = 0;
            for (int i = 0; i < h; i++)
            {
                double d = rowMeans[i] - mean;
                rowVar += d * d;
            }
            rowVar /= h;
            for (int i = 0; i < w; i++)
            {
                double d = colMeans[i] - mean;
                colVar += d * d;
            }
            colVar /= w;

            double bandVar = Math.Max(rowVar, colVar);
            return bandVar / var;
        }

        public bool HasBanding(SKBitmap image, double threshold, out double score)
        {
            score = ComputeBandingScore(image);
            return score > threshold;
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
            var pixels = image.Pixels;

            foreach (var p in pixels)
            {
                double intensity = Intensity(p);
                if (intensity >= brightThreshold)
                {
                    count++;
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
            var src = image.Pixels;
            var dst = map.GetPixelSpan();

            for (int i = 0; i < src.Length; i++)
            {
                byte val = Intensity(src[i]) >= brightThreshold ? (byte)255 : (byte)0;
                dst[i] = val;
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
            var span = mask.GetPixelSpan();
            int w = mask.Width;

            for (int y = 0; y < mask.Height; y++)
            {
                int row = y * w;
                for (int x = 0; x < mask.Width; x++)
                {
                    int idx = row + x;
                    if (visited[x, y]) continue;
                    if (span[idx] == 0) continue;

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
                            int nidx = ny * w + nx;
                            if (span[nidx] == 0)
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
                HasMotionBlur = HasMotionBlur(image, settings.MotionBlurThreshold, out var mblur),
                MotionBlurScore = mblur,
                HasGlare = HasGlare(image, settings.BrightThreshold, settings.AreaThreshold, out var area),
                GlareArea = area,
                IsWellExposed = IsWellExposed(image, settings.ExposureMin, settings.ExposureMax, out var exposure),
                Exposure = exposure,
                HasLowContrast = HasLowContrast(image, settings.ContrastMin, out var contrast),
                Contrast = contrast,
                HasColorDominance = HasColorDominance(image, settings.DominanceThreshold, out var dom),
                ColorDominance = dom,
                HasNoise = HasNoise(image, settings.NoiseThreshold, out var noise),
                Noise = noise,
                HasBanding = HasBanding(image, settings.BandingThreshold, out var band),
                BandingScore = band
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
                                     !result.HasNoise &&
                                     !result.HasMotionBlur &&
                                     !result.HasBanding;

            return result;
        }
    }
}

