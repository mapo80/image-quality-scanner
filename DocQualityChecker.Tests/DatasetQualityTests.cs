using System.Collections.Generic;
using System.IO;
using DocQualityChecker;
using OpenCvSharp;
using Xunit;

namespace DocQualityChecker.Tests
{
    public class DatasetQualityTests
    {
        public static IEnumerable<object[]> GetDatasetImages()
        {
            var datasetDir = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "dataset");
            if (!Directory.Exists(datasetDir))
                yield break;

            foreach (var file in Directory.EnumerateFiles(datasetDir))
            {
                if (file.EndsWith(".jpg") || file.EndsWith(".jpeg") || file.EndsWith(".png"))
                    yield return new object[] { file };
            }
        }

        [Theory]
        [MemberData(nameof(GetDatasetImages))]
        public void ComputeQuality_OnDatasetImages(string path)
        {
            if (!File.Exists(path))
                throw new FileNotFoundException(path);

            using var img = Cv2.ImRead(path);
            var checker = new DocumentQualityChecker("../DocQualityChecker/Models/brisque_model_live.yml", "../DocQualityChecker/Models/brisque_range_live.yml");
            var settings = new QualitySettings();
            var result = checker.CheckQuality(img, settings);

            Assert.InRange(result.BrisqueScore, 0.0, double.MaxValue);
        }
    }
}
