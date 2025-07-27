using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using DocQualityChecker;
using SkiaSharp;

if (args.Length < 1)
{
    Console.WriteLine("Usage: PerformanceSampler <image|directory> [scale]");
    return;
}

string path = args[0];
double scale = args.Length > 1 && double.TryParse(args[1], out var s) ? s : 1.0;
var checker = new DocumentQualityChecker();
var settings = new QualitySettings { GenerateHeatmaps = true, ProcessingScale = scale };
var sw = new Stopwatch();

IEnumerable<string> images;
if (File.Exists(path))
    images = new[] { path };
else if (Directory.Exists(path))
    images = Directory.EnumerateFiles(path).Where(f => f.EndsWith(".jpg") || f.EndsWith(".png"));
else
{
    Console.WriteLine($"Path not found: {path}");
    return;
}

foreach (var imgPath in images)
{
    using var img = SKBitmap.Decode(imgPath);
    var timings = new Dictionary<string, double>();

    sw.Restart();
    double brisqueScore = checker.ComputeBrisqueScore(img);
    timings["Brisque"] = sw.Elapsed.TotalMilliseconds;

    sw.Restart();
    bool isBlurry = checker.IsBlurry(img, settings.BlurThreshold, out double blurScore);
    timings["Blur"] = sw.Elapsed.TotalMilliseconds;

    sw.Restart();
    bool hasMotionBlur = checker.HasMotionBlur(img, settings.MotionBlurThreshold, out double motionScore);
    timings["MotionBlur"] = sw.Elapsed.TotalMilliseconds;

    sw.Restart();
    bool hasGlare = checker.HasGlare(img, settings.BrightThreshold, settings.AreaThreshold, out int glareArea);
    timings["Glare"] = sw.Elapsed.TotalMilliseconds;

    sw.Restart();
    bool isWellExposed = checker.IsWellExposed(img, settings.ExposureMin, settings.ExposureMax, out double exposure);
    timings["Exposure"] = sw.Elapsed.TotalMilliseconds;

    sw.Restart();
    bool hasLowContrast = checker.HasLowContrast(img, settings.ContrastMin, out double contrast);
    timings["Contrast"] = sw.Elapsed.TotalMilliseconds;

    sw.Restart();
    bool hasColorDom = checker.HasColorDominance(img, settings.DominanceThreshold, out double colorDom);
    timings["ColorDominance"] = sw.Elapsed.TotalMilliseconds;

    sw.Restart();
    bool hasNoise = checker.HasNoise(img, settings.NoiseThreshold, out double noise);
    timings["Noise"] = sw.Elapsed.TotalMilliseconds;

    sw.Restart();
    bool hasBanding = checker.HasBanding(img, settings.BandingThreshold, out double bandingScore);
    timings["Banding"] = sw.Elapsed.TotalMilliseconds;

    sw.Restart();
    var blurMap = checker.CreateBlurHeatmap(img);
    timings["BlurHeatmap"] = sw.Elapsed.TotalMilliseconds;

    sw.Restart();
    var glareMap = checker.CreateGlareHeatmap(img, settings.BrightThreshold);
    timings["GlareHeatmap"] = sw.Elapsed.TotalMilliseconds;

    sw.Restart();
    var blurRegions = checker.FindBlurRegions(img, settings.BlurThreshold);
    timings["BlurRegions"] = sw.Elapsed.TotalMilliseconds;

    sw.Restart();
    var glareRegions = checker.FindGlareRegions(img, settings.BrightThreshold);
    timings["GlareRegions"] = sw.Elapsed.TotalMilliseconds;

    sw.Restart();
    var result = checker.CheckQuality(img, settings);
    timings["Total"] = sw.Elapsed.TotalMilliseconds;

    string dir = Path.GetDirectoryName(imgPath) ?? ".";
    string name = Path.GetFileNameWithoutExtension(imgPath);

    // save metrics txt
    var lines = new List<string>
    {
        $"BrisqueScore: {brisqueScore:F2}",
        $"BlurScore: {blurScore:F2}",
        $"IsBlurry: {isBlurry}",
        $"GlareArea: {glareArea}",
        $"HasGlare: {hasGlare}",
        $"Exposure: {exposure:F2}",
        $"IsWellExposed: {isWellExposed}",
        $"Contrast: {contrast:F2}",
        $"HasLowContrast: {hasLowContrast}",
        $"ColorDominance: {colorDom:F2}",
        $"HasColorDominance: {hasColorDom}",
        $"Noise: {noise:F2}",
        $"HasNoise: {hasNoise}",
        $"BandingScore: {bandingScore:F2}",
        $"HasBanding: {hasBanding}",
        $"IsValidDocument: {result.IsValidDocument}"
    };
    File.WriteAllLines(Path.Combine(dir, name + "_metrics.txt"), lines);

    // save timings json
    string json = JsonSerializer.Serialize(timings, new JsonSerializerOptions { WriteIndented = true });
    File.WriteAllText(Path.Combine(dir, name + "_timings.json"), json);

    // save heatmaps
    using (var fs = File.OpenWrite(Path.Combine(dir, name + "_blur_heatmap.png")))
        blurMap.Encode(fs, SKEncodedImageFormat.Png, 100);
    using (var fs = File.OpenWrite(Path.Combine(dir, name + "_glare_heatmap.png")))
        glareMap.Encode(fs, SKEncodedImageFormat.Png, 100);

    Console.WriteLine($"Processed {Path.GetFileName(imgPath)}");
}
