namespace TalkLikeTv.Services;

public static class ZipDirService
{
    public static FileInfo ZipStringsList(List<string> stringList, int max, string txtPath, string filename)
    {
        var chunkedPhrases = stringList.Chunk(max).Select(chunk => chunk.ToList()).ToList();
        _createPhrasesTxt(chunkedPhrases, txtPath, filename);
        return CreateZipFile(txtPath, filename);
    }

    private static void _createPhrasesTxt(IEnumerable<List<string>> chunkedPhrases, string zipPath, string filename)
    {
        try
        {
            var count = 0;
            foreach (var chunk in chunkedPhrases)
            {
                var filePath = Path.Combine(zipPath, $"{filename}-phrases-{count}.txt");
                count++;

                using (var writer = new StreamWriter(filePath))
                {
                    foreach (var phrase in chunk)
                    {
                        writer.WriteLine(phrase);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            var errorMessage = "An error occurred while creating phrase files.";
            Console.WriteLine($"{errorMessage} {ex.Message}");
            throw new Exception(errorMessage, ex);
        }
    }

    public static FileInfo CreateZipFile(string sourceDir, string filename)
    {
        // create a directory to hold the zip file
        Directory.CreateDirectory("/tmp/CreateZipFile/");

        var zipFilePath = Path.Combine("/tmp/CreateZipFile/", $"{filename}");

        try
        {
            if (File.Exists(zipFilePath))
            {
                File.Delete(zipFilePath);
            }

            System.IO.Compression.ZipFile.CreateFromDirectory(sourceDir, zipFilePath);
            Console.WriteLine("Directory successfully zipped to: " + zipFilePath);
        }
        catch (Exception ex)
        {
            var errorMessage = "An error occurred while creating the zip file.";
            Console.WriteLine($"{errorMessage} {ex.Message}");
            throw new Exception(errorMessage, ex);
        }

        return new FileInfo(zipFilePath);
    }
}