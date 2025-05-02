using System.Net;
using FastEndpoints;
using Shouldly;
using FastEndpoints.Testing;
using TalkLikeTv.EntityModels;
using TalkLikeTv.FastEndpoints.Endpoints;

namespace TalkLikeTv.FastEndpointsTests.UnitTests;

public class LanguagesEndpointTests(MyApp app) : TestBase<MyApp>
{
    [Fact]
    public async Task GetLanguages_ShouldReturnAllLanguages()
    {
        // Create a request with empty ID
        var request = new LanguagesRequest("");

        // Execute GET request to /languages endpoint
        var result = await app.Client.GETAsync<LanguagesEndpoint, LanguagesRequest, Language[]>(request);

        // Assert response status code
        result.Response.StatusCode.ShouldBe(HttpStatusCode.OK);

        // Assert result content
        result.Result.ShouldNotBeNull();
        result.Result.Length.ShouldBe(1);
        result.Result[0].LanguageId.ShouldBe(1);
        result.Result[0].Name.ShouldBe("English");
    }

    [Fact]
    public async Task GetLanguageById_ShouldReturnLanguage()
    {
        // For route parameters, use proper request model
        var request = new LanguagesRequest("1");

        // Execute GET request to /languages/1 endpoint
        var result = await app.Client.GETAsync<LanguagesEndpoint, LanguagesRequest, Language[]>(request);

        // Assert response status code
        result.Response.StatusCode.ShouldBe(HttpStatusCode.OK);

        // Assert result content
        result.Result.ShouldNotBeNull();
        result.Result.Length.ShouldBe(1);
        result.Result[0].LanguageId.ShouldBe(1);
        result.Result[0].Name.ShouldBe("English");
    }

    [Fact]
    public async Task GetLanguageById_WithNonExistentId_ShouldReturnNotFound()
    {
        // For route parameters, use proper request model with non-existent ID
        var request = new LanguagesRequest("999");

        // Execute GET request with invalid ID
        var result = await app.Client.GETAsync<LanguagesEndpoint, LanguagesRequest>(request);

        // Assert response status code
        result.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetLanguageByTag_ShouldReturnLanguage()
    {
        // For route parameters, use proper request model
        var request = new LanguagesByTagRequest("en-US");

        // Execute GET request to /languages/tag/en-US endpoint
        var result = await app.Client.GETAsync<LanguagesByTagEndpoint, LanguagesByTagRequest, Language[]>(request);

        // Assert response status code
        result.Response.StatusCode.ShouldBe(HttpStatusCode.OK);

        // Assert result content
        result.Result.ShouldNotBeNull();
        result.Result.Length.ShouldBe(1);
        result.Result[0].LanguageId.ShouldBe(1);
        result.Result[0].Name.ShouldBe("English");
    }

    [Fact]
    public async Task GetLanguageByTag_WithNonExistentTag_ShouldReturnNotFound()
    {
        // For route parameters, use proper request model with non-existent tag
        var request = new LanguagesByTagRequest("xx-XX");

        // Execute GET request with invalid tag
        var result = await app.Client.GETAsync<LanguagesByTagEndpoint, LanguagesByTagRequest>(request);

        // Assert response status code
        result.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }
}