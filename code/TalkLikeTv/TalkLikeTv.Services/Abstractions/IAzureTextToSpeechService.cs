using TalkLikeTv.EntityModels;

namespace TalkLikeTv.Services.Abstractions;

public interface IAzureTextToSpeechService
{
    Task GenerateSpeechToFileAsync(string text, Voice voice, string filePath);
}