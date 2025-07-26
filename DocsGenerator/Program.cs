using DocQualityChecker;
using SkiaSharp;

string outputDir = Path.Combine("docs", "images");
Directory.CreateDirectory(outputDir);

void SaveBitmap(SKBitmap bmp, string fileName)
{
    using var fs = File.OpenWrite(Path.Combine(outputDir, fileName));
    bmp.Encode(fs, SKEncodedImageFormat.Png, 100);
}

void DrawBoxes(SKBitmap bmp, IEnumerable<SKRectI>? boxes, SKColor color)
{
    if (boxes == null) return;
    using var canvas = new SKCanvas(bmp);
    using var paint = new SKPaint { Color = color, Style = SKPaintStyle.Stroke, StrokeWidth = 2 };
    foreach (var b in boxes)
        canvas.DrawRect(b, paint);
    canvas.Flush();
}

SKBitmap CreateBaseImage()
{
    var bmp = new SKBitmap(200, 200);
    using var canvas = new SKCanvas(bmp);
    canvas.Clear(new SKColor(200, 200, 200));
    using var paint = new SKPaint { Color = SKColors.Black };
    canvas.DrawRect(new SKRect(40, 80, 160, 120), paint);
    canvas.Flush();
    return bmp;
}

SKBitmap CreateBlurryImage()
{
    using var baseImg = CreateBaseImage();
    var blurred = new SKBitmap(baseImg.Width, baseImg.Height);
    using var canvas = new SKCanvas(blurred);
    using var paint = new SKPaint { ImageFilter = SKImageFilter.CreateBlur(10, 10) };
    canvas.DrawBitmap(baseImg, 0, 0, paint);
    canvas.Flush();
    return blurred;
}

SKBitmap CreateGlareImage()
{
    var img = CreateBaseImage();
    using var canvas = new SKCanvas(img);
    using var paint = new SKPaint { Color = SKColors.White };
    canvas.DrawRect(new SKRect(60, 60, 110, 110), paint);
    canvas.Flush();
    return img;
}

SKBitmap CreateUnderExposedImage()
{
    var bmp = new SKBitmap(200, 200);
    using var canvas = new SKCanvas(bmp);
    canvas.Clear(new SKColor(20, 20, 20));
    canvas.Flush();
    return bmp;
}

SKBitmap CreateLowContrastImage()
{
    var bmp = new SKBitmap(200, 200);
    using var canvas = new SKCanvas(bmp);
    using var paint = new SKPaint { Color = new SKColor(125, 125, 125) };
    canvas.Clear(new SKColor(120, 120, 120));
    canvas.DrawRect(new SKRect(40, 80, 160, 120), paint);
    canvas.Flush();
    return bmp;
}

SKBitmap CreateColorDominanceImage()
{
    var bmp = new SKBitmap(200, 200);
    using var canvas = new SKCanvas(bmp);
    canvas.Clear(new SKColor(255, 0, 0));
    canvas.Flush();
    return bmp;
}

SKBitmap CreateNoisyImage()
{
    var img = CreateBaseImage();
    var rnd = new Random(0);
    for (int y = 0; y < img.Height; y++)
        for (int x = 0; x < img.Width; x++)
            if (rnd.NextDouble() < 0.3)
            {
                byte val = (byte)rnd.Next(256);
                img.SetPixel(x, y, new SKColor(val, val, val));
            }
    return img;
}

DocumentQualityResult ProcessImage(string name, SKBitmap img, QualitySettings settings)
{
    var checker = new DocumentQualityChecker();
    var result = checker.CheckQuality(img, settings);
    SaveBitmap(img, $"{name}_original.png");
    if (result.BlurHeatmap != null)
        SaveBitmap(result.BlurHeatmap, $"{name}_blur_heatmap.png");
    if (result.GlareHeatmap != null)
        SaveBitmap(result.GlareHeatmap, $"{name}_glare_heatmap.png");
    using var boxed = img.Copy();
    DrawBoxes(boxed, result.BlurRegions, SKColors.Red);
    DrawBoxes(boxed, result.GlareRegions, SKColors.Lime);
    SaveBitmap(boxed, $"{name}_bbox.png");
    using var sw = new StreamWriter(Path.Combine(outputDir, $"{name}_metrics.txt"));
    sw.WriteLine($"BrisqueScore: {result.BrisqueScore:F2}");
    sw.WriteLine($"BlurScore: {result.BlurScore:F2}");
    sw.WriteLine($"IsBlurry: {result.IsBlurry}");
    sw.WriteLine($"GlareArea: {result.GlareArea}");
    sw.WriteLine($"HasGlare: {result.HasGlare}");
    sw.WriteLine($"Exposure: {result.Exposure:F2}");
    sw.WriteLine($"IsWellExposed: {result.IsWellExposed}");
    sw.WriteLine($"Contrast: {result.Contrast:F2}");
    sw.WriteLine($"HasLowContrast: {result.HasLowContrast}");
    sw.WriteLine($"ColorDominance: {result.ColorDominance:F2}");
    sw.WriteLine($"HasColorDominance: {result.HasColorDominance}");
    sw.WriteLine($"Noise: {result.Noise:F2}");
    sw.WriteLine($"HasNoise: {result.HasNoise}");
    sw.WriteLine($"IsValidDocument: {result.IsValidDocument}");
    return result;
}

void RunScenario(string name, Func<SKBitmap> creator)
{
    using var img = creator();
    var settings = new QualitySettings { GenerateHeatmaps = true };
    ProcessImage(name, img, settings);
}

RunScenario("high_quality", CreateBaseImage);
RunScenario("blurry", CreateBlurryImage);
RunScenario("glare", CreateGlareImage);

// high brisque scenario by setting threshold to 0
{
    using var img = CreateBaseImage();
    var settings = new QualitySettings { GenerateHeatmaps = true, BrisqueMax = 0 };
    ProcessImage("high_brisque", img, settings);
}

RunScenario("underexposed", CreateUnderExposedImage);
RunScenario("low_contrast", CreateLowContrastImage);
RunScenario("color_cast", CreateColorDominanceImage);
RunScenario("noise", CreateNoisyImage);

Console.WriteLine($"Images written to {outputDir}");
