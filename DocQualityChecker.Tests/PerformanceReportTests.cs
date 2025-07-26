using DocQualityChecker;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using Xunit;

namespace DocQualityChecker.Tests
{
    public class PerformanceReportTests
    {
        private record ReportEntry(
            string Image,
            double Brisque,
            double Blur,
            double MotionBlur,
            double Glare,
            double Exposure,
            double Contrast,
            double ColorDominance,
            double Noise,
            double Banding,
            double BlurHeatmap,
            double GlareHeatmap,
            double BlurRegions,
            double GlareRegions,
            double Total);

        [Fact]
        public void GeneratePerformanceReport()
        {
            string root = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "docs", "dataset_samples");
            var imagePaths = Directory.EnumerateFiles(root, "*.*", SearchOption.AllDirectories)
                .Where(p => p.EndsWith(".jpg") || p.EndsWith(".png"))
                .Where(p => !p.Contains("heatmap") && !p.Contains("_metrics") && !p.EndsWith("performance_report.json"));

            var checker = new DocumentQualityChecker();
            var settings = new QualitySettings { GenerateHeatmaps = true };
            var results = new List<ReportEntry>();
            var sw = new Stopwatch();

            foreach (var path in imagePaths.OrderBy(p => p))
            {
                using var img = SKBitmap.Decode(path);
                sw.Restart();
                checker.ComputeBrisqueScore(img);
                double brisque = sw.Elapsed.TotalMilliseconds;

                double blurScore;
                sw.Restart();
                checker.IsBlurry(img, settings.BlurThreshold, out blurScore);
                double blur = sw.Elapsed.TotalMilliseconds;

                double motionScore;
                sw.Restart();
                checker.HasMotionBlur(img, settings.MotionBlurThreshold, out motionScore);
                double motionBlur = sw.Elapsed.TotalMilliseconds;

                int area;
                sw.Restart();
                checker.HasGlare(img, settings.BrightThreshold, settings.AreaThreshold, out area);
                double glare = sw.Elapsed.TotalMilliseconds;

                double exposure;
                sw.Restart();
                checker.IsWellExposed(img, settings.ExposureMin, settings.ExposureMax, out exposure);
                double exp = sw.Elapsed.TotalMilliseconds;

                double contrast;
                sw.Restart();
                checker.HasLowContrast(img, settings.ContrastMin, out contrast);
                double cont = sw.Elapsed.TotalMilliseconds;

                double dom;
                sw.Restart();
                checker.HasColorDominance(img, settings.DominanceThreshold, out dom);
                double colorDom = sw.Elapsed.TotalMilliseconds;

                double noise;
                sw.Restart();
                checker.HasNoise(img, settings.NoiseThreshold, out noise);
                double noi = sw.Elapsed.TotalMilliseconds;

                double band;
                sw.Restart();
                checker.HasBanding(img, settings.BandingThreshold, out band);
                double banding = sw.Elapsed.TotalMilliseconds;

                sw.Restart();
                checker.CreateBlurHeatmap(img);
                double blurMap = sw.Elapsed.TotalMilliseconds;

                sw.Restart();
                checker.CreateGlareHeatmap(img, settings.BrightThreshold);
                double glareMap = sw.Elapsed.TotalMilliseconds;

                sw.Restart();
                checker.FindBlurRegions(img, settings.BlurThreshold);
                double blurRegions = sw.Elapsed.TotalMilliseconds;

                sw.Restart();
                checker.FindGlareRegions(img, settings.BrightThreshold);
                double glareRegions = sw.Elapsed.TotalMilliseconds;

                sw.Restart();
                checker.CheckQuality(img, settings);
                double total = sw.Elapsed.TotalMilliseconds;

                results.Add(new ReportEntry(Path.GetRelativePath(root, path), brisque, blur, motionBlur, glare, exp, cont,
                                             colorDom, noi, banding, blurMap, glareMap, blurRegions, glareRegions, total));
            }

            string json = JsonSerializer.Serialize(results, new JsonSerializerOptions { WriteIndented = true });
            string outPath = Path.Combine(root, "performance_report.json");
            File.WriteAllText(outPath, json);

            Assert.True(File.Exists(outPath));
        }
    }
}
