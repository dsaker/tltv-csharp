using Microsoft.AspNetCore.Http;

namespace TalkLikeTv.Services;

public class PhraseService
{
    public List<string>? GetPhraseStrings(IFormFile file)
    {
        ArgumentNullException.ThrowIfNull(file, nameof(file));

        var fileStream = file.OpenReadStream();
        if (fileStream == null)
        {
            throw new Exception("Invalid file stream.");
        }

        // Check if the content is single line
        if (TextFormatDetector.DetectTextFormat(fileStream) != TextFormatDetector.TextFormat.OnePhrasePerLine)
        {
            throw new InvalidDataException("Invalid file format. Please parse the file at the home page.");
        }

        fileStream.Seek(0, SeekOrigin.Begin);
        using var reader = new StreamReader(fileStream);
        var parsedPhrases = ParseService.ParseOnePhrasePerLine(reader);

        if (parsedPhrases.Count > 0)
        {
            return parsedPhrases;
        }

        return null;
    }
}