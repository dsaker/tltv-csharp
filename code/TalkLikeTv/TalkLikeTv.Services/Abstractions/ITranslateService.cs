namespace TalkLikeTv.Services.Abstractions;

public interface ITranslateService
{
    Task<string> DetectLanguageFromPhrasesAsync(List<string> phrases);
    Task<List<string>> TranslatePhrasesAsync(List<string> phrases, string fromLanguage, string toLanguage);
}