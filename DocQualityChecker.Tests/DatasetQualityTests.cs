using System.IO;
using DocQualityChecker;
using OpenCvSharp;
using Xunit;

namespace DocQualityChecker.Tests
{
    public class DatasetQualityTests
    {
        [Fact]
        public void ComputeQuality_OnDatasetImages()
        {
            var datasetDir = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "dataset");
            if (!Directory.Exists(datasetDir))
                return;

            foreach (var file in Directory.EnumerateFiles(datasetDir))
            {
                if (!(file.EndsWith(".jpg") || file.EndsWith(".jpeg") || file.EndsWith(".png")))
                    continue;

                using var img = Cv2.ImRead(file);
                var checker = new DocumentQualityChecker("../DocQualityChecker/Models/brisque_model_live.yml", "../DocQualityChecker/Models/brisque_range_live.yml");
                var settings = new QualitySettings();
                var result = checker.CheckQuality(img, settings);
                Assert.InRange(result.BrisqueScore, 0.0, double.MaxValue);
            }
        }
    }
}
