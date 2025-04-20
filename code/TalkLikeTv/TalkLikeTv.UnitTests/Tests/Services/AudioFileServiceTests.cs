using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using TalkLikeTv.EntityModels;
using TalkLikeTv.Repositories;
using TalkLikeTv.Services;
using TalkLikeTv.Services.Abstractions;

namespace TalkLikeTv.UnitTests.Tests.Services;

public class AudioFileServiceTests
{
    private readonly Mock<IPhraseService> _mockPhraseService;
    private readonly Mock<IPhraseRepository> _mockPhraseRepository;
    private readonly AudioFileService _service;

    public AudioFileServiceTests()
    {
        var mockLogger = new Mock<ILogger<AudioFileService>>();
        _mockPhraseService = new Mock<IPhraseService>();
        _mockPhraseRepository = new Mock<IPhraseRepository>();

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "TalkLikeTv:MaxPhrases", "100" },
                { "TalkLikeTv:BaseDir", "/base/dir/" }
            })
            .Build();

        _service = new AudioFileService(
            mockLogger.Object,
            _mockPhraseService.Object,
            _mockPhraseRepository.Object,
            configuration // Pass the properly initialized configuration
        );
    }

    [Fact]
    public void ExtractAndValidatePhraseStrings_ReturnsSuccess_WhenValid()
    {
        // Arrange
        var mockFile = new Mock<IFormFile>();
        var phrases = new List<string> { "Phrase1", "Phrase2" };
        _mockPhraseService.Setup(p => p.GetPhraseStrings(mockFile.Object))
            .Returns(new IPhraseService.PhraseResult { Success = true, Phrases = phrases });

        // Act
        var result = _service.ExtractAndValidatePhraseStrings(mockFile.Object);

        // Assert
        Assert.True(result.PhraseStrings.SequenceEqual(phrases));
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void ExtractAndValidatePhraseStrings_ReturnsError_WhenPhraseCountExceedsMax()
    {
        // Arrange
        var mockFile = new Mock<IFormFile>();
        var phrases = Enumerable.Range(1, 101).Select(i => $"Phrase{i}").ToList();
        _mockPhraseService.Setup(p => p.GetPhraseStrings(mockFile.Object))
            .Returns(new IPhraseService.PhraseResult { Success = true, Phrases = phrases });

        // Act
        var result = _service.ExtractAndValidatePhraseStrings(mockFile.Object);

        // Assert
        Assert.Contains("Phrase count exceeds the maximum of 100.", result.Errors);
    }

    [Fact]
    public async Task BuildAudioFilesAsync_ReturnsError_WhenPatternIsInvalid()
    {
        // Arrange
        var parameters = new IAudioFileService.BuildAudioFilesParams
        {
            Pattern = "InvalidPattern", // Corrected to match the expected error message
            Pause = 3,
            Title = new Title { TitleId = 1, TitleName = "TestTitle", NumPhrases = 2 },
            ToVoice = new Voice { ShortName = "Voice1" },
            FromVoice = new Voice { ShortName = "Voice2" },
            ToLang = new Language { Tag = "en" },
            FromLang = new Language { Tag = "fr" },
            TitleOutputPath = "/output/path"
        };

        // Act
        var result = await _service.BuildAudioFilesAsync(parameters);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("Pattern not found: InvalidPattern", result.Errors);
    }

    [Fact]
    public async Task BuildAudioFilesAsync_ReturnsError_WhenPauseValueIsInvalid()
    {
        // Arrange
        var parameters = new IAudioFileService.BuildAudioFilesParams
        {
            Pattern = "advanced",
            Pause = 99,
            Title = new Title { TitleId = 1, TitleName = "TestTitle", NumPhrases = 2 },
            ToVoice = new Voice { ShortName = "Voice1" },
            FromVoice = new Voice { ShortName = "Voice2" },
            ToLang = new Language { Tag = "en" },
            FromLang = new Language { Tag = "fr" },
            TitleOutputPath = "/tmp/path"
        };

        _mockPhraseRepository.Setup(repo => repo.GetPhrasesByTitleIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Phrase>());

        // Act
        var result = await _service.BuildAudioFilesAsync(parameters);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("Invalid pause value: 99", result.Errors);
    }
    
}