using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TalkLikeTv.EntityModels;
using TalkLikeTv.Utilities;

namespace TalkLikeTv.Services;

public class TranslationService
{
    private readonly TalkliketvContext _db;
    private readonly ILogger<TranslationService> _logger;
    private readonly string _baseDir;

    public TranslationService(TalkliketvContext db, ILogger<TranslationService> logger)
    {
        _db = db;
        _logger = logger;
        _baseDir = Environment.GetEnvironmentVariable("BASE_DIR") ?? throw new InvalidOperationException("BASE_DIR is not configured.");
    }
    
    public class ProcessTranslationsParams
    {
        public required Title Title { get; set; }
        public required Voice ToVoice { get; set; }
        public required Voice FromVoice { get; set; }
        public required Language ToLang { get; set; }
        public required Language FromLang { get; set; }
    }
    
    public async Task<(bool Success, List<string> Errors)> ProcessTranslations(ProcessTranslationsParams p)
    {
        var errors = new List<string>();
    
        try
        {
            var dbPhrases = await _db.Phrases
                .AsNoTracking()
                .Where(phrase => phrase.TitleId == p.Title.TitleId)
                .ToListAsync();

            if (dbPhrases.Count != p.Title.NumPhrases)
            {
                errors.Add("Phrases count must equal title.NumPhrases.");
                return (false, errors);
            }

            var originalLanguageTranslations = await _db.Translates
                .Where(t => t.LanguageId == p.Title.OriginalLanguageId && dbPhrases.Select(p => p.PhraseId).Contains(t.PhraseId))
                .ToListAsync();

            if (originalLanguageTranslations.Count != p.Title.NumPhrases)
            {
                errors.Add("Phrases count must equal title.NumPhrases.");
                return (false, errors);
            }

            if (p.Title.OriginalLanguage == null)
            {
                errors.Add("Title.OriginalLanguage is null.");
                return (false, errors);
            }

            var dbFromTranslates = await GetOrCreateTranslationsAsync(dbPhrases, p.Title.OriginalLanguage.Tag, p.FromLang, originalLanguageTranslations);

            var dbToTranslates = await GetOrCreateTranslationsAsync(dbPhrases, p.FromLang.Tag, p.ToLang, dbFromTranslates);

            // Generate TTS audio files and save them to the specified directory
            await GenerateSpeechFilesAsync(p.ToVoice, p.ToLang, p.Title, dbToTranslates);
            await GenerateSpeechFilesAsync(p.FromVoice, p.FromLang, p.Title, dbFromTranslates);

            return (true, errors);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing translations for title {TitleName}", p.Title.TitleName);
            errors.Add("An error occurred while processing translations.");
            return (false, errors);
        }
    }
    
    private async Task<List<Translate>> GetOrCreateTranslationsAsync(List<Phrase> dbPhrases, string fromLanguageTag, Language toLanguage, List<Translate> fromTranslates)
    {
        var existingTranslations = await _db.Translates
            .Where(t => t.LanguageId == toLanguage.LanguageId && dbPhrases.Select(p => p.PhraseId).Contains(t.PhraseId))
            .ToListAsync();

        if (existingTranslations.Count == dbPhrases.Count)
        {
            return existingTranslations;
        }
        
        var translateStrings = fromTranslates.Select(t => t.Phrase).ToList();
        
        var translatedPhrases = await new AzureTranslateService().TranslatePhrasesAsync(translateStrings, fromLanguageTag, toLanguage.Tag);

        var newTranslations = dbPhrases.Select((phrase, index) => new Translate
        {
            PhraseId = phrase.PhraseId,
            LanguageId = toLanguage.LanguageId,
            Phrase = translatedPhrases[index],
            PhraseHint = StringUtils.MakeHintString(translatedPhrases[index])
        }).ToList();

        _db.Translates.AddRange(newTranslations);
        await _db.SaveChangesAsync();

        return newTranslations;
    }
    
    private async Task GenerateSpeechFilesAsync(Voice voice, Language voiceLanguage, Title newTitle, List<Translate> dbTranslates)
    {
        var azureTtsService = new AzureTextToSpeechService();
        var wavDir = Path.Combine(_baseDir, newTitle.TitleName, voiceLanguage.Tag, voice.ShortName);

        if (Directory.Exists(wavDir))
        {
            return;
        }

        Directory.CreateDirectory(wavDir);

        var tasks = dbTranslates.Select(translate =>
            azureTtsService.GenerateSpeechToFileAsync(translate.Phrase, voice, Path.Combine(wavDir, $"{translate.PhraseId}"))
        ).ToList();

        await Task.WhenAll(tasks);
    }
}

