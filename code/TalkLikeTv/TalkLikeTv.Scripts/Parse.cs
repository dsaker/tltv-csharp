

namespace TalkLikeTv.FileService;


public static class FileService
{
    public static List<string> ParseFile(FileStream? fileStream)
    {

        if (fileStream == null)
        {
            throw new Exception("Unable to parse file: fileStream is null");
        }

        Console.WriteLine($"File size: {fileStream.Length} bytes");

        if (fileStream.Length > 8192)
        {
            throw new Exception($"File too large ({fileStream.Length} > 8192 bytes)");
        }

        return new List<string>();
    }
}
