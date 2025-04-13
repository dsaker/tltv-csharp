using DotEnv.Core;
using TalkLikeTv.EntityModels; // To use TalkLikeTvContext.

namespace TalkLikeTv.UnitTests.Tests.EntityModels;

public class EntityModelTests
{
    private TalkliketvContext CreateDbContext()
    {
        var currentDirectory = Directory.GetCurrentDirectory();
        Console.WriteLine($"Current directory: {currentDirectory}");
        
        // Load .env file from test project or reference the MVC project's file
        var envPath = Path.Combine(Directory.GetCurrentDirectory(), "../../../../TalkLikeTv.Mvc/.env"); 
    
        Assert.True(File.Exists(envPath), "The .env file does not exist in the expected location.");
        
        new EnvLoader()
            .SetBasePath(Path.GetDirectoryName(envPath))
            .AddEnvFile(Path.GetFileName(envPath))
            .Load();

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