namespace TalkLikeTv.Services.Abstractions;

using Microsoft.AspNetCore.Http;

public interface IPhraseService
{
    PhraseResult GetPhraseStrings(IFormFile file);

    public class PhraseResult
    {
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
        public List<string>? Phrases { get; set; }
    }
}