using DocQualityChecker;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel;
using SkiaSharp;
using System.Text;
using DocQualityChecker.Api.Models;
using System.Linq;

namespace DocQualityChecker.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class QualityController : ControllerBase
    {
        private readonly DocumentQualityChecker _checker = new();

        [HttpPost("check")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> Check([FromForm] QualityCheckRequest request)
        {
            if (request.Image == null || request.Image.Length == 0)
                return BadRequest("Image is required");

            using var ms = new MemoryStream();
            await request.Image.CopyToAsync(ms);
            using var bitmap = SKBitmap.Decode(ms.ToArray());
            if (bitmap == null)
                return BadRequest("Invalid image");

            var settings = request.Settings ?? new QualitySettings();
            var checks = request.Checks ?? new List<string>();
            if (checks.Count == 0)
            {
                checks = new List<string> { "Brisque", "Blur", "Glare", "Exposure", "Contrast", "ColorDominance", "Noise", "MotionBlur", "Banding" };
            }

            var response = new QualityCheckResponse();

            var needBlurHeatmap = settings.GenerateHeatmaps &&
                                   checks.Any(c => c.Equals("blur", StringComparison.InvariantCultureIgnoreCase));
            var needGlareHeatmap = settings.GenerateHeatmaps &&
                                    checks.Any(c => c.Equals("glare", StringComparison.InvariantCultureIgnoreCase));

            foreach (var c in checks)
            {
                switch (c.ToLowerInvariant())
                {
                    case "brisque":
                        double brisque = _checker.ComputeBrisqueScore(bitmap);
                        response.Results["BrisqueScore"] = brisque;
                        response.Results["BrisqueValid"] = brisque <= settings.BrisqueMax;
                        response.Explanations["Brisque"] = $"Varianza normalizzata {brisque:F2} (soglia {settings.BrisqueMax})";
                        break;
                    case "blur":
                        bool blurry = _checker.IsBlurry(bitmap, settings.BlurThreshold, out var blurScore);
                        response.Results["BlurScore"] = blurScore;
                        response.Results["IsBlurry"] = blurry;
                        response.Explanations["Blur"] = $"Varianza Laplaciano {blurScore:F2} (soglia {settings.BlurThreshold})";
                        if (needBlurHeatmap)
                        {
                            using var map = _checker.CreateBlurHeatmap(bitmap);
                            using var data = map.Encode(SKEncodedImageFormat.Png, 100);
                            response.BlurHeatmap = Convert.ToBase64String(data.ToArray());
                            var regions = _checker.FindBlurRegions(bitmap, settings.BlurThreshold)
                                                 .Select(r => new RegionDto { X = r.Left, Y = r.Top, Width = r.Width, Height = r.Height })
                                                 .ToList();
                            response.BlurRegions = regions;
                        }
                        break;
                    case "glare":
                        bool glare = _checker.HasGlare(bitmap, settings.BrightThreshold, settings.AreaThreshold, out var glareArea);
                        response.Results["GlareArea"] = glareArea;
                        response.Results["HasGlare"] = glare;
                        response.Explanations["Glare"] = $"Area pixel luminosi {glareArea} (soglia {settings.AreaThreshold})";
                        if (needGlareHeatmap)
                        {
                            using var map = _checker.CreateGlareHeatmap(bitmap, settings.BrightThreshold);
                            using var data = map.Encode(SKEncodedImageFormat.Png, 100);
                            response.GlareHeatmap = Convert.ToBase64String(data.ToArray());
                            var regions = _checker.FindGlareRegions(bitmap, settings.BrightThreshold)
                                                 .Select(r => new RegionDto { X = r.Left, Y = r.Top, Width = r.Width, Height = r.Height })
                                                 .ToList();
                            response.GlareRegions = regions;
                        }
                        break;
                    case "exposure":
                        bool exposed = _checker.IsWellExposed(bitmap, settings.ExposureMin, settings.ExposureMax, out var exposure);
                        response.Results["Exposure"] = exposure;
                        response.Results["IsWellExposed"] = exposed;
                        response.Explanations["Exposure"] = $"Luminanza media {exposure:F2} (range {settings.ExposureMin}-{settings.ExposureMax})";
                        break;
                    case "contrast":
                        bool lowContrast = _checker.HasLowContrast(bitmap, settings.ContrastMin, out var contrast);
                        response.Results["Contrast"] = contrast;
                        response.Results["HasLowContrast"] = lowContrast;
                        response.Explanations["Contrast"] = $"Deviazione standard {contrast:F2} (min {settings.ContrastMin})";
                        break;
                    case "colordominance":
                        bool dom = _checker.HasColorDominance(bitmap, settings.DominanceThreshold, out var dominance);
                        response.Results["ColorDominance"] = dominance;
                        response.Results["HasColorDominance"] = dom;
                        response.Explanations["ColorDominance"] = $"Rapporto canale dominante {dominance:F2} (soglia {settings.DominanceThreshold})";
                        break;
                    case "noise":
                        bool hasNoise = _checker.HasNoise(bitmap, settings.NoiseThreshold, out var noise);
                        response.Results["Noise"] = noise;
                        response.Results["HasNoise"] = hasNoise;
                        response.Explanations["Noise"] = $"Livello di rumore {noise:F2} (soglia {settings.NoiseThreshold})";
                        break;
                    case "motionblur":
                        bool motionBlur = _checker.HasMotionBlur(bitmap, settings.MotionBlurThreshold, out var mbScore);
                        response.Results["MotionBlurScore"] = mbScore;
                        response.Results["HasMotionBlur"] = motionBlur;
                        response.Explanations["MotionBlur"] = $"Rapporto gradienti {mbScore:F2} (soglia {settings.MotionBlurThreshold})";
                        break;
                    case "banding":
                        bool band = _checker.HasBanding(bitmap, settings.BandingThreshold, out var bscore);
                        response.Results["BandingScore"] = bscore;
                        response.Results["HasBanding"] = band;
                        response.Explanations["Banding"] = $"Varianza bande {bscore:F2} (soglia {settings.BandingThreshold})";
                        break;
                }
            }

            bool isValid = true;
            if (response.Results.TryGetValue("BrisqueValid", out var br) && br is bool b && !b) isValid = false;
            if (response.Results.TryGetValue("IsBlurry", out var bl) && bl is bool blb && blb) isValid = false;
            if (response.Results.TryGetValue("HasGlare", out var gl) && gl is bool glb && glb) isValid = false;
            if (response.Results.TryGetValue("IsWellExposed", out var exp) && exp is bool exb && !exb) isValid = false;
            if (response.Results.TryGetValue("HasLowContrast", out var lc) && lc is bool lcb && lcb) isValid = false;
            if (response.Results.TryGetValue("HasColorDominance", out var cd) && cd is bool cdb && cdb) isValid = false;
            if (response.Results.TryGetValue("HasNoise", out var n) && n is bool nb && nb) isValid = false;
            if (response.Results.TryGetValue("HasMotionBlur", out var mb) && mb is bool mbb && mbb) isValid = false;
            if (response.Results.TryGetValue("HasBanding", out var ba) && ba is bool bab && bab) isValid = false;

            response.IsValidDocument = isValid;
            return Ok(response);
        }
    }

    public class QualityCheckRequest
    {
        public IFormFile? Image { get; set; }

        /// <summary>
        /// List of checks to execute. Possible values: Brisque, Blur, Glare, Exposure,
        /// Contrast, ColorDominance, Noise, MotionBlur, Banding. If not provided all
        /// checks are executed.
        /// </summary>
        [DefaultValue(new[] { "Brisque", "Blur", "Glare", "Exposure", "Contrast", "ColorDominance", "Noise", "MotionBlur", "Banding" })]
        public List<string>? Checks { get; set; }

        /// <summary>
        /// Threshold values used to determine if the image passes each check.
        /// Any property not specified uses its default value.
        /// </summary>
        public QualitySettings? Settings { get; set; }
    }

    /// <summary>
    /// Response returned after running the selected quality checks.
    /// </summary>
    public class QualityCheckResponse
    {
        /// <summary>Computed values for each check.</summary>
        public Dictionary<string, object> Results { get; } = new();

        /// <summary>Human readable explanations for each check.</summary>
        public Dictionary<string, string> Explanations { get; } = new();

        /// <summary>Final evaluation based on the enabled checks.</summary>
        public bool IsValidDocument { get; set; }

        /// <summary>Base64 encoded blur heatmap when requested.</summary>
        public string? BlurHeatmap { get; set; }

        /// <summary>Base64 encoded glare heatmap when requested.</summary>
        public string? GlareHeatmap { get; set; }

        /// <summary>Detected blurry regions when heatmaps are generated.</summary>
        public List<RegionDto>? BlurRegions { get; set; }

        /// <summary>Detected glare regions when heatmaps are generated.</summary>
        public List<RegionDto>? GlareRegions { get; set; }
    }
}
