using DocQualityChecker;
using OpenCvSharp;
using Xunit;

namespace DocQualityChecker.Tests
{
    public class DocumentQualityCheckerTests
    {
        private DocumentQualityChecker CreateChecker() => new DocumentQualityChecker("../DocQualityChecker/Models/brisque_model_live.yml", "../DocQualityChecker/Models/brisque_range_live.yml");

        private Mat CreateBaseImage()
        {
            var img = new Mat(new Size(200, 200), MatType.CV_8UC3, Scalar.All(255));
            Cv2.PutText(img, "ID", new Point(50, 100), HersheyFonts.HersheySimplex, 1.0, Scalar.Black, 2);
            return img;
        }

        [Fact]
        public void HighQualityImage_IsValid()
        {
            using var img = CreateBaseImage();
            var checker = CreateChecker();
            var settings = new QualitySettings();
            var result = checker.CheckQuality(img, settings);
            Assert.True(result.IsValidDocument);
        }

        [Fact]
        public void BlurryImage_IsInvalid()
        {
            using var img = CreateBaseImage();
            Cv2.GaussianBlur(img, img, new Size(21, 21), 0);
            var checker = CreateChecker();
            var settings = new QualitySettings();
            var result = checker.CheckQuality(img, settings);
            Assert.False(result.IsValidDocument);
            Assert.True(result.IsBlurry);
        }

        [Fact]
        public void GlareImage_IsInvalid()
        {
            using var img = CreateBaseImage();
            // Add bright rectangle simulating glare
            Cv2.Rectangle(img, new Rect(60, 60, 50, 50), new Scalar(255, 255, 255), -1);
            var checker = CreateChecker();
            var settings = new QualitySettings();
            var result = checker.CheckQuality(img, settings);
            Assert.False(result.IsValidDocument);
            Assert.True(result.HasGlare);
        }

        [Fact]
        public void HighBrisqueScore_IsInvalid()
        {
            using var img = CreateBaseImage();
            var checker = CreateChecker();
            var settings = new QualitySettings { BrisqueMax = 0 }; // Force failure
            var result = checker.CheckQuality(img, settings);
            Assert.False(result.IsValidDocument);
        }
    }
}
