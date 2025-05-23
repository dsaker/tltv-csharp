using TalkLikeTv.EntityModels;

namespace TalkLikeTv.Services.Abstractions;

public interface IAudioProcessingService
{
    Task<(Language? Language, List<string> Errors)> DetectLanguageAsync(List<string> phraseStrings, CancellationToken token = default);
    Task<Title> ProcessTitleAsync(string titleName, string? description, List<string> phraseStrings, Language detectedLanguage, CancellationToken token = default);
    Task<(FileInfo? ZipFile, List<string> Errors)> ProcessAudioRequestAsync(int toVoiceId, int fromVoiceId, Title title, int pauseDuration, string pattern, CancellationToken token = default);
}