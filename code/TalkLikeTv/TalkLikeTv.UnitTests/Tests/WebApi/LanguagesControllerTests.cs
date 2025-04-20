using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Microsoft.Extensions.Logging;
using TalkLikeTv.EntityModels;
using TalkLikeTv.Repositories;
using TalkLikeTv.WebApi.Controllers;

namespace TalkLikeTv.UnitTests.Tests.WebApi;

public class LanguagesControllerTests
{
    private readonly Mock<ILanguageRepository> _mockRepo;
    private readonly Mock<ILogger<LanguagesController>> _mockLogger;
    private readonly LanguagesController _controller;

    public LanguagesControllerTests()
    {
        _mockRepo = new Mock<ILanguageRepository>();
        _mockLogger = new Mock<ILogger<LanguagesController>>();
        _controller = new LanguagesController(_mockRepo.Object, _mockLogger.Object);
        
        // Set up HttpContext with CancellationToken
        var httpContext = new DefaultHttpContext();
        _controller.ControllerContext = new ControllerContext()
        {
            HttpContext = httpContext
        };
    }

    [Fact]
    public async Task GetLanguages_ReturnsLanguages_WhenSuccessful()
    {
        // Arrange
        var languages = new[] { new Language { LanguageId = 1, Name = "English" } };
        _mockRepo.Setup(repo => repo.RetrieveAllAsync(It.IsAny<CancellationToken>()))
                 .ReturnsAsync(languages);

        // Act
        var result = await _controller.GetLanguages();

        // Assert
        var okResult = Assert.IsType<ActionResult<IEnumerable<Language>>>(result);
        var returnValue = Assert.IsType<OkObjectResult>(okResult.Result);
        Assert.Equal(languages, returnValue.Value);
    }

    [Fact]
    public async Task GetLanguages_Returns500_WhenExceptionThrown()
    {
        // Arrange
        _mockRepo.Setup(repo => repo.RetrieveAllAsync(It.IsAny<CancellationToken>()))
                 .ThrowsAsync(new System.Exception("Database error"));

        // Act
        var result = await _controller.GetLanguages();

        // Assert
        var statusCodeResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(500, statusCodeResult.StatusCode);
    }

    [Fact]
    public async Task GetLanguage_ReturnsLanguage_WhenFound()
    {
        // Arrange
        var language = new Language { LanguageId = 1, Name = "English" };
        _mockRepo.Setup(repo => repo.RetrieveAsync("1", It.IsAny<CancellationToken>()))
                 .ReturnsAsync(language);

        // Act
        var result = await _controller.GetLanguage("1");

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(language, okResult.Value);
    }

    [Fact]
    public async Task GetLanguage_Returns404_WhenNotFound()
    {
        // Arrange
        _mockRepo.Setup(repo => repo.RetrieveAsync("1", It.IsAny<CancellationToken>()))
                 .ReturnsAsync((Language?)null);

        // Act
        var result = await _controller.GetLanguage("1");

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task GetLanguageByTag_ReturnsLanguage_WhenFound()
    {
        // Arrange
        var language = new Language { LanguageId = 1, Name = "English", Tag = "en" };
        _mockRepo.Setup(repo => repo.RetrieveByTagAsync("en", It.IsAny<CancellationToken>()))
                 .ReturnsAsync(language);

        // Act
        var result = await _controller.GetLanguageByTag("en");

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(language, okResult.Value);
    }

    [Fact]
    public async Task GetLanguageByTag_Returns404_WhenNotFound()
    {
        // Arrange
        _mockRepo.Setup(repo => repo.RetrieveByTagAsync("en", It.IsAny<CancellationToken>()))
                 .ReturnsAsync((Language?)null);

        // Act
        var result = await _controller.GetLanguageByTag("en");

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }
}