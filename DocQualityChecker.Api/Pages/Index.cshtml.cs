using System;
using DocQualityChecker;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SkiaSharp;

namespace DocQualityChecker.Api.Pages;

public class IndexModel : PageModel
{
    private readonly DocumentQualityChecker _checker = new();

    [BindProperty]
    public IFormFile? Image { get; set; }

    [BindProperty]
    public QualitySettings Settings { get; set; } = new();

    public DocumentQualityResult? Result { get; private set; }
    public string? BlurHeatmap { get; private set; }
    public string? GlareHeatmap { get; private set; }

    public IActionResult OnPost()
    {
        if (Image == null || Image.Length == 0)
        {
            ModelState.AddModelError("Image", "Please select an image.");
            return Page();
        }

        using var stream = Image.OpenReadStream();
        using var bitmap = SKBitmap.Decode(stream);
        Result = _checker.CheckQuality(bitmap, Settings);

        if (Settings.GenerateHeatmaps)
        {
            if (Result.BlurHeatmap != null)
            {
                using var img = SKImage.FromBitmap(Result.BlurHeatmap);
                using var data = img.Encode(SKEncodedImageFormat.Png, 100);
                BlurHeatmap = "data:image/png;base64," + Convert.ToBase64String(data.ToArray());
            }
            if (Result.GlareHeatmap != null)
            {
                using var img = SKImage.FromBitmap(Result.GlareHeatmap);
                using var data = img.Encode(SKEncodedImageFormat.Png, 100);
                GlareHeatmap = "data:image/png;base64," + Convert.ToBase64String(data.ToArray());
            }
        }

        return Page();
    }
}
