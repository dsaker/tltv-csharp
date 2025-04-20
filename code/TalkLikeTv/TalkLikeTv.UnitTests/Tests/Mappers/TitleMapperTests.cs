using TalkLikeTv.EntityModels;
using TalkLikeTv.WebApi.Mappers;

namespace TalkLikeTv.UnitTests.Tests.Mappers;

public class TitleMapperTests
{
    [Fact]
    public void ToResponse_ShouldMapTitleToTitleResponse()
    {
        // Arrange
        var title = new Title
        {
            TitleId = 1,
            TitleName = "Test Title",
            Description = "Test Description",
            NumPhrases = 10,
            Popularity = 5,
            OriginalLanguageId = 2
        };

        // Act
        var result = TitleMapper.ToResponse(title);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(title.TitleId, result.TitleId);
        Assert.Equal(title.TitleName, result.TitleName);
        Assert.Equal(title.Description, result.Description);
        Assert.Equal(title.NumPhrases, result.NumPhrases);
        Assert.Equal(title.Popularity, result.Popularity);
        Assert.Equal(title.OriginalLanguageId, result.OriginalLanguageId);
    }

    [Fact]
    public void ToResponseList_ShouldMapTitleCollectionToTitleResponseCollection()
    {
        // Arrange
        var titles = new List<Title>
        {
            new Title
            {
                TitleId = 1,
                TitleName = "Title 1",
                Description = "Description 1",
                NumPhrases = 5,
                Popularity = 3,
                OriginalLanguageId = 1
            },
            new Title
            {
                TitleId = 2,
                TitleName = "Title 2",
                Description = "Description 2",
                NumPhrases = 8,
                Popularity = 4,
                OriginalLanguageId = 2
            }
        };

        // Act
        var result = TitleMapper.ToResponseList(titles).ToList();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(titles.Count, result.Count);

        for (int i = 0; i < titles.Count; i++)
        {
            Assert.Equal(titles[i].TitleId, result[i].TitleId);
            Assert.Equal(titles[i].TitleName, result[i].TitleName);
            Assert.Equal(titles[i].Description, result[i].Description);
            Assert.Equal(titles[i].NumPhrases, result[i].NumPhrases);
            Assert.Equal(titles[i].Popularity, result[i].Popularity);
            Assert.Equal(titles[i].OriginalLanguageId, result[i].OriginalLanguageId);
        }
    }
}