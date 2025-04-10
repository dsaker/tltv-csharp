using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Microsoft.Extensions.Logging;
using TalkLikeTv.EntityModels;
using TalkLikeTv.Repositories;
using TalkLikeTv.WebApi.Controllers;

namespace TalkLikeTv.UnitTests.Tests.WebApi;

public class VoicesControllerTests
{
    private readonly Mock<IVoiceRepository> _mockRepo;
    private readonly Mock<ILogger<VoicesController>> _mockLogger;
    private readonly VoicesController _controller;

    public VoicesControllerTests()
    {
        _mockRepo = new Mock<IVoiceRepository>();
        _mockLogger = new Mock<ILogger<VoicesController>>();
        _controller = new VoicesController(_mockRepo.Object, _mockLogger.Object);
        
        // Set up HttpContext with CancellationToken
        var httpContext = new DefaultHttpContext();
        _controller.ControllerContext = new ControllerContext()
        {
            HttpContext = httpContext
        };
    }

    [Fact]
    public async Task GetVoices_ReturnsVoices_WhenSuccessful()
    {
        // Arrange
        var voices = new[]
        {
            new Voice
            {
                VoiceId = 1,
                DisplayName = "Female Voice",
                Platform = "Azure",
                Gender = "Female"
            }
        };

        _mockRepo.Setup(repo => repo.RetrieveAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(voices);

        // Act
        var result = await _controller.GetVoices();

        // Assert
        var okResult = Assert.IsType<ActionResult<IEnumerable<Voice>>>(result);
        var returnValue = Assert.IsType<OkObjectResult>(okResult.Result);
        Assert.Equal(200, returnValue.StatusCode);
        Assert.Equal(voices, returnValue.Value);
    }

    [Fact]
    public async Task GetVoices_Returns500_WhenExceptionThrown()
    {
        // Arrange
        _mockRepo.Setup(repo => repo.RetrieveAllAsync(It.IsAny<CancellationToken>()))
                 .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _controller.GetVoices();

        // Assert
        var statusCodeResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(500, statusCodeResult.StatusCode);
        Assert.Equal("An error occurred while processing your request.", statusCodeResult.Value);
    }

    [Fact]
    public async Task GetVoice_ReturnsVoice_WhenFound()
    {
        // Arrange
        var voice = new Voice 
        { 
            VoiceId = 1, 
            DisplayName = "Male Voice", 
            Platform = "Google",
            Gender = "Male"
        };
        
        _mockRepo.Setup(repo => repo.RetrieveAsync("1", It.IsAny<CancellationToken>()))
                 .ReturnsAsync(voice);

        // Act
        var result = await _controller.GetVoice("1");

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(voice, okResult.Value);
    }

    [Fact]
    public async Task GetVoice_Returns404_WhenNotFound()
    {
        // Arrange
        _mockRepo.Setup(repo => repo.RetrieveAsync("1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => null);

        // Act
        var result = await _controller.GetVoice("1");

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task GetVoice_Returns500_WhenExceptionThrown()
    {
        // Arrange
        _mockRepo.Setup(repo => repo.RetrieveAsync("1", It.IsAny<CancellationToken>()))
                 .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _controller.GetVoice("1");

        // Assert
        var statusCodeResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, statusCodeResult.StatusCode);
        Assert.Equal("An error occurred while processing your request.", statusCodeResult.Value);
        
        // Verify logger was called with correct parameters
        _mockLogger.Verify(
            x => x.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((o, t) => true),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception?, string>>((o, t) => true)),
            Times.Once);
    }
}