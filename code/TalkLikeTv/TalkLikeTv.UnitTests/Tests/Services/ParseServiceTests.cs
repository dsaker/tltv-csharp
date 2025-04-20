using TalkLikeTv.Services;
using System.Text;
using Moq;
using System.IO.Abstractions;
using TalkLikeTv.Services.Abstractions;

namespace TalkLikeTv.UnitTests.Tests.Services;

public class ParseServiceTests
{
    private readonly ParseService _parseService;
    private readonly Mock<IZipDirService> _mockZipDirService;

    public ParseServiceTests()
    {
        _mockZipDirService = new Mock<IZipDirService>();
        
        _mockZipDirService
            .Setup(z => z.ZipStringsList(
                It.IsAny<List<string>>(),
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<string>()))
            .Returns(new FileInfo(Path.Combine("/tmp/ParseFile", "test.zip")));
            
        _parseService = new ParseService(_mockZipDirService.Object);
    }

    [Fact]
    public void ParseFile_ShouldReturnError_WhenFileTooLarge()
    {
        // Arrange
        var largeFileStream = new MemoryStream(new byte[8192 * 9]); // Exceeds size limit
        var fileName = "test.txt";

        // Act
        var result = _parseService.ParseFile(largeFileStream, fileName, 10);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("File too large (73728 > 65536 bytes)", result.ErrorMessage);
    }

    [Fact]
    public void ParseFile_ShouldReturnSuccess_WhenFileIsValid()
    {
        // Arrange
        var validFileStream = new MemoryStream(Encoding.UTF8.GetBytes("Phrase 1\nPhrase 2\nPhrase 3"));
        var fileName = "test.txt";

        // Act
        var result = _parseService.ParseFile(validFileStream, fileName, 10);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.File);
    }

    [Fact]
    public void ParseOnePhrasePerLine_ShouldParseCorrectly()
    {
        // Arrange
        var input = "this is sentence one\nthis is sentence two\nthis is sentence three";
        var reader = new StreamReader(new MemoryStream(Encoding.UTF8.GetBytes(input)));

        // Act - note the static method call
        var result = ParseService.ParseOnePhrasePerLine(reader);

        // Assert
        Assert.Equal(3, result.Count);
        Assert.Contains("this is sentence one", result);
        Assert.Contains("this is sentence two", result);
        Assert.Contains("this is sentence three", result);
    }

    [Fact]
    public void ParseParagraph_ShouldSplitOnEndingPunctuation()
    {
        // Arrange
        var input = "This is a sentence. This is another sentence!";
        var reader = new StreamReader(new MemoryStream(Encoding.UTF8.GetBytes(input)));

        // Act - note the static method call
        var result = ParseService.ParseOnePhrasePerLine(reader);

        // Assert
        Assert.Single(result);
        Assert.Contains("This is a sentence. This is another sentence!", result);
    }

    [Fact]
    public void ParseParagraph_ShouldSplitOnEndingPunctuationTwo()
    {
        // Arrange
        var input = "This is a longer sentence. This is another longer sentence!";
        var reader = new StreamReader(new MemoryStream(Encoding.UTF8.GetBytes(input)));

        // Act - note the static method call
        var result = ParseService.ParseOnePhrasePerLine(reader);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Contains("This is a longer sentence.", result);
        Assert.Contains("This is another longer sentence!", result);
    }

    [Fact]
    public void ParseSrt_ShouldSkipInvalidLines()
    {
        // Arrange
        var input = "1\n00:00:01,000 --> 00:00:02,000\n[Music]\nHello world but longer\n";
        var reader = new StreamReader(new MemoryStream(Encoding.UTF8.GetBytes(input)));

        // Act - note the static method call
        var result = ParseService.ParseOnePhrasePerLine(reader);

        // Assert
        Assert.Single(result);
        Assert.Contains("Hello world but longer", result);
    }
}