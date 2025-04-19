using System.Net;
using System.Net.Http.Json;
using TalkLikeTv.WebApi.Mappers;

namespace TalkLikeTv.IntegrationTests.WebApi;

public class VoicesControllerTests :
    IClassFixture<TestWebApplicationFactory>
{
    private readonly TestWebApplicationFactory _factory;
    private const string relativePath = "/api/voices";

    public VoicesControllerTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetVoices_ReturnsVoiceCollection()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync(relativePath);

        response.EnsureSuccessStatusCode();

        var voices = await response.Content.ReadFromJsonAsync<IEnumerable<VoiceMapper.VoiceResponse>>();

        // Assert
        Assert.NotNull(voices);
        Assert.IsAssignableFrom<IEnumerable<VoiceMapper.VoiceResponse>>(voices);

        foreach (var voice in voices)
        {
            Assert.NotEqual(0, voice.VoiceId);
            Assert.False(string.IsNullOrEmpty(voice.DisplayName));
            Assert.False(string.IsNullOrEmpty(voice.Gender));
            Assert.False(string.IsNullOrEmpty(voice.Locale));
            Assert.NotNull(voice.LanguageId);
        }
    }

    [Fact]
    public async Task GetVoices_HasExpectedCacheHeaders()
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
    
}