using DotEnv.Core;
using TalkLikeTv.EntityModels; // To use TalkLikeTvContext.

namespace TalkLikeTv.IntegrationTests.EntityModels;

public class EntityModelTests
{
    private TalkliketvContext CreateDbContext()
    {
        return new TalkliketvContext();
    }

    [Fact]
    public void DatabaseConnectTest()
    {
        using var db = CreateDbContext();
        Assert.True(db.Database.CanConnect());
    }

    [Fact]
    public void LanguagesCountTest()
    {
        using var db = CreateDbContext();

        var expected = 78;
        var actual = db.Languages.Count();

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void TitleIdMinus1IsNotATitle()
    {
        using var db = CreateDbContext();

        var expected = "Not a Title";

        var title = db.Titles.Find(keyValues: -1);
        var actual = title?.TitleName ?? string.Empty;

        Assert.Equal(expected, actual);
    }
}