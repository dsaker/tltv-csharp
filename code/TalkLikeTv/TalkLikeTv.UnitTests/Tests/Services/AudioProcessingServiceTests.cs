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
    private readonly Mock<IAzureTranslateService> _mockTranslateService;
    private readonly Mock<ILanguageRepository> _mockLanguageRepository;
    private readonly Mock<IZipDirService> _mockZipDirService;
    private readonly Mock<IVoiceRepository> _mockVoiceRepository;
    
    public AudioProcessingServiceTests()
    {
        var mockLogger = new Mock<ILogger<AudioProcessingService>>();
        var mockTranslationService = new Mock<ITranslationService>();
        _mockLanguageRepository = new Mock<ILanguageRepository>();
        var mockTokenRepository = new Mock<ITokenRepository>();
        var mockTitleRepository = new Mock<ITitleRepository>();
        var mockTranslateRepository = new Mock<ITranslateRepository>();
        _mockTranslateService = new Mock<IAzureTranslateService>();
        _mockZipDirService = new Mock<IZipDirService>();
        _mockVoiceRepository = new Mock<IVoiceRepository>();
        var mockPhraseRepository = new Mock<IPhraseRepository>();
        var mockAudioFileService = new Mock<IAudioFileService>();

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "Talkliketv:AudioOutputDir", "/output/dir" }
            })
            .Build();

        _service = new AudioProcessingService(
            mockLogger.Object,
            mockTranslationService.Object,
            mockAudioFileService.Object,
            _mockVoiceRepository.Object,
            _mockLanguageRepository.Object,
            mockTokenRepository.Object,
            mockTitleRepository.Object,
            mockPhraseRepository.Object,
            mockTranslateRepository.Object,
            _mockTranslateService.Object,
            _mockZipDirService.Object,
            configuration
        );
    }

    [Fact]
    public async Task DetectLanguageAsync_ReturnsLanguage_WhenValidPhrasesProvided()
    {
        // Arrange
        _mockTranslateService
            .Setup(service => service.DetectLanguageFromPhrasesAsync(It.IsAny<List<string>>(), It.IsAny<CancellationToken>()))
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
        _mockVoiceRepository
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
    
    [Fact]
    public async Task ProcessAudioRequestAsync_CreatesZipFile_WhenAllStepsSucceed()
    {
        // Arrange
        var toVoice = new Voice { VoiceId = 1, ShortName = "voice1", LanguageId = 1 };
        var fromVoice = new Voice { VoiceId = 2, ShortName = "voice2", LanguageId = 2 };
        
        _mockVoiceRepository
            .Setup(repo => repo.RetrieveAsync("1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(toVoice);
        _mockVoiceRepository
            .Setup(repo => repo.RetrieveAsync("2", It.IsAny<CancellationToken>()))
            .ReturnsAsync(fromVoice);
        
        _mockLanguageRepository
            .Setup(repo => repo.RetrieveAsync("1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Language { LanguageId = 1, Tag = "en" });
        _mockLanguageRepository
            .Setup(repo => repo.RetrieveAsync("2", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Language { LanguageId = 2, Tag = "fr" });
        
        // Mock the translation service
        var mockTranslationService = new Mock<ITranslationService>();
        mockTranslationService
            .Setup(ts => ts.ProcessTranslations(It.IsAny<TranslationService.ProcessTranslationsParams>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((true, new List<string>()));
        
        // Mock the audio file service
        var mockAudioFileService = new Mock<IAudioFileService>();
        mockAudioFileService
            .Setup(afs => afs.BuildAudioFilesAsync(It.IsAny<IAudioFileService.BuildAudioFilesParams>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new IAudioFileService.AudioFileResult { Success = true, Errors = new List<string>() });
        
        _mockZipDirService
            .Setup(zds => zds.CreateZipFile(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(new FileInfo("/tmp/CreateZipFile/test-file.zip"));
            
        var title = new Title { TitleId = 1, TitleName = "TestTitle" };
        
        // Create a new service with the mocked dependencies
        var service = new AudioProcessingService(
            Mock.Of<ILogger<AudioProcessingService>>(),
            mockTranslationService.Object,
            mockAudioFileService.Object,
            _mockVoiceRepository.Object,
            _mockLanguageRepository.Object,
            Mock.Of<ITokenRepository>(),
            Mock.Of<ITitleRepository>(),
            Mock.Of<IPhraseRepository>(),
            Mock.Of<ITranslateRepository>(),
            Mock.Of<IAzureTranslateService>(),
            _mockZipDirService.Object,
            new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    { "TalkLikeTv:AudioOutputDir", "/output/dir" }
                })
                .Build()
        );
        
        // Act
        var (zipFile, errors) = await service.ProcessAudioRequestAsync(1, 2, title, 3, "pattern");
        
        // Assert
        Assert.NotNull(zipFile);
        Assert.Empty(errors);
        _mockZipDirService.Verify(zds => zds.CreateZipFile(
                It.IsAny<string>(), 
                It.Is<string>(s => s.Contains("TestTitle"))), 
            Times.Once);
    }
}