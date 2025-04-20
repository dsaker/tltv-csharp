using Microsoft.AspNetCore.Http;
using Moq;
using System.Text;
using TalkLikeTv.Services;

namespace TalkLikeTv.UnitTests.Tests.Services;

public class PhraseServiceTests
{
    [Fact]
    public void GetPhraseStrings_ShouldReturnError_WhenFileFormatIsInvalid()
    {
        // Arrange
        var mockFile = new Mock<IFormFile>();
        var content = "Invalid content";
        var fileName = "test.txt";
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));
        mockFile.Setup(f => f.OpenReadStream()).Returns(stream);
        mockFile.Setup(f => f.FileName).Returns(fileName);

        var service = new PhraseService();

        // Act
        var result = service.GetPhraseStrings(mockFile.Object);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("Invalid file format. Please parse the file at the home page.", result.ErrorMessage);
    }

    [Fact]
    public void GetPhraseStrings_ShouldReturnError_WhenFileIsEmpty()
    {
        // Arrange
        var mockFile = new Mock<IFormFile>();
        var content = "";
        var fileName = "empty.txt";
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));
        mockFile.Setup(f => f.OpenReadStream()).Returns(stream);
        mockFile.Setup(f => f.FileName).Returns(fileName);

        var service = new PhraseService();

        // Act
        var result = service.GetPhraseStrings(mockFile.Object);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("No phrases found in the file.", result.ErrorMessage);
    }

    [Fact]
    public void GetPhraseStrings_ShouldReturnSuccess_WhenFileIsValid()
    {
        // Arrange
        var mockFile = new Mock<IFormFile>();
        var content = "this is sentence one\nthis is sentence two\nthis is sentence three\nthis is sentence four";
        var fileName = "valid.txt";
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));
        mockFile.Setup(f => f.OpenReadStream()).Returns(stream);
        mockFile.Setup(f => f.FileName).Returns(fileName);

        var service = new PhraseService();

        // Act
        var result = service.GetPhraseStrings(mockFile.Object);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Phrases);
        Assert.Equal(4, result.Phrases.Count);
    }
}