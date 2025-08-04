using System;
using System.IO;
using System.Text.Json;
using DocQualityChecker;
using SkiaSharp;

if (args.Length >= 2 && args[0] == "--json")
{
    var path = args[1];
    using var bmp = SKBitmap.Decode(path);
    var checker = new DocumentQualityChecker();
    var settings = new QualitySettings();
    var res = checker.CheckQuality(bmp, settings);
    var obj = new
    {
        res.BlurScore,
        res.MotionBlurScore,
        res.GlareArea,
        res.Exposure,
        res.Contrast,
        res.Noise,
        res.ColorDominance,
        res.BandingScore,
        res.BrisqueScore,
        res.IsBlurry,
        res.HasGlare,
        res.IsWellExposed,
        res.HasLowContrast,
        res.HasNoise,
        res.HasColorDominance,
        res.HasBanding
    };
    Console.WriteLine(JsonSerializer.Serialize(obj));
}
else
{
    Console.Error.WriteLine("Usage: dotnet DocQualityChecker.dll --json <image>");
}
