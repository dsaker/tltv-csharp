using Moq;
using TalkLikeTv.EntityModels;
using TalkLikeTv.Repositories;
using TalkLikeTv.Services;

namespace TalkLikeTv.UnitTests.Tests.Services;

public class TitleValidationServiceTests
{
    [Fact]
    public async Task ValidateAsync_ShouldReturnError_WhenDuplicateTitleExists()
    {
        // Arrange
        var mockTitleRepository = new Mock<ITitleRepository>();
        var mockLanguageRepository = new Mock<ILanguageRepository>();
        var title = new Title { TitleName = "Duplicate Title" };

        mockTitleRepository
            .Setup(repo => repo.RetrieveByNameAsync(title.TitleName, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Title { TitleName = "Duplicate Title" });

        var service = new TitleValidationService(mockTitleRepository.Object, mockLanguageRepository.Object);

        // Act
        var (isValid, errors) = await service.ValidateAsync(title);

        // Assert
        Assert.False(isValid);
        Assert.Contains("A title with name 'Duplicate Title' already exists", errors);
    }

    [Fact]
    public async Task ValidateAsync_ShouldReturnError_WhenLanguageDoesNotExist()
    {
        // Arrange
        var mockTitleRepository = new Mock<ITitleRepository>();
        var mockLanguageRepository = new Mock<ILanguageRepository>();
        var title = new Title { OriginalLanguageId = 1 };

        mockLanguageRepository
            .Setup(repo => repo.RetrieveAsync("1", It.IsAny<CancellationToken>()))
            .ReturnsAsync((Language)null);

        var service = new TitleValidationService(mockTitleRepository.Object, mockLanguageRepository.Object);

        // Act
        var (isValid, errors) = await service.ValidateAsync(title);

        // Assert
        Assert.False(isValid);
        Assert.Contains("Language with ID 1 does not exist", errors);
    }

    [Fact]
    public async Task ValidateAsync_ShouldReturnValid_WhenTitleIsValid()
    {
        // Arrange
        var mockTitleRepository = new Mock<ITitleRepository>();
        var mockLanguageRepository = new Mock<ILanguageRepository>();
        var title = new Title { TitleName = "Valid Title", OriginalLanguageId = 1 };

        mockTitleRepository
            .Setup(repo => repo.RetrieveByNameAsync(title.TitleName, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Title)null);

        mockLanguageRepository
            .Setup(repo => repo.RetrieveAsync("1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Language { LanguageId = 1 });

        var service = new TitleValidationService(mockTitleRepository.Object, mockLanguageRepository.Object);

        // Act
        var (isValid, errors) = await service.ValidateAsync(title);

        // Assert
        Assert.True(isValid);
        Assert.Empty(errors);
    }
}