using FastEndpoints.Testing;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using TalkLikeTv.EntityModels;
using TalkLikeTv.Repositories;

namespace TalkLikeTv.FastEndpointsTests.UnitTests;

public class MyApp : AppFixture<FastEndpoints.Program>
{
    private readonly Mock<ITitleRepository> _mockTitleRepository = new();
    private readonly Mock<ILanguageRepository> _mockLanguageRepository = new();
    private readonly Mock<IVoiceRepository> _mockVoiceRepository = new();

    protected override ValueTask SetupAsync()
    {
        // Set up title repository mock with integer IDs
        _mockTitleRepository.Setup(repo => repo.RetrieveAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[]
            {
                new Title { TitleId = 1, TitleName = "Title1" },
                new Title { TitleId = 2, TitleName = "Title2" }
            });

        // Set up individual title retrieval
        _mockTitleRepository.Setup(repo => repo.RetrieveAsync("1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Title { TitleId = 1, TitleName = "Title1" });
        _mockTitleRepository.Setup(repo => repo.RetrieveAsync("2", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Title { TitleId = 2, TitleName = "Title2" });
        _mockTitleRepository.Setup(repo => repo.RetrieveAsync("999", It.IsAny<CancellationToken>()))
            .ReturnsAsync((Title)null);

        // Set up title retrieval by name
        _mockTitleRepository.Setup(repo => repo.RetrieveByNameAsync("Title1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Title { TitleId = 1, TitleName = "Title1" });
        _mockTitleRepository.Setup(repo => repo.RetrieveByNameAsync("Title2", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Title { TitleId = 2, TitleName = "Title2" });
        _mockTitleRepository.Setup(repo => repo.RetrieveByNameAsync("NonExistentTitle", It.IsAny<CancellationToken>()))
            .ReturnsAsync((Title)null);

        // Set up language repository mock with integer IDs
        _mockLanguageRepository.Setup(repo => repo.RetrieveAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Language[] { new() { LanguageId = 1, Name = "English" } });
        
        // Set up title creation
        _mockTitleRepository.Setup(repo => repo.CreateAsync(It.IsAny<Title>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Title title, CancellationToken ct) =>
            {
                // Return the same title but with an ID assigned
                title.TitleId = 3; // Assign a new ID
                return title;
            });
        
        _mockTitleRepository.Setup(repo => repo.UpdateAsync("1", It.IsAny<Title>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true); // Simulate successful update for ID "1"

        _mockTitleRepository.Setup(repo => repo.UpdateAsync("999", It.IsAny<Title>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        
        // Add mock setup for delete operations
        _mockTitleRepository.Setup(repo => repo.DeleteAsync("1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true); // Simulate successful deletion for ID "1"

        _mockTitleRepository.Setup(repo => repo.DeleteAsync("999", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false); // Simulate failed deletion for non-existent ID "999"

        _mockVoiceRepository.Setup(repo => repo.RetrieveAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[]
            {
                new Voice 
                { 
                    VoiceId = 1, 
                    DisplayName = "Voice1", 
                    LanguageId = 1,
                    Platform = "Platform1",
                    LocalName = "LocalName1",
                    ShortName = "Short1",
                    Gender = "Female",
                    Locale = "en-US",
                    LocaleName = "English (US)",
                    SampleRateHertz = 24000,
                    VoiceType = "Neural",
                    Status = "GA",
                    WordsPerMinute = 150
                },
                new Voice 
                { 
                    VoiceId = 2, 
                    DisplayName = "Voice2", 
                    LanguageId = 2,
                    Platform = "Platform2",
                    LocalName = "LocalName2",
                    ShortName = "Short2",
                    Gender = "Male",
                    Locale = "es-ES",
                    LocaleName = "Spanish (Spain)",
                    SampleRateHertz = 24000,
                    VoiceType = "Neural",
                    Status = "GA",
                    WordsPerMinute = 140
                }
            });
        
        _mockLanguageRepository.Setup(repo => repo.RetrieveAsync("1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Language { LanguageId = 1, Name = "English" });
        _mockLanguageRepository.Setup(repo => repo.RetrieveAsync("999", It.IsAny<CancellationToken>()))
            .ReturnsAsync((Language)null);
        _mockLanguageRepository.Setup(repo => repo.RetrieveByTagAsync("en-US", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Language { LanguageId = 1, Name = "English" });
        _mockLanguageRepository.Setup(repo => repo.RetrieveByTagAsync("xx-XX", It.IsAny<CancellationToken>()))
            .ReturnsAsync((Language)null);
        
        return ValueTask.CompletedTask;
    }

    protected override void ConfigureApp(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
    }

    protected override void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<ITitleRepository>(_mockTitleRepository.Object);
        services.AddSingleton<ILanguageRepository>(_mockLanguageRepository.Object);
        services.AddSingleton<IVoiceRepository>(_mockVoiceRepository.Object);
    }

    protected override ValueTask TearDownAsync()
    {
        return ValueTask.CompletedTask;
    }
}