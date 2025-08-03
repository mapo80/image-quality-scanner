using System.Diagnostics;
using System.Globalization;
using DocQualityChecker;
using SkiaSharp;

string samplePath = "data/sample_50.txt";
string outDir = "data";
bool encode = false;

for (int i = 0; i < args.Length; i++)
{
    switch (args[i])
    {
        case "--sample":
            if (i + 1 < args.Length) samplePath = args[++i];
            break;
        case "--outDir":
            if (i + 1 < args.Length) outDir = args[++i];
            break;
        case "--encode":
            encode = true;
            break;
    }
}

var paths = File.ReadAllLines(samplePath)
    .Select(l => l.Trim())
    .Where(l => !string.IsNullOrWhiteSpace(l))
    .ToList();

var checker = new DocumentQualityChecker();
var settings = new QualitySettings();
var culture = CultureInfo.InvariantCulture;

Directory.CreateDirectory(outDir);
var perImagePath = Path.Combine(outDir, "metrics_per_image.csv");
using var perImageWriter = new StreamWriter(perImagePath);
perImageWriter.WriteLine("path,BlurScore,IsBlurry,MotionBlurScore,GlareArea,HasGlare,Exposure,IsWellExposed,Contrast,HasLowContrast,Noise,HasNoise,ColorDominance,HasColorDominance,BandingScore,BrisqueScore,ElapsedMs");

var encodedDir = Path.Combine(outDir, "encoded_samples");
if (encode) Directory.CreateDirectory(encodedDir);

var results = new List<(string Path, DocumentQualityResult Res, double Elapsed)>();
foreach (var p in paths)
{
    var sw = Stopwatch.StartNew();
    using var fs = File.OpenRead(p);
    using var bmp = SKBitmap.Decode(fs);
    var res = checker.CheckQuality(bmp, settings);
    sw.Stop();
    var elapsed = sw.Elapsed.TotalMilliseconds;
    results.Add((p, res, elapsed));

    perImageWriter.WriteLine(string.Join(',', new string[]
    {
        p,
        res.BlurScore.ToString(culture),
        res.IsBlurry.ToString(culture),
        res.MotionBlurScore.ToString(culture),
        res.GlareArea.ToString(culture),
        res.HasGlare.ToString(culture),
        res.Exposure.ToString(culture),
        res.IsWellExposed.ToString(culture),
        res.Contrast.ToString(culture),
        res.HasLowContrast.ToString(culture),
        res.Noise.ToString(culture),
        res.HasNoise.ToString(culture),
        res.ColorDominance.ToString(culture),
        res.HasColorDominance.ToString(culture),
        res.BandingScore.ToString(culture),
        res.BrisqueScore.ToString(culture),
        elapsed.ToString(culture)
    }));

    if (encode)
    {
        var bytes = File.ReadAllBytes(p);
        var name = Path.GetFileName(p) + ".base64";
        File.WriteAllText(Path.Combine(encodedDir, name), Convert.ToBase64String(bytes));
    }
}

perImageWriter.Flush();

int total = results.Count;

var summaryLines = new List<string>();
summaryLines.Add("Metric,PassRate,Mean,Std,Min,Max");

static double Mean(IEnumerable<double> vals) => vals.Average();
static double Std(IEnumerable<double> vals)
{
    var avg = vals.Average();
    var sum = vals.Sum(v => Math.Pow(v - avg, 2));
    return Math.Sqrt(sum / vals.Count());
}

// Boolean pass rates
double PassRate(Func<DocumentQualityResult, bool> predicate) =>
    results.Count == 0 ? 0 : results.Count(r => !predicate(r.Res)) / (double)results.Count;

summaryLines.Add($"IsBlurry,{PassRate(r => r.IsBlurry).ToString(culture)},,,,");
summaryLines.Add($"HasGlare,{PassRate(r => r.HasGlare).ToString(culture)},,,,");
summaryLines.Add($"HasNoise,{PassRate(r => r.HasNoise).ToString(culture)},,,,");
summaryLines.Add($"HasLowContrast,{PassRate(r => r.HasLowContrast).ToString(culture)},,,,");
summaryLines.Add($"HasColorDominance,{PassRate(r => r.HasColorDominance).ToString(culture)},,,,");
summaryLines.Add($"!IsWellExposed,{PassRate(r => !r.IsWellExposed).ToString(culture)},,,,");

// Numeric metrics
void AddNumeric(string name, Func<DocumentQualityResult, double> selector)
{
    var values = results.Select(r => selector(r.Res)).ToList();
    if (values.Count == 0) return;
    summaryLines.Add(string.Join(',', new string[]
    {
        name,
        "",
        Mean(values).ToString(culture),
        Std(values).ToString(culture),
        values.Min().ToString(culture),
        values.Max().ToString(culture)
    }));
}

AddNumeric("BlurScore", r => r.BlurScore);
AddNumeric("MotionBlurScore", r => r.MotionBlurScore);
AddNumeric("GlareArea", r => r.GlareArea);
AddNumeric("Exposure", r => r.Exposure);
AddNumeric("Contrast", r => r.Contrast);
AddNumeric("Noise", r => r.Noise);
AddNumeric("ColorDominance", r => r.ColorDominance);
AddNumeric("BandingScore", r => r.BandingScore);
AddNumeric("BrisqueScore", r => r.BrisqueScore);
AddNumeric("AvgProcessingTimeMs", r => 0); // placeholder, override below

var avgTime = results.Average(r => r.Elapsed);
summaryLines[summaryLines.FindIndex(l => l.StartsWith("AvgProcessingTimeMs"))] =
    $"AvgProcessingTimeMs,,{avgTime.ToString(culture)},,,";

File.WriteAllLines(Path.Combine(outDir, "metrics_summary.csv"), summaryLines);

foreach (var line in summaryLines.Skip(1))
    Console.WriteLine(line);
