using TalkLikeTv.Services;
using System.Text;

namespace TalkLikeTv.UnitTests.Tests.Services;

public class TextFormatDetectorTests
{
    [Fact]
    public void DetectTextFormat_ShouldReturnSrt_WhenFileIsInSrtFormat()
    {
        // Arrange
        var srtContent = "1\n00:00:01,000 --> 00:00:02,000\nHello world\n";
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(srtContent));

        // Act
        var result = TextFormatDetector.DetectTextFormat(stream);

        // Assert
        Assert.Equal(TextFormatDetector.TextFormat.Srt, result);
    }

    [Fact]
    public void DetectTextFormat_ShouldReturnOnePhrasePerLine_WhenFileHasShortLines()
    {
        // Arrange
        var onePhrasePerLineContent = "Phrase 1\nPhrase 2\nPhrase 3\nPhrase 4\n";
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(onePhrasePerLineContent));

        // Act
        var result = TextFormatDetector.DetectTextFormat(stream);

        // Assert
        Assert.Equal(TextFormatDetector.TextFormat.OnePhrasePerLine, result);
    }

    [Fact]
    public void DetectTextFormat_ShouldReturnParagraph_WhenFileHasLongLines()
    {
        // Arrange
        var paragraphContent = "This is a long paragraph that exceeds the typical length of a single line.\n";
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(paragraphContent));

        // Act
        var result = TextFormatDetector.DetectTextFormat(stream);

        // Assert
        Assert.Equal(TextFormatDetector.TextFormat.Paragraph, result);
    }

    [Fact]
    public void DetectTextFormat_ShouldReturnParagraph_WhenFileHasFewLines()
    {
        // Arrange
        var fewLinesContent = "Line 1\nLine 2\nLine 3\n";
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(fewLinesContent));

        // Act
        var result = TextFormatDetector.DetectTextFormat(stream);

        // Assert
        Assert.Equal(TextFormatDetector.TextFormat.Paragraph, result);
    }

    [Fact]
    public void DetectTextFormat_ShouldThrowArgumentNullException_WhenStreamIsNull()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => TextFormatDetector.DetectTextFormat(null));
    }
}