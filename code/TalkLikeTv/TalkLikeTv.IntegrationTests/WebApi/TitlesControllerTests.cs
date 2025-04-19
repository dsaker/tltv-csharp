using System.Net;
using System.Net.Http.Json;
using TalkLikeTv.WebApi.Mappers;
using TalkLikeTv.WebApi.Models;

namespace TalkLikeTv.IntegrationTests.WebApi;

public class TitlesControllerTests :
    IClassFixture<TestWebApplicationFactory>
{
    private readonly TestWebApplicationFactory _factory;
    private const string relativePath = "/api/titles";

    public TitlesControllerTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetTitles_ReturnsTitleCollection()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync(relativePath);

        response.EnsureSuccessStatusCode();

        var titles = await response.Content.ReadFromJsonAsync<IEnumerable<TitleMapper.TitleResponse>>();

        // Assert
        Assert.NotNull(titles);
        Assert.IsAssignableFrom<IEnumerable<TitleMapper.TitleResponse>>(titles);

        foreach (var title in titles)
        {
            Assert.NotEqual(0, title.TitleId);
            Assert.False(string.IsNullOrEmpty(title.TitleName));
            Assert.NotNull(title.NumPhrases);
            Assert.NotNull(title.Popularity);
        }
    }

    [Fact]
    public async Task GetTitles_HasExpectedCacheHeaders()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync(relativePath);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var cacheControlHeader = response.Headers.CacheControl.ToString();
        Assert.Contains("max-age=3600", cacheControlHeader);
        Assert.Contains("public", cacheControlHeader);
    }

    [Fact]
    public async Task GetTitles_WithInvalidLanguageId_ReturnsBadRequest()
    {
        // Arrange
        var client = _factory.CreateClient();
        var invalidLanguageIdPath = $"{relativePath}?originallanguageid=invalid";

        // Act
        var response = await client.GetAsync(invalidLanguageIdPath);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var errorResponse = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        Assert.NotNull(errorResponse);
        Assert.Contains("Invalid originalLanguageId format", errorResponse.Errors.First());
    }

    [Fact]
    public async Task GetTitles_WithValidLanguageId_ReturnsFilteredTitles()
    {
        // Arrange
        var client = _factory.CreateClient();
        var validLanguageIdPath = $"{relativePath}?originallanguageid=1";

        // Act
        var response = await client.GetAsync(validLanguageIdPath);

        response.EnsureSuccessStatusCode();

        var titles = await response.Content.ReadFromJsonAsync<IEnumerable<TitleMapper.TitleResponse>>();

        // Assert
        Assert.NotNull(titles);
        Assert.All(titles, title => Assert.Equal(1, title.OriginalLanguageId));
    }
}