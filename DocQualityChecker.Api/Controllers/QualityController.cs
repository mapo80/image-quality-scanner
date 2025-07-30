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
            if ((request.Image == null && request.Pdf == null) ||
                (request.Image != null && request.Pdf != null))
            {
                return BadRequest("Provide an image or a PDF file");
            }

            var settings = request.Settings ?? new QualitySettings();
            var checks = request.Checks ?? new List<string>();
            if (checks.Count == 0)
            {
                checks = new List<string> { "Brisque", "Blur", "Glare", "Exposure", "Contrast", "ColorDominance", "Noise", "MotionBlur", "Banding" };
            }

            if (request.Image != null)
            {
                using var ms = new MemoryStream();
                await request.Image.CopyToAsync(ms);
                using var bitmap = SKBitmap.Decode(ms.ToArray());
                if (bitmap == null)
                    return BadRequest("Invalid image");

                var result = _checker.CheckQuality(bitmap, settings);
                var response = MapResult(result, checks, settings);
                return Ok(response);
            }

            using (var ms = new MemoryStream())
            {
                await request.Pdf!.CopyToAsync(ms);
                ms.Position = 0;
                var responses = new List<QualityCheckResponse>();
                foreach (var result in _checker.CheckQuality(ms, settings, request.PageIndex))
                {
                    responses.Add(MapResult(result, checks, settings));
                }
                return Ok(responses);
            }
        }

        private static QualityCheckResponse MapResult(DocumentQualityResult data, List<string> checks, QualitySettings settings)
        {
            var response = new QualityCheckResponse();

            bool needBlurHeatmap = settings.GenerateHeatmaps &&
                                    checks.Any(c => c.Equals("blur", StringComparison.InvariantCultureIgnoreCase));
            bool needGlareHeatmap = settings.GenerateHeatmaps &&
                                     checks.Any(c => c.Equals("glare", StringComparison.InvariantCultureIgnoreCase));

            foreach (var c in checks)
            {
                switch (c.ToLowerInvariant())
                {
                    case "brisque":
                        response.Results["BrisqueScore"] = data.BrisqueScore;
                        response.Results["BrisqueValid"] = data.BrisqueScore <= settings.BrisqueMax;
                        response.Explanations["Brisque"] = $"Varianza normalizzata {data.BrisqueScore:F2} (soglia {settings.BrisqueMax})";
                        break;
                    case "blur":
                        response.Results["BlurScore"] = data.BlurScore;
                        response.Results["IsBlurry"] = data.IsBlurry;
                        response.Explanations["Blur"] = $"Varianza Laplaciano {data.BlurScore:F2} (soglia {settings.BlurThreshold})";
                        if (needBlurHeatmap && data.BlurHeatmap != null)
                        {
                            using var mapData = data.BlurHeatmap.Encode(SKEncodedImageFormat.Png, 100);
                            response.BlurHeatmap = Convert.ToBase64String(mapData.ToArray());
                            if (data.BlurRegions != null)
                                response.BlurRegions = data.BlurRegions.Select(r => new RegionDto { X = r.Left, Y = r.Top, Width = r.Width, Height = r.Height }).ToList();
                        }
                        break;
                    case "glare":
                        response.Results["GlareArea"] = data.GlareArea;
                        response.Results["HasGlare"] = data.HasGlare;
                        response.Explanations["Glare"] = $"Area pixel luminosi {data.GlareArea} (soglia {settings.AreaThreshold})";
                        if (needGlareHeatmap && data.GlareHeatmap != null)
                        {
                            using var glareData = data.GlareHeatmap.Encode(SKEncodedImageFormat.Png, 100);
                            response.GlareHeatmap = Convert.ToBase64String(glareData.ToArray());
                            if (data.GlareRegions != null)
                                response.GlareRegions = data.GlareRegions.Select(r => new RegionDto { X = r.Left, Y = r.Top, Width = r.Width, Height = r.Height }).ToList();
                        }
                        break;
                    case "exposure":
                        response.Results["Exposure"] = data.Exposure;
                        response.Results["IsWellExposed"] = data.IsWellExposed;
                        response.Explanations["Exposure"] = $"Luminanza media {data.Exposure:F2} (range {settings.ExposureMin}-{settings.ExposureMax})";
                        break;
                    case "contrast":
                        response.Results["Contrast"] = data.Contrast;
                        response.Results["HasLowContrast"] = data.HasLowContrast;
                        response.Explanations["Contrast"] = $"Deviazione standard {data.Contrast:F2} (min {settings.ContrastMin})";
                        break;
                    case "colordominance":
                        response.Results["ColorDominance"] = data.ColorDominance;
                        response.Results["HasColorDominance"] = data.HasColorDominance;
                        response.Explanations["ColorDominance"] = $"Rapporto canale dominante {data.ColorDominance:F2} (soglia {settings.DominanceThreshold})";
                        break;
                    case "noise":
                        response.Results["Noise"] = data.Noise;
                        response.Results["HasNoise"] = data.HasNoise;
                        response.Explanations["Noise"] = $"Livello di rumore {data.Noise:F2} (soglia {settings.NoiseThreshold})";
                        break;
                    case "motionblur":
                        response.Results["MotionBlurScore"] = data.MotionBlurScore;
                        response.Results["HasMotionBlur"] = data.HasMotionBlur;
                        response.Explanations["MotionBlur"] = $"Rapporto gradienti {data.MotionBlurScore:F2} (soglia {settings.MotionBlurThreshold})";
                        break;
                    case "banding":
                        response.Results["BandingScore"] = data.BandingScore;
                        response.Results["HasBanding"] = data.HasBanding;
                        response.Explanations["Banding"] = $"Varianza bande {data.BandingScore:F2} (soglia {settings.BandingThreshold})";
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
            return response;
        }
    }

    public class QualityCheckRequest
    {
        public IFormFile? Image { get; set; }
        public IFormFile? Pdf { get; set; }

        /// <summary>
        /// Zero based index of the PDF page to process. If null all pages are
        /// analysed.
        /// </summary>
        public int? PageIndex { get; set; }

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
