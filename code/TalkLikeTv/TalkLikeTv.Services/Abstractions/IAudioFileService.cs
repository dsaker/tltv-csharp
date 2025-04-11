using Microsoft.AspNetCore.Http;
using TalkLikeTv.EntityModels;

namespace TalkLikeTv.Services.Abstractions;

public interface IAudioFileService
{
    // Define result types outside of implementation classes
    public class ExtractAndValidateResult
    {
        public List<string>? PhraseStrings { get; set; }
        public List<string> Errors { get; set; } = new();
    }
    
    public class AudioFileResult
    {
        public bool Success { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
    }
    
    public class BuildAudioFilesParams
    {
        public required Title Title { get; init; }
        public required Voice ToVoice { get; init; }
        public required Voice FromVoice { get; init; }
        public required Language ToLang { get; init; }
        public required Language FromLang { get; init; }
        public required int Pause { get; init; }
        public required string Pattern { get; init; }
        public required string TitleOutputPath { get; init; }
    }

    ExtractAndValidateResult ExtractAndValidatePhraseStrings(IFormFile file);
    Task<AudioFileResult> BuildAudioFilesAsync(BuildAudioFilesParams parameters, CancellationToken cancellationToken = default);
}