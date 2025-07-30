using DocQualityChecker;
using System;
using System.IO;
using System.Linq;
using Xunit;

namespace DocQualityChecker.Tests
{
    public class PdfDatasetTests
    {
        private static string GetPdfPath(string name) => Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "docs", "dataset_samples", "pdf", name);

        [Fact]
        public void BlurPdf_AllPages_Analyzed()
        {
            using var fs = File.OpenRead(GetPdfPath("blur.pdf"));
            var checker = new DocumentQualityChecker();
            var settings = new QualitySettings();
            var results = checker.CheckQuality(fs, settings).ToList();
            Assert.Equal(9, results.Count);
            Assert.True(results.Count(r => r.IsBlurry) >= 3);
        }

        [Fact]
        public void GlarePdf_AllPages_Analyzed()
        {
            using var fs = File.OpenRead(GetPdfPath("glare.pdf"));
            var checker = new DocumentQualityChecker();
            var settings = new QualitySettings();
            var results = checker.CheckQuality(fs, settings).ToList();
            Assert.Equal(9, results.Count);
            Assert.True(results.Count(r => r.HasGlare) >= 6);
        }
    }
}
