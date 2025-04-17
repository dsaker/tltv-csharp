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
            .ReturnsAsync(false); //

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
    }

    protected override ValueTask TearDownAsync()
    {
        return ValueTask.CompletedTask;
    }
}