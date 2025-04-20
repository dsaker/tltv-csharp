using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;

namespace TalkLikeTv.UnitTests.Tests.Services;

public class ZipDirServiceTests
{
    private readonly MockFileSystem _mockFileSystem;
    private readonly TestableZipDirService _service;

    public ZipDirServiceTests()
    {
        _mockFileSystem = new MockFileSystem();
        _service = new TestableZipDirService(_mockFileSystem);
    }

    private class TestableZipDirService : ZipDirService
    {
        public TestableZipDirService(IFileSystem fileSystem) : base(fileSystem) { }

        protected override void CreateZipFromDirectory(string sourceDir, string destinationPath)
        {
            FileSystem.File.WriteAllBytes(destinationPath, Array.Empty<byte>());
        }
    }

    [Fact]
    public void ZipStringsList_ShouldCreateCorrectTextFiles()
    {
        var testDir = "/test/directory";
        var testFilename = "test-file";
        var phrases = new List<string> { "Test phrase 1", "Test phrase 2", "Test phrase 3" };
        var chunkSize = 2;

        _mockFileSystem.AddDirectory(testDir);

        var result = _service.ZipStringsList(phrases, chunkSize, testDir, testFilename);

        Assert.NotNull(result);
        Assert.True(_mockFileSystem.FileExists(Path.Combine(testDir, $"{testFilename}-phrases-0.txt")));
        Assert.True(_mockFileSystem.FileExists(Path.Combine(testDir, $"{testFilename}-phrases-1.txt")));

        var file0Content = _mockFileSystem.GetFile(Path.Combine(testDir, $"{testFilename}-phrases-0.txt")).TextContents;
        Assert.Contains("Test phrase 1", file0Content as string);
        Assert.Contains("Test phrase 2", file0Content as string);

        var file1Content = _mockFileSystem.GetFile(Path.Combine(testDir, $"{testFilename}-phrases-1.txt")).TextContents;
        Assert.Contains("Test phrase 3", file1Content as string);
    }

    [Fact]
    public void CreateZipFile_ShouldCreateZipFileInCorrectLocation()
    {
        var sourceDir = "/source/directory";
        var filename = "test-archive";

        _mockFileSystem.AddDirectory(sourceDir);
        _mockFileSystem.AddFile(Path.Combine(sourceDir, "test.txt"), new MockFileData("test content"));

        var result = _service.CreateZipFile(sourceDir, filename);

        Assert.NotNull(result);
        Assert.Equal("/tmp/CreateZipFile/test-archive", result.FullName as string);
        Assert.True(_mockFileSystem.Directory.Exists("/tmp/CreateZipFile/"));
        Assert.True(_mockFileSystem.FileExists("/tmp/CreateZipFile/test-archive"));
    }

    [Fact]
    public void CreateZipFile_ShouldOverwriteExistingZipFile()
    {
        var sourceDir = "/source/directory";
        var filename = "test-archive";

        _mockFileSystem.AddDirectory(sourceDir);
        _mockFileSystem.AddDirectory("/tmp/CreateZipFile/");
        _mockFileSystem.AddFile("/tmp/CreateZipFile/test-archive", new MockFileData("existing content"));

        var result = _service.CreateZipFile(sourceDir, filename);

        Assert.NotNull(result);
        Assert.Equal("/tmp/CreateZipFile/test-archive", result.FullName as string);
        Assert.True(_mockFileSystem.FileExists("/tmp/CreateZipFile/test-archive"));
    }
}