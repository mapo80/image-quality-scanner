using System;
using OpenCvSharp;
using OpenCvSharp.Quality;

namespace DocQualityChecker
{
    /// <summary>
    /// Provides methods to evaluate image quality for identity documents.
    /// </summary>
    public class DocumentQualityChecker
    {
        private readonly string _modelPath;
        private readonly string _rangePath;

        /// <summary>
        /// Initializes a new instance of <see cref="DocumentQualityChecker"/>.
        /// </summary>
        /// <param name="modelPath">Path to BRISQUE model file.</param>
        /// <param name="rangePath">Path to BRISQUE range file.</param>
        public DocumentQualityChecker(string modelPath = "Models/brisque_model_live.yml", string rangePath = "Models/brisque_range_live.yml")
        {
            _modelPath = modelPath;
            _rangePath = rangePath;
        }

        /// <summary>
        /// Computes the BRISQUE score of the provided image.
        /// </summary>
        /// <param name="image">Source image.</param>
        /// <returns>BRISQUE score as a double.</returns>
        /// <exception cref="ArgumentNullException">Thrown when image is null.</exception>
        public double ComputeBrisqueScore(Mat image)
        {
            if (image == null) throw new ArgumentNullException(nameof(image));
            using var brisque = QualityBRISQUE.Create(_modelPath, _rangePath);
            var score = brisque.Compute(image);
            return score[0];
        }

        /// <summary>
        /// Checks if the image is blurry using the variance of the Laplacian.
        /// </summary>
        /// <param name="image">Source image.</param>
        /// <param name="threshold">Blur threshold.</param>
        /// <param name="blurScore">Returned blur score.</param>
        /// <returns>True if image is considered blurry.</returns>
        public bool IsBlurry(Mat image, double threshold, out double blurScore)
        {
            if (image == null) throw new ArgumentNullException(nameof(image));

            using var gray = image.CvtColor(ColorConversionCodes.BGR2GRAY);
            using var laplacian = gray.Laplacian(MatType.CV_64F);
            Cv2.MeanStdDev(laplacian, out _, out var stdDev);
            blurScore = stdDev.Val0 * stdDev.Val0;
            return blurScore < threshold;
        }

        /// <summary>
        /// Detects presence of glare in the image.
        /// </summary>
        /// <param name="image">Source image.</param>
        /// <param name="brightThreshold">Brightness threshold used to segment glare.</param>
        /// <param name="areaThreshold">Minimum area of glare region.</param>
        /// <param name="glareArea">Total area of detected glare.</param>
        /// <returns>True if glare is detected above the threshold.</returns>
        public bool HasGlare(Mat image, int brightThreshold, int areaThreshold, out int glareArea)
        {
            if (image == null) throw new ArgumentNullException(nameof(image));

            using var gray = image.CvtColor(ColorConversionCodes.BGR2GRAY);
            using var mask = gray.Threshold(brightThreshold, 255, ThresholdTypes.Binary);
            // Remove small artifacts
            var kernel = Cv2.GetStructuringElement(MorphShapes.Rect, new Size(3, 3));
            Cv2.MorphologyEx(mask, mask, MorphTypes.Open, kernel);
            glareArea = Cv2.CountNonZero(mask);
            return glareArea > areaThreshold;
        }

        /// <summary>
        /// Performs all quality checks on the provided image.
        /// </summary>
        /// <param name="image">Source image.</param>
        /// <param name="settings">Threshold settings.</param>
        /// <returns>Aggregated result of all quality checks.</returns>
        public DocumentQualityResult CheckQuality(Mat image, QualitySettings settings)
        {
            if (image == null) throw new ArgumentNullException(nameof(image));
            if (settings == null) throw new ArgumentNullException(nameof(settings));

            var result = new DocumentQualityResult();
            result.BrisqueScore = ComputeBrisqueScore(image);
            result.IsBlurry = IsBlurry(image, settings.BlurThreshold, out var blurScore);
            result.BlurScore = blurScore;
            result.HasGlare = HasGlare(image, settings.BrightThreshold, settings.AreaThreshold, out var area);
            result.GlareArea = area;
            result.IsValidDocument = result.BrisqueScore <= settings.BrisqueMax &&
                                      !result.IsBlurry &&
                                      !result.HasGlare;
            return result;
        }
    }
}
