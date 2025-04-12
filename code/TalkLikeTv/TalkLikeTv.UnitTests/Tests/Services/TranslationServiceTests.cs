using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using TalkLikeTv.EntityModels;
using TalkLikeTv.Repositories;
using TalkLikeTv.Services;
using TalkLikeTv.Services.Abstractions;

namespace TalkLikeTv.UnitTests.Tests.Services;

public class TranslationServiceTests
{
    private readonly Mock<IPhraseRepository> _mockPhraseRepository;
    private readonly Mock<ITranslateRepository> _mockTranslateRepository;
    private readonly Mock<ILanguageRepository> _mockLanguageRepository;
    private readonly Mock<IAzureTranslateService> _mockAzureTranslateService;
    private readonly Mock<IAzureTextToSpeechService> _mockAzureTextToSpeechService;
    private readonly Mock<ILogger<TranslationService>> _mockLogger;
    private readonly IConfiguration _configuration;
    private readonly TranslationService _service;

    public TranslationServiceTests()
    {
        _mockPhraseRepository = new Mock<IPhraseRepository>();
        _mockTranslateRepository = new Mock<ITranslateRepository>();
        _mockLanguageRepository = new Mock<ILanguageRepository>();
        _mockAzureTranslateService = new Mock<IAzureTranslateService>();
        _mockAzureTextToSpeechService = new Mock<IAzureTextToSpeechService>();
        _mockLogger = new Mock<ILogger<TranslationService>>();

        // In your test constructor
        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "TalkLikeTv:BaseDir", Path.Combine(Path.GetTempPath(), "tltv-test") }
            })
            .Build();

        _service = new TranslationService(
            _mockLogger.Object,
            _mockTranslateRepository.Object,
            _mockPhraseRepository.Object,
            _mockLanguageRepository.Object,
            _mockAzureTranslateService.Object,
            _mockAzureTextToSpeechService.Object,
            _configuration
        );
    }

    [Fact]
    public async Task ProcessTranslations_ShouldReturnError_WhenPhrasesCountDoesNotMatch()
    {
        // Arrange
        var title = new Title { TitleId = 1, NumPhrases = 5 };
        _mockPhraseRepository
            .Setup(repo => repo.GetPhrasesByTitleIdAsync(title.TitleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Phrase> { new Phrase(), new Phrase() }); // Only 2 phrases

        var parameters = new TranslationService.ProcessTranslationsParams
        {
            Title = title,
            ToVoice = new Voice(),
            FromVoice = new Voice(),
            ToLang = new Language(),
            FromLang = new Language()
        };

        // Act
        var (success, errors) = await _service.ProcessTranslations(parameters, CancellationToken.None);

        // Assert
        Assert.False(success);
        Assert.Contains("Phrases count must equal title.NumPhrases.", errors);
    }

    [Fact]
    public async Task ProcessTranslations_ShouldReturnError_WhenOriginalLanguageIsMissing()
    {
        // Arrange
        var title = new Title { TitleId = 1, NumPhrases = 2, OriginalLanguageId = null };
        _mockPhraseRepository
            .Setup(repo => repo.GetPhrasesByTitleIdAsync(title.TitleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Phrase> { new Phrase(), new Phrase() });

        var parameters = new TranslationService.ProcessTranslationsParams
        {
            Title = title,
            ToVoice = new Voice(),
            FromVoice = new Voice(),
            ToLang = new Language(),
            FromLang = new Language()
        };

        // Act
        var (success, errors) = await _service.ProcessTranslations(parameters, CancellationToken.None);

        // Assert
        Assert.False(success);
        Assert.Contains("Original language ID is null.", errors);
    }

    [Fact]
    public async Task ProcessTranslations_ShouldReturnSuccess_WhenProcessingIsSuccessful()
    {
        // Arrange
        var title = new Title { TitleId = 1, NumPhrases = 2, OriginalLanguageId = 1, TitleName = "Test Title" };
        var phrases = new List<Phrase>
        {
            new Phrase { PhraseId = 1 },
            new Phrase { PhraseId = 2 }
        };
        var originalTranslations = new List<Translate>
        {
            new Translate { PhraseId = 1, Phrase = "Original 1" },
            new Translate { PhraseId = 2, Phrase = "Original 2" }
        };
        var translatedPhrases = new List<string> { "Translated 1", "Translated 2" };

        _mockPhraseRepository
            .Setup(repo => repo.GetPhrasesByTitleIdAsync(title.TitleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(phrases);

        _mockTranslateRepository
            .SetupSequence(repo => repo.GetTranslatesByLanguageAndPhrasesAsync(It.IsAny<int>(), It.IsAny<List<int>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(originalTranslations)
            .ReturnsAsync(originalTranslations)
            .ReturnsAsync(new List<Translate>());

        _mockLanguageRepository
            .Setup(repo => repo.RetrieveAsync("1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Language { LanguageId = 1, Tag = "en" });

        _mockAzureTranslateService
            .SetupSequence(service => service.TranslatePhrasesAsync(It.IsAny<List<string>>(), "en", "fr"))
            .ReturnsAsync(translatedPhrases);

        _mockAzureTextToSpeechService
            .Setup(service => service.GenerateSpeechToFileAsync(It.IsAny<string>(), It.IsAny<Voice>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        var parameters = new TranslationService.ProcessTranslationsParams
        {
            Title = title,
            ToVoice = new Voice { ShortName = "Voice1" },
            FromVoice = new Voice { ShortName = "Voice2" },
            ToLang = new Language { LanguageId = 2, Tag = "fr" },
            FromLang = new Language { LanguageId = 1, Tag = "en" }
        };

        // Act
        var (success, errors) = await _service.ProcessTranslations(parameters, CancellationToken.None);

        // Assert
        Assert.True(success);
        Assert.Empty(errors);

        // Verify that GenerateSpeechToFileAsync was called for each translation
        _mockAzureTextToSpeechService.Verify(
            service => service.GenerateSpeechToFileAsync(It.IsAny<string>(), It.IsAny<Voice>(), It.IsAny<string>()),
            Times.Exactly(4) // 2 for ToVoice and 2 for FromVoice
        );
    }
}