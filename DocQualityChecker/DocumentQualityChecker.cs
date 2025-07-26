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
                GlareArea = area
            };

            result.IsValidDocument = result.BrisqueScore <= settings.BrisqueMax &&
                                     !result.IsBlurry &&
                                     !result.HasGlare;

            return result;
        }
    }
}

