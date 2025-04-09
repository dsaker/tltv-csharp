namespace TalkLikeTv.WebUITests;

using Microsoft.Playwright;
using Xunit;

public class PlaywrightFixture : IAsyncLifetime
{ 
    private IPlaywright PlaywrightInstance { get; set; } = null!;
    public IBrowser Browser { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        PlaywrightInstance = await Playwright.CreateAsync();
        Browser = await PlaywrightInstance.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = false,
            Timeout = 10000
        });
    }

    // Modify your PlaywrightFixture.cs to ensure proper cleanup
    public async Task DisposeAsync()
    {
        try
        {
            // First close all contexts - use Contexts property instead of ContextsAsync()
            foreach (var context in Browser.Contexts)
            {
                await context.CloseAsync();
            }
            // Then close browser
            await Browser.CloseAsync();
            // Finally dispose playwright
            PlaywrightInstance.Dispose();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error disposing Playwright: {ex.Message}");
        }
    }
}