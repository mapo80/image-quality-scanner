using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using SkiaSharp;

namespace DocQualityChecker.Api.Tests;

public class RazorAppTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public RazorAppTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Home_page_loads()
    {
        var response = await _client.GetAsync("/");
        var html = await response.Content.ReadAsStringAsync();
        Assert.True(response.IsSuccessStatusCode, response.StatusCode.ToString());
        Assert.Contains("Document quality analysis", html);
    }

    [Fact]
    public async Task Quality_endpoint_returns_result()
    {
        using var bmp = CreateBaseImage();
        using var img = SKImage.FromBitmap(bmp);
        using var data = img.Encode(SKEncodedImageFormat.Png, 100);
        using var ms = new MemoryStream();
        data.SaveTo(ms);
        ms.Position = 0;

        var content = new MultipartFormDataContent();
        var file = new StreamContent(ms);
        file.Headers.ContentType = new MediaTypeHeaderValue("image/png");
        content.Add(file, "Image", "test.png");

        var response = await _client.PostAsync("/quality/check", content);
        var json = await response.Content.ReadAsStringAsync();

        Assert.True(response.IsSuccessStatusCode, response.StatusCode.ToString());
        Assert.Contains("BlurScore", json);
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
}
