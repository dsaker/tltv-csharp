using System.IO.Compression;
using Microsoft.Playwright;
using Xunit.Abstractions;

namespace TalkLikeTv.WebUITests;

public class MvcWebUITests : IClassFixture<PlaywrightFixture>
{
    private IBrowserContext? _session;
    private IPage? _page;
    private IResponse? _response;
    private readonly PlaywrightFixture _fixture;
    private readonly ITestOutputHelper _output;

    public MvcWebUITests(PlaywrightFixture fixture, ITestOutputHelper output)
    {
        _fixture = fixture;
        _output = output;
    }
    
    private async Task GotoHomePage()
    {
        _session = await _fixture.Browser.NewContextAsync();
        _page = await _session.NewPageAsync();
        _page.SetDefaultTimeout(10000);
        _response = await _page.GotoAsync("https://localhost:7099/", new PageGotoOptions
        {
            Timeout = 10000
        });
    }

    [Fact]
    public async Task HomePage_Title()
    {
        // Arrange: Launch Chrome browser and navigate to home page.
        // using to make sure Dispose is called at the end of the test.
        await GotoHomePage();

        if (_page is null)
        {
            throw new NullReferenceException("Home page not found.");
        }

        var actualTitle = await _page.TitleAsync();

        // Assert: Navigating to home page worked and its title is as expected.
        var expectedTitle = "Home Page - TalkLikeTv";
        Assert.NotNull(_response);
        Assert.True(_response.Ok);
        Assert.Equal(expectedTitle, actualTitle);
    }
    
    [Fact]
    public async Task UploadFileAndParse()
    {
        await GotoHomePage();

        if (_page is null)
        {
            throw new NullReferenceException("Home page not found.");
        }

        // Act: Upload a file and submit the form.
        var filePath = Path.Combine(Environment.CurrentDirectory, "parsefile.srt");
        await _page.SetInputFilesAsync("[data-testid='parse_file_input']", filePath);
        // Capture the download URL
        var downloadTask = _page.RunAndWaitForDownloadAsync(async () =>
        {
            await _page.ClickAsync("[data-testid='parse_submit']");
        });

        var download = await downloadTask;
        var downloadPath = Path.Combine(Environment.CurrentDirectory, "downloaded.zip");
        await download.SaveAsAsync(downloadPath);

        // Assert: Check if the file was downloaded and its size is greater than 0.
        var fileInfo = new FileInfo(downloadPath);
        Assert.True(_getZippedFileCount(fileInfo) == 4, "The downloaded zip file should not be empty.");
    }
    
    private static int _getZippedFileCount(FileInfo zipFile)
    {
        if (zipFile is null or { Exists: false })
        {
            throw new FileNotFoundException("The zip file does not exist.");
        }

        using (var archive = ZipFile.OpenRead(zipFile.FullName))
        {
            return archive.Entries.Count;
        }
    }

    [Theory]
    [InlineData(1229, 692, "Desktop")] // Your current size
    [InlineData(375, 667, "Mobile", Skip = "Skip for now")] // iPhone 8
    [InlineData(768, 1024, "iPad", Skip = "Skip for now")]  // iPad
    public async Task CreateAudioFromFileThenFromTitle(int width, int height, string deviceType)
    {
        // Arrange: Navigate to home page
        await GotoHomePage();

        if (_page is null)
        {
            throw new NullReferenceException("Home page not found.");
        }

        // Set viewport size based on parameters
        await _page.SetViewportSizeAsync(width, height);
    
        _output.WriteLine($"Running test on {deviceType} viewport: {width}x{height}");
        
        // For mobile view, first expand the navbar
        if (width <= 576) { // Bootstrap's sm breakpoint
            await _page.ClickAsync(".navbar-toggler");
        }

        // Navigate to Audio page - now clickable
        await _page.ClickAsync("[data-testid='nav_audio']");

        // Select "from" language
        await _page.ClickAsync("input.form-check-input.language-radio[name='fromLanguage'][value='1']");

        // Select "from" voice
        await _page.ClickAsync("#from-voice-input-1");

        // Select "to" language 
        await _page.ClickAsync("input.form-check-input.language-radio[name='toLanguage'][value='106']");

        // Select "to" voice
        await _page.ClickAsync("#to-voice-input-472");

        // Set pause duration
        await _page.ClickAsync("#pauseDuration-3");

        // Select pattern
        await _page.ClickAsync("#pattern-standard");

        // Go to next step
        await _page.ClickAsync("#next-button");

        // Fill token
        await _page.FillAsync("#token", "PQQV5SBN7XONQZD26Z5TU763HU");

        // Fill title name (ensuring uniqueness)
        var randomTitle = "random_title_" + Guid.NewGuid().ToString("N")[..8];
        await _page.FillAsync("#titleName", randomTitle);

        // Fill description
        await _page.FillAsync("#description", "random description");

        // Upload file
        var filePath = Path.Combine(Environment.CurrentDirectory, "createtitle.txt");
        await _page.SetInputFilesAsync("#file", filePath);

        // Submit form and wait for download
        var downloadTask = _page.RunAndWaitForDownloadAsync(async () => { await _page.ClickAsync(".btn"); });

        var download = await downloadTask;
        var downloadPath = Path.Combine(Environment.CurrentDirectory, "downloaded.zip");
        await download.SaveAsAsync(downloadPath);

        // Assert the download was successful
        var fileInfo = new FileInfo(downloadPath);
        const long expectedSize = 2089636;
        const long tolerance = 10240; // 10KB tolerance
        Assert.True(Math.Abs(fileInfo.Length - expectedSize) <= tolerance,
            $"File size {fileInfo.Length} differs from expected {expectedSize} by more than {tolerance} bytes");

        // create another audio file from the title just created
        // Navigate to homepage
        await GotoHomePage();

        if (_page is null)
        {
            throw new NullReferenceException("Home page not found.");
        }

        // Set viewport size based on parameters
        await _page.SetViewportSizeAsync(width, height);

        // For mobile view, first expand the navbar
        if (width <= 576) { // Bootstrap's sm breakpoint
            await _page.ClickAsync(".navbar-toggler");
        }

        // Navigate to Audio page - now clickable
        await _page.ClickAsync("[data-testid='nav_title']");

        // Select using data-testid attribute
        await _page.ClickAsync("[data-testid='SearchByKeyword']");

        // Enter search keyword
        await _page.FillAsync("#Keyword", randomTitle);

        // Click search button
        await _page.ClickAsync("[data-testid='SearchButton']");

        // Click on the first title in the results
        await _page.ClickAsync("css=tr:nth-child(1) .btn");
        
        // Select "from" language
        await _page.ClickAsync("input.form-check-input.language-radio[name='fromLanguage'][value='13']");

        // Select "from" voice
        await _page.ClickAsync("#from-voice-input-50");

        // Select "to" language 
        await _page.ClickAsync("input.form-check-input.language-radio[name='toLanguage'][value='39']");

        // Select "to" voice
        await _page.ClickAsync("#to-voice-input-339");

        // Set pause duration to 4
        await _page.ClickAsync("#pauseDuration-4");

        // Select advanced pattern
        await _page.ClickAsync("#pattern-advanced");

        // Go to next step
        await _page.ClickAsync("#next-button");

        // Enter token for processing
        await _page.FillAsync("#token", "PQQV5SBN7XONQZD26Z5TU763HU");

        // Submit form and wait for download
        var titleTask = _page.RunAndWaitForDownloadAsync(async () => { await _page.ClickAsync(".btn"); });

        var titleDownload = await titleTask;
        var titlePath = Path.Combine(Environment.CurrentDirectory, "titleDownload.zip");
        await titleDownload.SaveAsAsync(titlePath);

        // Assert the download was successful
        var titleFileInfo = new FileInfo(titlePath);
        const long titleExpectedSize = 1623690;
        const long titleTolerance = 10240; // 10KB tolerance
        Assert.True(Math.Abs(titleFileInfo.Length - titleExpectedSize) <= titleTolerance,
            $"File size {titleFileInfo.Length} differs from expected {titleExpectedSize} by more than {titleTolerance} bytes");
    }
}