using System.IO;
using DocQualityChecker.Api.Pages;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Routing;
using SkiaSharp;
using Xunit;

namespace DocQualityChecker.Api.Tests;

public class IndexModelTests
{
    private static IndexModel CreateModel()
    {
        var model = new IndexModel();
        var httpContext = new DefaultHttpContext();
        var actionContext = new ActionContext(httpContext, new RouteData(), new PageActionDescriptor());
        model.PageContext = new PageContext(actionContext);
        return model;
    }

    private static SKBitmap CreateBaseImage()
    {
        var bmp = new SKBitmap(200, 200);
        using var canvas = new SKCanvas(bmp);
        canvas.Clear(new SKColor(200, 200, 200));
        using var paint = new SKPaint { Color = SKColors.Black };
        canvas.DrawRect(new SKRect(40, 80, 160, 120), paint);
        canvas.Flush();
        return bmp;
    }

    [Fact]
    public void OnPost_NoImage_AddsModelError()
    {
        var model = CreateModel();

        var result = model.OnPost();

        Assert.IsType<PageResult>(result);
        Assert.False(model.ModelState.IsValid);
        Assert.Contains("Please select an image.", model.ModelState["Image"]!.Errors[0].ErrorMessage);
        Assert.Null(model.Result);
    }

    [Fact]
    public void OnPost_WithImage_ReturnsResult()
    {
        var model = CreateModel();

        using var bmp = CreateBaseImage();
        using var img = SKImage.FromBitmap(bmp);
        using var data = img.Encode(SKEncodedImageFormat.Png, 100);
        using var ms = new MemoryStream();
        data.SaveTo(ms);
        ms.Position = 0;

        model.Image = new FormFile(ms, 0, ms.Length, "Image", "test.png");

        var actionResult = model.OnPost();

        Assert.IsType<PageResult>(actionResult);
        Assert.True(model.ModelState.IsValid);
        Assert.NotNull(model.Result);
    }
}

