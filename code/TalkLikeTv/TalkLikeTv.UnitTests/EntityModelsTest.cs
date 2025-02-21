using TalkLikeTv.EntityModels; // To use TalkLikeTvContext.

namespace TalkLikeTv.UnitTests;

public class EntityModelTests
{
    [Fact]
    public void DatabaseConnectTest()
    {
        using TalkliketvContext db = new();
        Assert.True(db.Database.CanConnect());
    }

    [Fact]
    public void LanguagesCountTest()
    {
        using TalkliketvContext db = new();

        int expected = 1;
        int actual = db.Languages.Count();

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void ProductId1IsChaiTest()
    {
        using TalkliketvContext db = new();

        string expected = "Not a Title"
            ;

        Title? title = db.Titles.Find(keyValues: -1);
        string actual = title?.Title1 ?? string.Empty;

        Assert.Equal(expected, actual);
    }
}