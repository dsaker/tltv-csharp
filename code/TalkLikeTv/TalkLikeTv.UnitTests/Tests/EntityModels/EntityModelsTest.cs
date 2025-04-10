using TalkLikeTv.EntityModels; // To use TalkLikeTvContext.

namespace TalkLikeTv.UnitTests.Tests.EntityModels;

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

        var expected = 1;
        var actual = db.Languages.Count();

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void TitleIdMinus1IsNotATitle()
    {
        using TalkliketvContext db = new();

        var expected = "Not a Title";

        var title = db.Titles.Find(keyValues: -1);
        var actual = title?.TitleName ?? string.Empty;

        Assert.Equal(expected, actual);
    }
}