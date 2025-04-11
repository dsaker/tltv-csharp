using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using TalkLikeTv.Repositories;
using TalkLikeTv.Services;
using TalkLikeTv.WebApi.Controllers;
using TalkLikeTv.WebApi.Models;

namespace TalkLikeTv.UnitTests.Tests.WebApi;

public class AudioControllerTests
{
    private readonly Mock<IAudioProcessingService> _mockAudioProcessingService;
    private readonly Mock<ITokenService> _mockTokenService;
    private readonly Mock<ITitleRepository> _mockTitleRepository;
    private readonly Mock<ILogger<AudioController>> _mockLogger;
    private readonly AudioController _controller;

    public AudioControllerTests()
    {
        _mockAudioProcessingService = new Mock<IAudioProcessingService>();
        _mockTokenService = new Mock<ITokenService>();
        _mockTitleRepository = new Mock<ITitleRepository>();
        _mockLogger = new Mock<ILogger<AudioController>>();

        _controller = new AudioController(
            _mockAudioProcessingService.Object,
            _mockTokenService.Object,
            _mockLogger.Object,
            _mockTitleRepository.Object);
        
        // Set up HttpContext with CancellationToken
        var httpContext = new DefaultHttpContext();
        _controller.ControllerContext = new ControllerContext()
        {
            HttpContext = httpContext
        };
    }

    [Fact]
    public async Task AudioFromTitle_ReturnsBadRequest_WhenModelStateIsInvalid()
    {
        // Arrange
        _controller.ModelState.AddModelError("TitleId", "Required");

        // Act
        var result = await _controller.AudioFromTitle(new AudioFromTitleApiModel());

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task AudioFromTitle_ReturnsBadRequest_WhenTokenIsInvalid()
    {
        // Arrange
        var model = new AudioFromTitleApiModel { Token = "invalidToken" };
        _mockTokenService.Setup(s => s.CheckTokenStatus(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ITokenService.TokenResult { Success = false, ErrorMessage = "Invalid token" });

        // Act
        var result = await _controller.AudioFromTitle(model);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        var errorResponse = Assert.IsType<ErrorResponse>(badRequestResult.Value);
        Assert.Contains("Invalid token", errorResponse.Errors);
    }

    [Fact]
    public async Task AudioFromTitle_ReturnsBadRequest_WhenTitleNotFound()
    {
        // Arrange
        var model = new AudioFromTitleApiModel { TitleId = 1, Token = "validToken" };
        _mockTokenService.Setup(s => s.CheckTokenStatus(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ITokenService.TokenResult { Success = true });
        _mockTitleRepository.Setup(r => r.RetrieveAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((TalkLikeTv.EntityModels.Title?)null);

        // Act
        var result = await _controller.AudioFromTitle(model);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        var errorResponse = Assert.IsType<ErrorResponse>(badRequestResult.Value);
        Assert.Contains("Title not found.", errorResponse.Errors);
    }

    [Fact]
    public async Task AudioFromTitle_ReturnsBadRequest_WhenAudioProcessingFails()
    {
        // Arrange
        var model = new AudioFromTitleApiModel { TitleId = 1, Token = "validToken" };
        _mockTokenService.Setup(s => s.CheckTokenStatus(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ITokenService.TokenResult { Success = true });
        _mockTitleRepository.Setup(r => r.RetrieveAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new TalkLikeTv.EntityModels.Title());
        _mockAudioProcessingService.Setup(s => s.ProcessAudioRequestAsync(
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<TalkLikeTv.EntityModels.Title>(),
                It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((null, new List<string> { "Error processing audio" }));

        // Act
        var result = await _controller.AudioFromTitle(model);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        var errorResponse = Assert.IsType<ErrorResponse>(badRequestResult.Value);
        Assert.Contains("Error processing audio", errorResponse.Errors);
    }

    [Fact]
    public async Task AudioFromTitle_ReturnsFile_WhenSuccessful()
    {
        // Arrange
        var model = new AudioFromTitleApiModel
        {
            TitleId = 1,
            Token = "validToken",
            ToVoiceId = 2,
            FromVoiceId = 3,
            PauseDuration = 5,
            Pattern = "testPattern"
        };

        // Create a temporary file to simulate the zip file
        var tempFilePath = Path.Combine(Path.GetTempPath(), "test.zip");
        await File.WriteAllTextAsync(tempFilePath, "Dummy content");

        var mockFilePath = new FileInfo(tempFilePath);

        _mockTokenService.Setup(s => s.CheckTokenStatus(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ITokenService.TokenResult { Success = true });
        _mockTitleRepository.Setup(r => r.RetrieveAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new TalkLikeTv.EntityModels.Title());
        _mockAudioProcessingService.Setup(s => s.ProcessAudioRequestAsync(
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<TalkLikeTv.EntityModels.Title>(),
                It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((mockFilePath, new List<string>()));
        _mockAudioProcessingService.Setup(s => s.MarkTokenAsUsedAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((true, new List<string>()));

        // Act
        var result = await _controller.AudioFromTitle(model);

        // Assert
        var fileResult = Assert.IsType<FileStreamResult>(result);
        Assert.Equal("application/zip", fileResult.ContentType);

        // Clean up the temporary file
        if (File.Exists(tempFilePath))
        {
            File.Delete(tempFilePath);
        }
    }
}