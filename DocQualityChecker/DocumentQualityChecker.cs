using System;
using System.Linq;
using System.Threading;
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

            var intensities = GetIntensityBuffer(image);
            return ComputeBrisqueScore(intensities);
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
            var intensities = GetIntensityBuffer(image);
            return IsBlurry(intensities, image.Width, image.Height, threshold, out blurScore);
        }

        /// <summary>
        /// Estimates motion blur by comparing horizontal and vertical gradient energy.
        /// Values far from 1 indicate strong directional blur.
        /// </summary>
        public double ComputeMotionBlurScore(SKBitmap image)
        {
            if (image == null) throw new ArgumentNullException(nameof(image));
            var intensities = GetIntensityBuffer(image);
            return ComputeMotionBlurScore(intensities, image.Width, image.Height);
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
            var intensities = GetIntensityBuffer(image);
            var lap = BuildLaplacian(intensities, image.Width, image.Height);
            return CreateBlurHeatmap(lap, image.Width, image.Height);
        }

        /// <summary>
        /// Finds bounding boxes of blurred regions by thresholding the Laplacian
        /// variance per pixel.
        /// </summary>
        public List<SKRectI> FindBlurRegions(SKBitmap image, double threshold)
        {
            if (image == null) throw new ArgumentNullException(nameof(image));
            var intensities = GetIntensityBuffer(image);
            return FindBlurRegions(intensities, image.Width, image.Height, threshold);
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

        private static double ComputeBrisqueScore(double[] intensities)
        {
            double sum = 0;
            double sumSq = 0;
            int count = intensities.Length;

            foreach (double i in intensities)
            {
                sum += i;
                sumSq += i * i;
            }

            double mean = sum / count;
            double variance = sumSq / count - mean * mean;
            return variance / (255.0 * 255.0) * 100.0;
        }

        private static bool IsBlurry(double[] intensities, int w, int h, double threshold, out double blurScore)
        {
            BuildLaplacian(intensities, w, h, out blurScore);
            return blurScore < threshold;
        }

        private static bool IsBlurryFromLaplacian(double[] laplacian, int w, int h, double threshold, out double blurScore)
        {
            double sumSq = 0;
            for (int y = 1; y < h - 1; y++)
            {
                int row = y * w;
                for (int x = 1; x < w - 1; x++)
                {
                    double val = laplacian[row + x];
                    sumSq += val * val;
                }
            }

            blurScore = sumSq / ((w - 2) * (h - 2));
            return blurScore < threshold;
        }

        private static double ComputeMotionBlurScore(double[] intensities, int w, int h)
        {
            double sumX = 0, sumY = 0;
            for (int y = 1; y < h - 1; y++)
            {
                int row = y * w;
                for (int x = 1; x < w - 1; x++)
                {
                    int idx = row + x;
                    double left = intensities[idx - 1];
                    double right = intensities[idx + 1];
                    double up = intensities[idx - w];
                    double down = intensities[idx + w];

                    sumX += Math.Abs(right - left);
                    sumY += Math.Abs(down - up);
                }
            }

            double max = Math.Max(sumX, sumY);
            double min = Math.Min(sumX, sumY) + 1e-5;
            return max / min;
        }

        private static double[] BuildLaplacian(double[] intensities, int w, int h, out double variance)
        {
            var lap = new double[intensities.Length];
            double sumSq = 0;

            object lockObj = new object();
            Parallel.For(1, h - 1, () => 0.0, (y, _, local) =>
            {
                int row = y * w;
                for (int x = 1; x < w - 1; x++)
                {
                    int idx = row + x;
                    double val = intensities[idx - w] + intensities[idx - 1] + intensities[idx + 1] + intensities[idx + w] - 4 * intensities[idx];
                    lap[idx] = val;
                    local += val * val;
                }

                return local;
            }, local =>
            {
                lock (lockObj)
                {
                    sumSq += local;
                }
            });

            variance = sumSq / ((w - 2) * (h - 2));
            return lap;
        }

        private static double[] BuildLaplacian(double[] intensities, int w, int h)
            => BuildLaplacian(intensities, w, h, out _);

        private static SKBitmap CreateBlurHeatmap(double[] laplacian, int w, int h)
        {
            var map = new SKBitmap(w, h, SKColorType.Gray8, SKAlphaType.Opaque);
            var span = map.GetPixelSpan();

            for (int i = 0; i < laplacian.Length; i++)
            {
                span[i] = (byte)Math.Clamp(Math.Abs(laplacian[i]), 0, 255);
            }

            return map;
        }

        private static byte[] BuildBlurMask(double[] laplacian, double threshold)
        {
            var mask = new byte[laplacian.Length];
            Parallel.For(0, laplacian.Length, i =>
            {
                double val = laplacian[i] * laplacian[i];
                mask[i] = val < threshold ? (byte)255 : (byte)0;
            });
            return mask;
        }

        private static List<SKRectI> FindBlurRegions(double[] intensities, int w, int h, double threshold)
        {
            var lap = BuildLaplacian(intensities, w, h);
            var mask = BuildBlurMask(lap, threshold);
            return FindConnectedComponents(mask, w, h);
        }

        private static double ComputeNoise(double[] intensities, int w, int h)
        {
            double sum = 0;
            int count = 0;

            object lockObj = new object();
            Parallel.For(1, h - 1, () => (localSum: 0.0, localCount: 0), (y, _, local) =>
            {
                int row = y * w;
                for (int x = 1; x < w - 1; x++)
                {
                    int idx = row + x;
                    double center = intensities[idx];
                    double neighborSum =
                        intensities[idx - w - 1] + intensities[idx - w] + intensities[idx - w + 1] +
                        intensities[idx - 1] + intensities[idx + 1] +
                        intensities[idx + w - 1] + intensities[idx + w] + intensities[idx + w + 1];
                    double mean = neighborSum / 8.0;
                    double diff = center - mean;
                    local.localSum += diff * diff;
                    local.localCount++;
                }

                return local;
            }, local =>
            {
                lock (lockObj)
                {
                    sum += local.localSum;
                    count += local.localCount;
                }
            });

            return sum / count;
        }

        private static double ComputeBandingScore(double[] intensities, int w, int h)
        {
            var rowMeans = new double[h];
            var colMeans = new double[w];
            double sum = 0, sumSq = 0;

            for (int y = 0; y < h; y++)
            {
                int row = y * w;
                for (int x = 0; x < w; x++)
                {
                    double val = intensities[row + x];
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

        private static bool HasGlare(double[] intensities, int brightThreshold, int areaThreshold, out int glareArea)
        {
            var mask = BuildGlareMask(intensities, brightThreshold);
            return HasGlare(mask, areaThreshold, out glareArea);
        }

        private static bool HasGlare(byte[] mask, int areaThreshold, out int glareArea)
        {
            int count = 0;
            foreach (byte b in mask)
            {
                if (b != 0)
                    count++;
            }
            glareArea = count;
            return glareArea > areaThreshold;
        }

        private static byte[] BuildGlareMask(double[] intensities, int brightThreshold)
        {
            var mask = new byte[intensities.Length];
            Parallel.For(0, intensities.Length, i =>
            {
                mask[i] = intensities[i] >= brightThreshold ? (byte)255 : (byte)0;
            });
            return mask;
        }

        private static SKBitmap CreateGlareHeatmap(byte[] mask, int w, int h)
        {
            var map = new SKBitmap(w, h, SKColorType.Gray8, SKAlphaType.Opaque);
            mask.AsSpan().CopyTo(map.GetPixelSpan());
            return map;
        }

        private static SKBitmap CreateGlareHeatmap(double[] intensities, int w, int h, int brightThreshold)
        {
            var mask = BuildGlareMask(intensities, brightThreshold);
            return CreateGlareHeatmap(mask, w, h);
        }

        private static List<SKRectI> FindGlareRegions(double[] intensities, int w, int h, int brightThreshold)
        {
            var mask = BuildGlareMask(intensities, brightThreshold);
            return FindConnectedComponents(mask, w, h);
        }

        private static double Intensity(SKColor p)
        {
            return (p.Red + p.Green + p.Blue) / 3.0;
        }

        private static double[] GetIntensityBuffer(SKBitmap image)
        {
            var pixels = image.Pixels;
            var buffer = new double[pixels.Length];
            Parallel.For(0, pixels.Length, i =>
            {
                buffer[i] = Intensity(pixels[i]);
            });
            return buffer;
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
            var intensities = GetIntensityBuffer(image);
            return HasGlare(intensities, brightThreshold, areaThreshold, out glareArea);
        }

        /// <summary>
        /// Creates a heatmap highlighting glare pixels.
        /// </summary>
        public SKBitmap CreateGlareHeatmap(SKBitmap image, int brightThreshold)
        {
            if (image == null) throw new ArgumentNullException(nameof(image));
            var intensities = GetIntensityBuffer(image);
            return CreateGlareHeatmap(intensities, image.Width, image.Height, brightThreshold);
        }

        /// <summary>
        /// Finds bounding boxes of glare regions based on a brightness threshold.
        /// </summary>
        public List<SKRectI> FindGlareRegions(SKBitmap image, int brightThreshold)
        {
            if (image == null) throw new ArgumentNullException(nameof(image));
            var intensities = GetIntensityBuffer(image);
            return FindGlareRegions(intensities, image.Width, image.Height, brightThreshold);
        }

        private static SKBitmap Resize(SKBitmap src, int width, int height)
        {
            var resized = new SKBitmap(width, height, src.ColorType, src.AlphaType);
            src.ScalePixels(resized, SKFilterQuality.Medium);
            return resized;
        }

        private static List<SKRectI> ScaleRects(IReadOnlyList<SKRectI> rects, double scaleX, double scaleY)
        {
            var scaled = new List<SKRectI>(rects.Count);
            foreach (var r in rects)
            {
                scaled.Add(new SKRectI(
                    (int)Math.Round(r.Left * scaleX),
                    (int)Math.Round(r.Top * scaleY),
                    (int)Math.Round(r.Right * scaleX),
                    (int)Math.Round(r.Bottom * scaleY)));
            }
            return scaled;
        }

        private static List<SKRectI> FindConnectedComponents(byte[] mask, int w, int h)
        {
            var visited = new bool[w, h];
            var rects = new List<SKRectI>();

            int[] dx = new[] { 1, -1, 0, 0 };
            int[] dy = new[] { 0, 0, 1, -1 };

            for (int y = 0; y < h; y++)
            {
                int row = y * w;
                for (int x = 0; x < w; x++)
                {
                    int idx = row + x;
                    if (visited[x, y]) continue;
                    if (mask[idx] == 0) continue;

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
                            if (nx < 0 || ny < 0 || nx >= w || ny >= h)
                                continue;
                            if (visited[nx, ny])
                                continue;
                            int nidx = ny * w + nx;
                            if (mask[nidx] == 0)
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

            double scale = Math.Clamp(settings.ProcessingScale, 0.1, 1.0);
            using var scaled = scale < 1.0 ? Resize(image, (int)(image.Width * scale), (int)(image.Height * scale)) : null;
            var src = scaled ?? image;

            var intensities = GetIntensityBuffer(src);
            int w = src.Width;
            int h = src.Height;

            var lap = BuildLaplacian(intensities, w, h, out double blurScore);
            var glareMask = BuildGlareMask(intensities, settings.BrightThreshold);

            double motionScore = ComputeMotionBlurScore(intensities, w, h);
            double noiseScore = ComputeNoise(intensities, w, h);
            double bandingScore = ComputeBandingScore(intensities, w, h);

            var result = new DocumentQualityResult
            {
                BrisqueScore = ComputeBrisqueScore(intensities),
                IsBlurry = blurScore < settings.BlurThreshold,
                BlurScore = blurScore,
                HasMotionBlur = motionScore > settings.MotionBlurThreshold,
                MotionBlurScore = motionScore,
                HasGlare = HasGlare(glareMask, settings.AreaThreshold, out var area),
                GlareArea = area,
                IsWellExposed = IsWellExposed(src, settings.ExposureMin, settings.ExposureMax, out var exposure),
                Exposure = exposure,
                HasLowContrast = HasLowContrast(src, settings.ContrastMin, out var contrast),
                Contrast = contrast,
                HasColorDominance = HasColorDominance(src, settings.DominanceThreshold, out var dom),
                ColorDominance = dom,
                HasNoise = noiseScore > settings.NoiseThreshold,
                Noise = noiseScore,
                HasBanding = bandingScore > settings.BandingThreshold,
                BandingScore = bandingScore
            };

            if (settings.GenerateHeatmaps)
            {
                result.BlurHeatmap = CreateBlurHeatmap(lap, w, h);
                result.GlareHeatmap = CreateGlareHeatmap(glareMask, w, h);

                var blurMask = BuildBlurMask(lap, settings.BlurThreshold);

                result.BlurRegions = FindConnectedComponents(blurMask, w, h);
                result.GlareRegions = FindConnectedComponents(glareMask, w, h);

                if (scale < 1.0)
                {
                    result.BlurHeatmap = Resize(result.BlurHeatmap, image.Width, image.Height);
                    result.GlareHeatmap = Resize(result.GlareHeatmap, image.Width, image.Height);
                    double sx = image.Width / (double)w;
                    double sy = image.Height / (double)h;
                    result.BlurRegions = ScaleRects(result.BlurRegions!, sx, sy);
                    result.GlareRegions = ScaleRects(result.GlareRegions!, sx, sy);
                }
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

