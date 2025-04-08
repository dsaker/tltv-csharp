using Microsoft.AspNetCore.Http;

namespace TalkLikeTv.Services;

public class PhraseService
{
    public class PhraseResult
    {
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
        public List<string>? Phrases { get; set; }
    }

    public PhraseResult GetPhraseStrings(IFormFile file)
    {
        if (file == null)
        {
            return new PhraseResult { Success = false, ErrorMessage = "File is null." };
        }

        var fileStream = file.OpenReadStream();
        if (fileStream == null)
        {
            return new PhraseResult { Success = false, ErrorMessage = "Invalid file stream." };
        }

        if (TextFormatDetector.DetectTextFormat(fileStream) != TextFormatDetector.TextFormat.OnePhrasePerLine)
        {
            return new PhraseResult { Success = false, ErrorMessage = "Invalid file format. Please parse the file at the home page." };
        }

        fileStream.Seek(0, SeekOrigin.Begin);
        using var reader = new StreamReader(fileStream);
        var parsedPhrases = ParseService.ParseOnePhrasePerLine(reader);

        if (parsedPhrases.Count > 0)
        {
            return new PhraseResult { Success = true, Phrases = parsedPhrases };
        }

        return new PhraseResult { Success = false, ErrorMessage = "No phrases found in the file." };
    }
}