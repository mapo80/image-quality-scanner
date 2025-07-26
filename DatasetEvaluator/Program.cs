using DocQualityChecker;
using SkiaSharp;

if (args.Length < 1)
{
    Console.WriteLine("Usage: DatasetEvaluator <image|directory> [limit]");
    return;
}

string path = args[0];
int limit = args.Length > 1 && int.TryParse(args[1], out var l) ? l : int.MaxValue;

var checker = new DocumentQualityChecker();
var settings = new QualitySettings { GenerateHeatmaps = true };

if (File.Exists(path))
{
    Evaluate(path);
}
else if (Directory.Exists(path))
{
    string imagesDir = Path.Combine(path, "images");
    if (!Directory.Exists(imagesDir))
        imagesDir = path;

    int processed = 0;
    foreach (var imgPath in Directory.EnumerateFiles(imagesDir)
                                   .Where(f => f.EndsWith(".jpg") || f.EndsWith(".png")))
    {
        Evaluate(imgPath);
        if (++processed >= limit) break;
    }
}
else
{
    Console.WriteLine($"Path not found: {path}");
}

void Evaluate(string imgPath)
{
    using var img = SKBitmap.Decode(imgPath);
    var result = checker.CheckQuality(img, settings);

    Console.WriteLine($"File: {Path.GetFileName(imgPath)}");
    Console.WriteLine($"  BrisqueScore: {result.BrisqueScore:F2}");
    Console.WriteLine($"  BlurScore: {result.BlurScore:F2}");
    Console.WriteLine($"  IsBlurry: {result.IsBlurry}");
    Console.WriteLine($"  GlareArea: {result.GlareArea}");
    Console.WriteLine($"  HasGlare: {result.HasGlare}");
    Console.WriteLine($"  Exposure: {result.Exposure:F2}");
    Console.WriteLine($"  IsWellExposed: {result.IsWellExposed}");
    Console.WriteLine($"  Contrast: {result.Contrast:F2}");
    Console.WriteLine($"  HasLowContrast: {result.HasLowContrast}");
    Console.WriteLine($"  ColorDominance: {result.ColorDominance:F2}");
    Console.WriteLine($"  HasColorDominance: {result.HasColorDominance}");
    Console.WriteLine($"  Noise: {result.Noise:F2}");
    Console.WriteLine($"  HasNoise: {result.HasNoise}");
    Console.WriteLine($"  IsValidDocument: {result.IsValidDocument}");

    if (result.GlareHeatmap != null)
    {
        string dir = Path.GetDirectoryName(imgPath) ?? ".";
        string name = Path.GetFileNameWithoutExtension(imgPath);
        using var fs = File.OpenWrite(Path.Combine(dir, name + "_glare_heatmap.png"));
        result.GlareHeatmap.Encode(fs, SKEncodedImageFormat.Png, 100);
    }
}
