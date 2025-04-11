using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using TalkLikeTv.EntityModels;
using TalkLikeTv.Repositories;
using TalkLikeTv.Services.Abstractions;
using TalkLikeTv.Utilities;

namespace TalkLikeTv.Services;

public class TranslationService : ITranslationService
{
    private readonly ILogger<TranslationService> _logger;
    private readonly string _baseDir;
    private readonly ITranslateRepository _translateRepository;
    private readonly IPhraseRepository _phraseRepository;
    private readonly ILanguageRepository _languageRepository;

    public TranslationService(
        ILogger<TranslationService> logger, 
        ITranslateRepository translateRepository,
        IPhraseRepository phraseRepository,
        ILanguageRepository languageRepository,
        IConfiguration configuration)
    {
        _logger = logger;
        _translateRepository = translateRepository;
        _phraseRepository = phraseRepository;
        _languageRepository = languageRepository;
        _baseDir = configuration.GetValue<string>("SharedSettings:BaseDir") ?? throw new InvalidOperationException("BaseDir is not configured.");
    }
    
    public class ProcessTranslationsParams
    {
        public required Title Title { get; set; }
        public required Voice ToVoice { get; set; }
        public required Voice FromVoice { get; set; }
        public required Language ToLang { get; set; }
        public required Language FromLang { get; set; }
    }
    
    public async Task<(bool Success, List<string> Errors)> ProcessTranslations(ProcessTranslationsParams p, CancellationToken cancellationToken)
    {
        var errors = new List<string>();

        try
        {
            var dbPhrases = await _phraseRepository.GetPhrasesByTitleIdAsync(p.Title.TitleId);

            if (dbPhrases.Count != p.Title.NumPhrases)
            {
                errors.Add("Phrases count must equal title.NumPhrases.");
                return (false, errors);
            }

            if (p.Title.OriginalLanguageId == null)
            {
                errors.Add("Original language ID is null.");
                return (false, errors);
            }

            // Convert nullable int to non-nullable for the repository call
            int originalLanguageId = p.Title.OriginalLanguageId.Value;
            
            var phraseIds = dbPhrases.Select(ph => ph.PhraseId).ToList();
            var originalLanguageTranslations = await _translateRepository.GetTranslatesByLanguageAndPhrasesAsync(
                originalLanguageId, 
                phraseIds);

            if (originalLanguageTranslations.Count != p.Title.NumPhrases)
            {
                errors.Add("Phrases count must equal title.NumPhrases.");
                return (false, errors);
            }
            
            var originalLanguage = await _languageRepository.RetrieveAsync(originalLanguageId.ToString(), cancellationToken);

            if (originalLanguage == null)
            {
                errors.Add("Original language not found.");
                return (false, errors);
            }

            var dbFromTranslates = await GetOrCreateTranslationsAsync(dbPhrases, originalLanguage.Tag, p.FromLang, originalLanguageTranslations);
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
    
    public async Task<List<Translate>> GetOrCreateTranslationsAsync(List<Phrase> dbPhrases, string fromLanguageTag, Language toLanguage, List<Translate> fromTranslates)
    {
        var phraseIds = dbPhrases.Select(p => p.PhraseId).ToList();
        var existingTranslations = await _translateRepository.GetTranslatesByLanguageAndPhrasesAsync(
            toLanguage.LanguageId, 
            phraseIds);

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

        // Use the repository to create translations
        await _translateRepository.CreateManyAsync(newTranslations);

        return newTranslations;
    }
    
    public async Task GenerateSpeechFilesAsync(Voice voice, Language voiceLanguage, Title newTitle, List<Translate> dbTranslates)
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

