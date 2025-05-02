namespace TalkLikeTv.Services.Abstractions;

public interface IAzureTranslateService
{
    Task<string> DetectLanguageFromPhrasesAsync(List<string> phrases, CancellationToken token = default);
    Task<List<string>> TranslatePhrasesAsync(List<string> phrases, string fromLanguage, string toLanguage, CancellationToken token = default);
}