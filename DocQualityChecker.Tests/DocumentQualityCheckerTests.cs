using DocQualityChecker;
using SkiaSharp;
using Xunit;

namespace DocQualityChecker.Tests
{
    public class DocumentQualityCheckerTests
    {
        private DocumentQualityChecker CreateChecker() => new DocumentQualityChecker();

        private SKBitmap CreateBaseImage()
        {
            var bmp = new SKBitmap(200, 200);
            using var canvas = new SKCanvas(bmp);
            canvas.Clear(new SKColor(200, 200, 200));
            using var paint = new SKPaint { Color = SKColors.Black };
            canvas.DrawRect(new SKRect(40, 80, 160, 120), paint);
            canvas.Flush();
            return bmp;
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
            using var blurred = new SKBitmap(img.Width, img.Height);
            using (var canvas = new SKCanvas(blurred))
            using (var paint = new SKPaint { ImageFilter = SKImageFilter.CreateBlur(10, 10) })
            {
                canvas.DrawBitmap(img, 0, 0, paint);
                canvas.Flush();
            }
            var checker = CreateChecker();
            var settings = new QualitySettings();
            var result = checker.CheckQuality(blurred, settings);
            Assert.False(result.IsValidDocument);
            Assert.True(result.IsBlurry);
        }

        [Fact]
        public void GlareImage_IsInvalid()
        {
            using var img = CreateBaseImage();
            using (var canvas = new SKCanvas(img))
            using (var paint = new SKPaint { Color = SKColors.White })
            {
                canvas.DrawRect(new SKRect(60, 60, 110, 110), paint);
                canvas.Flush();
            }
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

        [Fact]
        public void Heatmaps_AreGenerated_WhenRequested()
        {
            using var img = CreateBaseImage();
            using (var canvas = new SKCanvas(img))
            using (var paint = new SKPaint { Color = SKColors.White })
            {
                canvas.DrawRect(new SKRect(60, 60, 110, 110), paint);
                canvas.Flush();
            }

            var checker = CreateChecker();
            var settings = new QualitySettings { GenerateHeatmaps = true };
            var result = checker.CheckQuality(img, settings);

            Assert.NotNull(result.BlurHeatmap);
            Assert.NotNull(result.GlareHeatmap);
            var glarePixel = result.GlareHeatmap!.GetPixel(80, 80);
            Assert.Equal(255, glarePixel.Red);
        }

        [Fact]
        public void BlurHeatmap_HasValues()
        {
            using var img = CreateBaseImage();
            var checker = CreateChecker();
            var settings = new QualitySettings { GenerateHeatmaps = true };
            var result = checker.CheckQuality(img, settings);

            Assert.NotNull(result.BlurHeatmap);
            // Expect some edge values around the drawn rectangle
            var pixel = result.BlurHeatmap!.GetPixel(40, 100);
            Assert.True(pixel.Red > 0);
        }
    }
}
