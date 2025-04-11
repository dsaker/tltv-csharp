using TalkLikeTv.EntityModels;

namespace TalkLikeTv.Services.Abstractions;

public interface ITranslationService
{
    Task<(bool Success, List<string> Errors)> ProcessTranslations(
        TranslationService.ProcessTranslationsParams parameters, 
        CancellationToken cancellationToken);

    Task<List<Translate>> GetOrCreateTranslationsAsync(
        List<Phrase> dbPhrases, 
        string fromLanguageTag, 
        Language toLanguage, 
        List<Translate> fromTranslates);

    Task GenerateSpeechFilesAsync(
        Voice voice, 
        Language voiceLanguage, 
        Title newTitle, 
        List<Translate> dbTranslates);
}