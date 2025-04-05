using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using TalkLikeTv.EntityModels;
using TalkLikeTv.Repositories;
using TalkLikeTv.Utilities;

namespace TalkLikeTv.Services;

public class AudioProcessingService
{
    private readonly ILogger<AudioProcessingService> _logger;
    private readonly TranslationService _translationService;
    private readonly AudioFileService _audioFileService;
    private readonly string _audioOutputDir;
    private readonly IVoiceRepository _voiceRepository;
    private readonly ILanguageRepository _languageRepository;
    private readonly ITokenRepository _tokenRepository;
    private readonly ITitleRepository _titleRepository;
    private readonly IPhraseRepository _phraseRepository;
    private readonly ITranslateRepository _translateRepository;

    public AudioProcessingService(
        ILogger<AudioProcessingService> logger,
        TranslationService translationService,
        AudioFileService audioFileService,
        IVoiceRepository voiceRepository,
        ILanguageRepository languageRepository,
        ITokenRepository tokenRepository,
        ITitleRepository titleRepository,
        IPhraseRepository phraseRepository,
        ITranslateRepository translateRepository,
        IConfiguration configuration)
    {
        _logger = logger;
        _translationService = translationService;
        _audioFileService = audioFileService;
        _voiceRepository = voiceRepository;
        _languageRepository = languageRepository;
        _tokenRepository = tokenRepository;
        _titleRepository = titleRepository;
        _phraseRepository = phraseRepository;
        _translateRepository = translateRepository;
        _audioOutputDir = configuration.GetValue<string>("SharedSettings:AudioOutputDir") ?? throw new InvalidOperationException("AudioOutputdir is not configured.");
    }

    private async Task<(Voice?, Voice?)> GetVoicesAsync(int toVoiceId, int fromVoiceId, CancellationToken token = default)
    {
        try
        {
            var toVoice = await _voiceRepository.RetrieveAsync(toVoiceId.ToString(), token);
            var fromVoice = await _voiceRepository.RetrieveAsync(fromVoiceId.ToString(), token);
            return (toVoice, fromVoice);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching voices with IDs {ToVoiceId} and {FromVoiceId}", toVoiceId, fromVoiceId);
            return (null, null);
        }
    }

    public async Task<(Language? Language, List<string> Errors)> DetectLanguageAsync(
        List<string> phraseStrings, 
        CancellationToken token = default)
    {
        var errors = new List<string>();
    
        try
        {
            var translator = new AzureTranslateService();
            var detectedCode = await translator.DetectLanguageFromPhrasesAsync(phraseStrings);

            var detectedLanguage = await _languageRepository.RetrieveByTagAsync(detectedCode, token);
            if (detectedLanguage == null)
            {
                errors.Add($"Language '{detectedCode}' not found.");
            }
            return (detectedLanguage, errors);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error detecting language from phrases");
            errors.Add("An error occurred while detecting the language.");
            return (null, errors);
        }
    }
    
    private async Task<(Language? ToLanguage, Language? FromLanguage, List<string> Errors)> GetLanguagesAsync(
        Voice toVoice,
        Voice fromVoice,
        CancellationToken token = default)
    {
        var errors = new List<string>();
    
        try
        {
            var toLangIdString = toVoice.LanguageId.ToString();
            var fromLangIdString = fromVoice.LanguageId.ToString();
        
            if (toLangIdString == null || fromLangIdString == null)
            {
                errors.Add($"Language ID is null for voices {toVoice.ShortName} and {fromVoice.ShortName}");
                return (null, null, errors);
            }
        
            var toLang = await _languageRepository.RetrieveAsync(toLangIdString, token);
            var fromLang = await _languageRepository.RetrieveAsync(fromLangIdString, token);
        
            if (toLang == null)
                errors.Add($"Language for voice {toVoice.ShortName} not found.");
            
            if (fromLang == null)
                errors.Add($"Language for voice {fromVoice.ShortName} not found.");
        
            return (toLang, fromLang, errors);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching languages for voices {ToVoice} and {FromVoice}",
                toVoice.ShortName, fromVoice.ShortName);
            errors.Add("An error occurred while retrieving languages.");
            return (null, null, errors);
        }
    }

    private async Task<(bool Success, List<string> Errors)> ProcessTranslationsAsync(
        Title title, 
        Voice toVoice, 
        Voice fromVoice, 
        Language toLang, 
        Language fromLang,
        CancellationToken token = default)
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

            return await _translationService.ProcessTranslations(parameters, token);
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
        
            var token = await _tokenRepository.RetrieveByHashAsync(tokenHash);
            if (token == null)
            {
                errors.Add("Invalid token.");
                return (false, errors);
            }

            token.Used = true;
            await _tokenRepository.UpdateAsync(token.TokenId.ToString(), token);
            return (true, errors);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking token {TokenHash} as used", tokenHash);
            errors.Add("An error occurred while processing the token.");
            return (false, errors);
        }
    }

    public async Task<Title> ProcessTitleAsync(
        string titleName, 
        string? description, 
        List<string> phraseStrings, 
        Language detectedLanguage,
        CancellationToken token = default)
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

            var dbTitle = await _titleRepository.CreateAsync(newTitle, token);

            var phrases = phraseStrings.Select(_ => new Phrase
            {
                TitleId = newTitle.TitleId,
            }).ToList();

            phrases = await _phraseRepository.CreateManyAsync(phrases, token);

            var phraseTranslates = phrases.Select((phrase, index) => new Translate
            {
                PhraseId = phrase.PhraseId,
                LanguageId = detectedLanguage.LanguageId,
                Phrase = phraseStrings[index],
                PhraseHint = StringUtils.MakeHintString(phraseStrings[index])
            }).ToList();

            await _translateRepository.CreateManyAsync(phraseTranslates, token);

            return dbTitle;
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
        string pattern,
        CancellationToken token = default)
    {
        var errors = new List<string>();
    
        try
        {
            // Get voices and validate them
            var (toVoice, fromVoice) = await GetVoicesAsync(toVoiceId, fromVoiceId, token);
            if (toVoice == null || fromVoice == null)
            {
                errors.Add("Invalid voice selection.");
                return (null, errors);
            }

            // Get languages
            var (toLang, fromLang, languageErrors) = await GetLanguagesAsync(toVoice, fromVoice, token);
            if (languageErrors.Any())
            {
                errors.AddRange(languageErrors);
                return (null, errors);
            }

            // Process translations
            var (translationSuccess, translationErrors) = await ProcessTranslationsAsync(
                title, toVoice, fromVoice, toLang!, fromLang!, token);

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
}