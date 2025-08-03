using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;

namespace DocQualityChecker.Web.Tests;

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
}
