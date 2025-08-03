using DocQualityChecker;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SkiaSharp;

namespace DocQualityChecker.Web.Pages;

public class IndexModel : PageModel
{
    private readonly DocumentQualityChecker _checker = new();

    [BindProperty]
    public IFormFile? Image { get; set; }

    public QualitySettings Settings { get; } = new();

    public DocumentQualityResult? Result { get; private set; }

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
        return Page();
    }
}
