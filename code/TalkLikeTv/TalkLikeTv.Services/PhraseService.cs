using Microsoft.AspNetCore.Http;
using TalkLikeTv.Services.Abstractions;

namespace TalkLikeTv.Services;

public class PhraseService : IPhraseService
{

    public IPhraseService.PhraseResult GetPhraseStrings(IFormFile file)
    {

        var fileStream = file.OpenReadStream();
        
        // Check if the file is empty
        if (fileStream.Length == 0)
        {
            return new IPhraseService.PhraseResult { Success = false, ErrorMessage = "No phrases found in the file." };
        }

        if (TextFormatDetector.DetectTextFormat(fileStream) != TextFormatDetector.TextFormat.OnePhrasePerLine)
        {
            return new IPhraseService.PhraseResult { Success = false, ErrorMessage = "Invalid file format. Please parse the file at the home page." };
        }

        fileStream.Seek(0, SeekOrigin.Begin);
        using var reader = new StreamReader(fileStream);
        var parsedPhrases = ParseService.ParseOnePhrasePerLine(reader);

        if (parsedPhrases.Count == 0)
        {
            return new IPhraseService.PhraseResult { Success = false, ErrorMessage = "No phrases found in the file." };
        }

        return new IPhraseService.PhraseResult { Success = true, Phrases = parsedPhrases };
    }
}