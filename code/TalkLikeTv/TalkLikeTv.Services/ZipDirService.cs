using System.IO.Abstractions;
using TalkLikeTv.Services.Abstractions;

public class ZipDirService : IZipDirService
{
    protected readonly IFileSystem FileSystem;

    public ZipDirService(IFileSystem fileSystem)
    {
        FileSystem = fileSystem;
    }

    public FileInfo CreateZipFile(string sourceDir, string filename)
    {
        try
        {
            var zipOutputDirectory = "/tmp/CreateZipFile";
            var zipFilePath = Path.Combine(zipOutputDirectory, filename);

            if (!FileSystem.Directory.Exists(zipOutputDirectory))
            {
                FileSystem.Directory.CreateDirectory(zipOutputDirectory);
            }

            // Use CreateZipFromDirectory which can be overridden in tests
            CreateZipFromDirectory(sourceDir, zipFilePath);

            return new FileInfo(zipFilePath);
        }
        catch (Exception ex)
        {
            throw new Exception("An error occurred while creating the zip file.", ex);
        }
    }

    protected virtual void CreateZipFromDirectory(string sourceDir, string destinationPath)
    {
        // The real implementation still uses System.IO.Compression
        System.IO.Compression.ZipFile.CreateFromDirectory(sourceDir, destinationPath);
    }
    
    public FileInfo ZipStringsList(List<string> stringList, int max, string txtPath, string filename)
    {
        var chunkedPhrases = stringList.Chunk(max).Select(chunk => chunk.ToList()).ToList();
        CreatePhrasesTxt(chunkedPhrases, txtPath, filename);
        return CreateZipFile(txtPath, filename);
    }

    private void CreatePhrasesTxt(IEnumerable<List<string>> chunkedPhrases, string zipPath, string filename)
    {
        try
        {
            FileSystem.Directory.CreateDirectory(zipPath);

            var count = 0;
            foreach (var chunk in chunkedPhrases)
            {
                var filePath = Path.Combine(zipPath, $"{filename}-phrases-{count}.txt");
                count++;

                using var writer = new StreamWriter(FileSystem.File.Create(filePath));
                foreach (var phrase in chunk)
                {
                    writer.WriteLine(phrase);
                }
            }
        }
        catch (Exception ex)
        {
            var errorMessage = "An error occurred while creating phrase files.";
            throw new Exception(errorMessage, ex);
        }
    }
}