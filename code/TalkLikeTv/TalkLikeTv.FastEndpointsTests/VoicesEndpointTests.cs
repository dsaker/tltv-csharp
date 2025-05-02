using System.Net;
using FastEndpoints;
using Shouldly;
using FastEndpoints.Testing;
using TalkLikeTv.FastEndpoints.Endpoints;
using TalkLikeTv.FastEndpoints.Mappers;

namespace TalkLikeTv.FastEndpointsTests.UnitTests;

public class VoicesEndpointTests(MyApp app) : TestBase<MyApp>
{
    [Fact]
    public async Task GetVoices_ShouldReturnAllVoices()
    {
        // Create a request object with LanguageId set to null
        var request = new VoicesRequest("");

        // Execute GET request to /voices endpoint
        var result = await app.Client.GETAsync<VoicesEndpoint, VoicesRequest, VoiceResponse[]>(request);

        // Assert response status code
        result.Response.StatusCode.ShouldBe(HttpStatusCode.OK);

        // Assert result content (will need mocked voice data in MyApp.cs)
        result.Result.ShouldNotBeNull();
    }

    [Fact]
    public async Task GetVoicesByLanguageId_ShouldReturnFilteredVoices()
    {
        // For route parameters, use proper request model
        var request = new VoicesRequest("1");

        // Execute GET request to /voices/1 endpoint
        var result = await app.Client.GETAsync<VoicesEndpoint, VoicesRequest, VoiceResponse[]>(request);

        // Assert response status code
        result.Response.StatusCode.ShouldBe(HttpStatusCode.OK);

        // Assert result content
        result.Result.ShouldNotBeNull();
        // All returned voices should have LanguageId = 1
        result.Result.All(v => v.LanguageId == 1).ShouldBeTrue();
    }

    [Fact]
    public async Task GetVoicesByLanguageId_WithInvalidId_ShouldReturnEmptyArray()
    {
        // For route parameters, use proper request model with non-existent language ID
        var request = new VoicesRequest("999");

        // Execute GET request with invalid language ID
        var result = await app.Client.GETAsync<VoicesEndpoint, VoicesRequest, VoiceResponse[]>(request);

        // Assert response status code - still OK but with empty array
        result.Response.StatusCode.ShouldBe(HttpStatusCode.OK);
        
        // Assert result is empty array
        result.Result.ShouldNotBeNull();
        result.Result.Length.ShouldBe(0);
    }
}