using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TalkLikeTv.EntityModels;
using TalkLikeTv.Utilities;

namespace TalkLikeTv.Services;

// TalkLikeTv.Services/AudioProcessingService.cs
public class AudioProcessingService
{
    private readonly TalkliketvContext _db;
    private readonly ILogger<AudioProcessingService> _logger;
    private readonly TranslationService _translationService;
    private readonly AudioFileService _audioFileService;
    private readonly TokenService _tokenService;
    private readonly string _audioOutputDir;

    public AudioProcessingService(
        TalkliketvContext db,
        ILogger<AudioProcessingService> logger,
        TranslationService translationService,
        TokenService tokenService,
        AudioFileService audioFileService)
    {
        _db = db;
        _logger = logger;
        _translationService = translationService;
        _audioFileService = audioFileService;
        _tokenService = tokenService;
        _audioOutputDir = Environment.GetEnvironmentVariable("AUDIO_OUTPUT_DIR") ?? throw new InvalidOperationException("AUDIO_OUTPUT_DIR is not configured.");
    }

    private async Task<(Voice?, Voice?)> GetVoicesAsync(int toVoiceId, int fromVoiceId)
    {
        try
        {
            var toVoice = await _db.Voices.SingleOrDefaultAsync(v => v.VoiceId == toVoiceId);
            var fromVoice = await _db.Voices.SingleOrDefaultAsync(v => v.VoiceId == fromVoiceId);
            return (toVoice, fromVoice);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching voices with IDs {ToVoiceId} and {FromVoiceId}", toVoiceId, fromVoiceId);
            return (null, null);
        }
    }

    public async Task<Language?> DetectLanguageAsync(List<string> phraseStrings, ModelStateDictionary modelState)
    {
        try
        {
            var translator = new AzureTranslateService();
            var detectedCode = await translator.DetectLanguageFromPhrasesAsync(phraseStrings);

            var detectedLanguage = await _db.Languages.SingleOrDefaultAsync(l => l.Tag == detectedCode);
            if (detectedLanguage == null)
            {
                modelState.AddModelError("", $"Language '{detectedCode}' not found.");
            }
            return detectedLanguage;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error detecting language from phrases");
            modelState.AddModelError("", "An error occurred while detecting the language.");
            return null;
        }
    }
    
    private async Task<(Language?, Language?)> GetLanguagesAsync(Voice toVoice, Voice fromVoice)
    {
        try
        {
            var toLang = await _db.Languages.SingleOrDefaultAsync(l => l.LanguageId == toVoice.LanguageId);
            var fromLang = await _db.Languages.SingleOrDefaultAsync(l => l.LanguageId == fromVoice.LanguageId);
            return (toLang, fromLang);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching languages for voices {ToVoice} and {FromVoice}", 
                toVoice.ShortName, fromVoice.ShortName);
            return (null, null);
        }
    }

    private async Task<(bool Success, List<string> Errors)> ProcessTranslationsAsync(
        Title title, 
        Voice toVoice, 
        Voice fromVoice, 
        Language toLang, 
        Language fromLang)
    {
        try
        {
            var parameters = new TranslationService.ProcessTranslationsParams
            {
                Title = title,
                ToVoice = toVoice,
                FromVoice = fromVoice,
                ToLang = toLang,
                FromLang = fromLang
            };

            return await _translationService.ProcessTranslations(parameters);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing translations for title {TitleName}", title.TitleName);
            return (false, new List<string> { "An error occurred while processing translations." });
        }
    }

    private async Task<AudioFileService.AudioFileResult> BuildAudioFilesAsync(
        Title title, Voice toVoice, Voice fromVoice, Language toLang, 
        Language fromLang, int pauseDuration, string pattern)
    {
        var parameters = new AudioFileService.BuildAudioFilesParams
        {
            Title = title,
            ToVoice = toVoice,
            FromVoice = fromVoice,
            ToLang = toLang,
            FromLang = fromLang,
            Pause = pauseDuration,
            Pattern = pattern,
            TitleOutputPath = Path.Combine(_audioOutputDir, title.TitleName, fromVoice.ShortName, toVoice.ShortName)
        };

        return await _audioFileService.BuildAudioFilesAsync(parameters);
    }

    private FileInfo CreateZipFile(string titleName, string fromLangTag, string toLangTag, 
        string fromVoiceShortName, string toVoiceShortName)
    {
        var zipFileName = $"{titleName}_{fromLangTag}_{toLangTag}.zip";
        var outputPath = Path.Combine(_audioOutputDir, titleName, fromVoiceShortName, toVoiceShortName);
        return ZipDirService.CreateZipFile(outputPath, zipFileName);
    }

    public async Task<(bool Success, List<string> Errors)> MarkTokenAsUsedAsync(string? tokenHash)
    {
        var errors = new List<string>();
    
        try
        {
            if (string.IsNullOrEmpty(tokenHash))
            {
                errors.Add("Token hash is empty or null.");
                return (false, errors);
            }
        
            var token = await _db.Tokens.SingleOrDefaultAsync(t => t.Hash == tokenHash);
            if (token == null)
            {
                errors.Add("Invalid token.");
                return (false, errors);
            }

            token.Used = true;
            await _db.SaveChangesAsync();
            return (true, errors);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking token {TokenHash} as used", tokenHash);
            errors.Add("An error occurred while processing the token.");
            return (false, errors);
        }
    }

    public async Task<Title> ProcessTitleAsync(string titleName, string? description, List<string> phraseStrings, Language detectedLanguage)
    {
        try
        {
            var languageId = detectedLanguage.LanguageId;
            var newTitle = new Title
            {
                TitleName = titleName,
                Description = description,
                NumPhrases = phraseStrings.Count,
                OriginalLanguageId = languageId,
            };

            _db.Titles.Add(newTitle);
            await _db.SaveChangesAsync();

            var phrases = phraseStrings.Select(_ => new Phrase
            {
                TitleId = newTitle.TitleId,
            }).ToList();

            _db.Phrases.AddRange(phrases);
            await _db.SaveChangesAsync();

            var phraseTranslates = phrases.Select((phrase, index) => new Translate
            {
                PhraseId = phrase.PhraseId,
                LanguageId = detectedLanguage.LanguageId,
                Phrase = phraseStrings[index],
                PhraseHint = StringUtils.MakeHintString(phraseStrings[index])
            }).ToList();

            _db.Translates.AddRange(phraseTranslates);
            await _db.SaveChangesAsync();

            return newTitle;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing title {TitleName}", titleName);
            throw new InvalidOperationException("An error occurred while processing the title.", ex);
        }
    }
    
    public async Task<(FileInfo? ZipFile, List<string> Errors)> ProcessAudioRequestAsync(
        int toVoiceId,
        int fromVoiceId,
        Title title,
        int pauseDuration,
        string pattern)
    {
        var errors = new List<string>();
    
        try
        {
            // Get voices and validate them
            var (toVoice, fromVoice) = await GetVoicesAsync(toVoiceId, fromVoiceId);
            if (toVoice == null || fromVoice == null)
            {
                errors.Add("Invalid voice selection.");
                return (null, errors);
            }

            // Get languages
            var (toLang, fromLang) = await GetLanguagesAsync(toVoice, fromVoice);
            if (toLang == null || fromLang == null)
            {
                errors.Add("Invalid language selection.");
                return (null, errors);
            }

            // Process translations
            var (translationSuccess, translationErrors) = await ProcessTranslationsAsync(
                title, toVoice, fromVoice, toLang, fromLang);

            if (!translationSuccess)
            {
                errors.AddRange(translationErrors);
                return (null, errors);
            }
            
            // Build audio files
            var audioFileResult = await BuildAudioFilesAsync(
                title, toVoice, fromVoice, toLang, fromLang,
                pauseDuration, pattern);

            if (!audioFileResult.Success)
            {
                errors.AddRange(audioFileResult.Errors);
                return (null, errors);
            }

            // Create zip file
            var zipFilePath = CreateZipFile(
                title.TitleName, fromLang.Tag, toLang.Tag, fromVoice.ShortName, toVoice.ShortName);

            return (zipFilePath, errors);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing audio request for title {TitleName}", title.TitleName);
            errors.Add("An error occurred while processing the audio request.");
            return (null, errors);
        }
    }
 
    public async Task<bool> ValidateAndMarkTokenAsync(string tokenHash, ModelStateDictionary modelState)
    {
        // Check if the token is valid
        if (string.IsNullOrEmpty(tokenHash) || !_tokenService.CheckTokenStatus(tokenHash))
        {
            modelState.AddModelError("", "Invalid token.");
            return false;
        }

        var (success, errors) = await MarkTokenAsUsedAsync(tokenHash);
        if (success)
        {
            return true;
        }
        
        foreach (var error in errors)
        {
            modelState.AddModelError("", error);
        }
        return false;
    }
}