using System.Net;
using FastEndpoints;
using Shouldly; // For ShouldBe extensions
using FastEndpoints.Testing;
using TalkLikeTv.EntityModels;
using TalkLikeTv.FastEndpoints.Endpoints;
using TalkLikeTv.FastEndpoints.Mappers;
using TalkLikeTv.Utilities.Mappers;

namespace TalkLikeTv.FastEndpointsTests.UnitTests;

public class TitlesEndpointTests(MyApp app) : TestBase<MyApp>
{
    [Fact]
    public async Task GetTitles_ShouldReturnAllTitles()
    {
        // Create a request object with both Id and Name set to null
        var request = new TitlesRequest("");

        // Execute GET request to /titles endpoint
        var result = await app.Client.GETAsync<TitlesEndpoint, TitlesRequest, Title[]>(request);

        // Assert response status code
        result.Response.StatusCode.ShouldBe(HttpStatusCode.OK);

        // Assert result content
        result.Result.ShouldNotBeNull();
        result.Result.Length.ShouldBe(2);
        result.Result[0].TitleName.ShouldBe("Title1");
        result.Result[1].TitleName.ShouldBe("Title2");
    }

    [Fact]
    public async Task GetTitleById_ShouldReturnTitle()
    {
        // For route parameters, use proper request model
        var request = new TitlesRequest("1");

        // Execute GET request to /titles/1 endpoint
        var result = await app.Client.GETAsync<TitlesEndpoint, TitlesRequest, Title[]>(request);

        // Assert response status code
        result.Response.StatusCode.ShouldBe(HttpStatusCode.OK);

        // Assert result content
        result.Result.ShouldNotBeNull();
        result.Result.Length.ShouldBe(1);
        result.Result[0].TitleId.ShouldBe(1);
        result.Result[0].TitleName.ShouldBe("Title1");
    }

    [Fact]
    public async Task GetTitleByName_ShouldReturnTitle()
    {
        // For route parameters, use proper request model
        var request = new TitlesByNameRequest("Title2");

        // Execute GET request to /titles/name/Title2 endpoint
        var result = await app.Client.GETAsync<TitlesByNameEndpoint, TitlesByNameRequest, Title[]>(request);

        // Assert response status code
        result.Response.StatusCode.ShouldBe(HttpStatusCode.OK);

        // Assert result content
        result.Result.ShouldNotBeNull();
        result.Result.Length.ShouldBe(1);
        result.Result[0].TitleId.ShouldBe(2);
        result.Result[0].TitleName.ShouldBe("Title2");
    }

    [Fact]
    public async Task GetTitleById_WithNonExistentId_ShouldReturnNotFound()
    {
        // For route parameters, use proper request model
        var request = new TitlesRequest("999");

        // Execute GET request with non-existent ID
        var result = await app.Client.GETAsync<TitlesEndpoint, TitlesRequest>(request);

        // Assert response status code
        result.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetTitleByName_WithNonExistentName_ShouldReturnNotFound()
    {
        // For route parameters, use proper request model
        var request = new TitlesByNameRequest("NonExistentTitle");

        // Execute GET request with non-existent name
        var result = await app.Client.GETAsync<TitlesEndpoint, TitlesByNameRequest>(request);

        // Assert response status code
        result.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CreateTitle_ShouldCreateAndReturnNewTitle()
    {
        // Create a new request object for creating a title
        var newTitleRequest = new CreateTitleRequest(
            TitleName: "NewTitle",
            Description: "A new test title",
            NumPhrases: 10,
            OriginalLanguageId: 1
        );

        // Execute POST request to /titles endpoint
        var result = await app.Client.POSTAsync<CreateTitleEndpoint, CreateTitleRequest, TitleResponse>(newTitleRequest);

        // Assert response status code
        result.Response.StatusCode.ShouldBe(HttpStatusCode.Created);

        // Assert result content
        result.Result.ShouldNotBeNull();
        result.Result.TitleId.ShouldBe(3);
        result.Result.TitleName.ShouldBe("NewTitle");
        result.Result.Description.ShouldBe("A new test title");

        // Assert Location header contains the correct URL
        var locationHeader = result.Response.Headers.Location;
        locationHeader.ShouldNotBeNull();
        locationHeader.ToString().ShouldContain("/titles/3");
    }
    
    [Fact]
    public async Task UpdateTitle_ShouldUpdateAndReturnOk()
    {
        // Arrange
        var request = new UpdateTitleRequest(
            Id: "1",
            Title: new UpdateTitleDto
            {
                TitleName = "UpdatedTitle",
                Description = "Updated description",
                NumPhrases = 15,
                Popularity = 5,
                OriginalLanguageId = 2
            }
        );

        // Act
        var result = await app.Client.PUTAsync<UpdateTitleEndpoint, UpdateTitleRequest>(request);

        // Assert
        result.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task UpdateTitle_WithNonExistentId_ShouldReturnNotFound()
    {
        // Arrange
        var request = new UpdateTitleRequest(
            Id: "999",
            Title: new UpdateTitleDto
            {
                TitleName = "UpdatedTitle",
                Description = "Updated description",
                NumPhrases = 15,
                Popularity = 5,
                OriginalLanguageId = 2
            }
        );

        // Act
        var result = await app.Client.PUTAsync<UpdateTitleEndpoint, UpdateTitleRequest>(request);

        // Assert
        result.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }
    
    [Fact]
    public async Task DeleteTitle_ShouldDeleteAndReturnNoContent()
    {
        // Arrange
        var request = new TitlesRequest("1");

        // Act
        var result = await app.Client.DELETEAsync<DeleteTitleEndpoint, TitlesRequest>(request);

        // Assert
        result.StatusCode.ShouldBe(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task DeleteTitle_WithNonExistentId_ShouldReturnNotFound()
    {
        // Arrange
        var request = new TitlesRequest("999");

        // Act
        var result = await app.Client.DELETEAsync<DeleteTitleEndpoint, TitlesRequest>(request);

        // Assert
        result.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }
}