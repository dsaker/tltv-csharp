using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using TalkLikeTv.EntityModels;
using TalkLikeTv.Utilities;

namespace TalkLikeTv.Services;

public class TranslationService
{
    private readonly TalkliketvContext _db;
    private readonly ILogger<TranslationService> _logger;
    private readonly string _baseDir;

    public TranslationService(TalkliketvContext db, ILogger<TranslationService> logger, IConfiguration configuration)
    {
        _db = db;
        _logger = logger;
        _baseDir = Environment.GetEnvironmentVariable("BASE_DIR") ?? throw new InvalidOperationException("BASE_DIR is not configured.");
    }


    public async Task<(bool Success, List<string> Errors)> ProcessTranslations(
        Title newTitle, 
        List<string> phraseStrings, 
        Voice fromVoice, 
        Voice toVoice, 
        string detectedCode, 
        ModelStateDictionary modelState)
    {
        var dbPhrases = await _db.Phrases
            .AsNoTracking()
            .Where(p => p.TitleId == newTitle.TitleId)
            .ToListAsync();

        if (dbPhrases.Count != phraseStrings.Count)
        {
            modelState.AddModelError("", "Create title failed at dbPhrases.Count.");
            return (false, new List<string> { "Create title failed at dbPhrases.Count." });
        }

        var fromVoiceLanguage = await _db.Languages
            .AsNoTracking()
            .SingleOrDefaultAsync(l => l.LanguageId == fromVoice.LanguageId);

        if (fromVoiceLanguage == null)
        {
            modelState.AddModelError("", "fromVoiceLanguage is null.");
            return (false, new List<string> { "fromVoiceLanguage is null." });
        }

        List<Translate>? dbFromTranslates;

        if (fromVoiceLanguage.Tag != detectedCode)
        {
            var fromTranslates = await new AzureTranslateService().TranslatePhrasesAsync(phraseStrings, detectedCode, fromVoiceLanguage.Tag);

            dbFromTranslates = dbPhrases.Select((phrase, index) => new Translate
            {
                PhraseId = phrase.PhraseId,
                LanguageId = fromVoiceLanguage.LanguageId,
                Phrase = fromTranslates[index],
                PhraseHint = StringUtils.MakeHintString(fromTranslates[index])
            }).ToList();

            _db.Translates.AddRange(dbFromTranslates);
            await _db.SaveChangesAsync();
        }
        else
        {
            dbFromTranslates = dbPhrases.Select((phrase, index) => new Translate
            {
                PhraseId = phrase.PhraseId,
                LanguageId = fromVoiceLanguage.LanguageId,
                Phrase = phraseStrings[index],
                PhraseHint = StringUtils.MakeHintString(phraseStrings[index])
            }).ToList();

            _db.Translates.AddRange(dbFromTranslates);
            await _db.SaveChangesAsync();
        }

        var toVoiceLanguage = await _db.Languages
            .AsNoTracking()
            .SingleOrDefaultAsync(l => l.LanguageId == toVoice.LanguageId);

        if (toVoiceLanguage == null)
        {
            modelState.AddModelError("", "toVoiceLanguage is null.");
            return (false, new List<string> { "toVoiceLanguage is null." });
        }

        var toTranslates = await new AzureTranslateService().TranslatePhrasesAsync(phraseStrings, detectedCode, toVoiceLanguage.Tag);

        var dbToTranslates = dbPhrases.Select((phrase, index) => new Translate
        {
            PhraseId = phrase.PhraseId,
            LanguageId = toVoiceLanguage.LanguageId,
            Phrase = toTranslates[index],
            PhraseHint = StringUtils.MakeHintString(toTranslates[index])
        }).ToList();

        _db.Translates.AddRange(dbToTranslates);
        await _db.SaveChangesAsync();

        // Generate TTS audio files and save them to the specified directory
        await GenerateSpeechFilesAsync(toVoice, toVoiceLanguage, newTitle, dbToTranslates);
        await GenerateSpeechFilesAsync(fromVoice, fromVoiceLanguage, newTitle, dbFromTranslates);
        
        return (true, new List<string>());
    }
    
    private async Task GenerateSpeechFilesAsync(Voice voice, Language voiceLanguage, Title newTitle, List<Translate> dbTranslates)
    {
        var azureTtsService = new AzureTextToSpeechService();
        var wavDir = Path.Combine(_baseDir, newTitle.TitleName, voiceLanguage.Tag, voice.ShortName);

        try
        {
            if (!Directory.Exists(wavDir))
            {
                Directory.CreateDirectory(wavDir);
            }

            var tasks = new List<Task>();

            foreach (var translate in dbTranslates)
            {
                var audioFilePath = Path.Combine(wavDir, $"{translate.PhraseId}");
                tasks.Add(azureTtsService.GenerateSpeechToFileAsync(translate.Phrase, voice, audioFilePath));
            }

            await Task.WhenAll(tasks);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while generating speech files.");
            throw;
        }
    }
}

