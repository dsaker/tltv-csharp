using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using TalkLikeTv.EntityModels;
using TalkLikeTv.Repositories;
using TalkLikeTv.Services;
using TalkLikeTv.Services.Abstractions;

namespace TalkLikeTv.UnitTests.Tests.Services;

public class AudioProcessingServiceTests
{
    private readonly AudioProcessingService _service;
    private readonly Mock<ITranslateService> _mockTranslateService;
    private readonly Mock<ILanguageRepository> _mockLanguageRepository;

    public AudioProcessingServiceTests()
    {
        var mockLogger = new Mock<ILogger<AudioProcessingService>>();
        var mockTranslationService = new Mock<ITranslationService>();
        var mockAudioFileService = new Mock<IAudioFileService>();
        var mockVoiceRepository = new Mock<IVoiceRepository>();
        _mockLanguageRepository = new Mock<ILanguageRepository>();
        var mockTokenRepository = new Mock<ITokenRepository>();
        var mockTitleRepository = new Mock<ITitleRepository>();
        var mockPhraseRepository = new Mock<IPhraseRepository>();
        var mockTranslateRepository = new Mock<ITranslateRepository>();
        _mockTranslateService = new Mock<ITranslateService>();

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "SharedSettings:AudioOutputDir", "/output/dir" }
            })
            .Build();

        _service = new AudioProcessingService(
            mockLogger.Object,
            mockTranslationService.Object,
            mockAudioFileService.Object,
            mockVoiceRepository.Object,
            _mockLanguageRepository.Object,
            mockTokenRepository.Object,
            mockTitleRepository.Object,
            mockPhraseRepository.Object,
            mockTranslateRepository.Object,
            _mockTranslateService.Object,
            configuration
        );
    }

    [Fact]
    public async Task DetectLanguageAsync_ReturnsLanguage_WhenValidPhrasesProvided()
    {
        // Arrange
        _mockTranslateService
            .Setup(service => service.DetectLanguageFromPhrasesAsync(It.IsAny<List<string>>()))
            .ReturnsAsync("en");

        _mockLanguageRepository
            .Setup(repo => repo.RetrieveByTagAsync("en", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Language { Tag = "en", LanguageId = 1 });

        var phrases = new List<string> { "Hello", "World" };

        // Act
        var (language, errors) = await _service.DetectLanguageAsync(phrases);

        // Assert
        Assert.NotNull(language);
        Assert.Equal("en", language?.Tag);
        Assert.Empty(errors);
    }

    [Fact]
    public async Task ProcessAudioRequestAsync_ReturnsError_WhenInvalidVoicesProvided()
    {
        // Arrange
        var mockVoiceRepository = new Mock<IVoiceRepository>();
        mockVoiceRepository
            .Setup(repo => repo.RetrieveAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Voice?)null);

        var title = new Title { TitleName = "TestTitle" };

        // Act
        var (zipFile, errors) = await _service.ProcessAudioRequestAsync(1, 2, title, 3, "pattern");

        // Assert
        Assert.Null(zipFile);
        Assert.Contains("Invalid voice selection.", errors);
    }

    [Fact]
    public async Task MarkTokenAsUsedAsync_ReturnsError_WhenTokenIsInvalid()
    {
        // Arrange
        var mockTokenRepository = new Mock<ITokenRepository>();
        mockTokenRepository
            .Setup(repo => repo.RetrieveByHashAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Token?)null);

        // Act
        var (success, errors) = await _service.MarkTokenAsUsedAsync("invalidToken");

        // Assert
        Assert.False(success);
        Assert.Contains("Invalid token.", errors);
    }
}