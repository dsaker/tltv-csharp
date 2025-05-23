using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using TalkLikeTv.EntityModels;
using Microsoft.Extensions.Configuration;
using TalkLikeTv.Repositories;
using TalkLikeTv.Services.Abstractions;

namespace TalkLikeTv.Services;

public class AudioFileService : IAudioFileService
{
    private readonly ILogger<AudioFileService> _logger;
    private readonly string _baseDir;
    private readonly IPhraseService _phraseService;
    private readonly IPhraseRepository _phraseRepository;
    private readonly int _maxPhrases;

    public AudioFileService(
        ILogger<AudioFileService> logger, 
        IPhraseService phraseService,
        IPhraseRepository phraseRepository,
        IConfiguration configuration)
    {
        _logger = logger;
        _phraseService = phraseService;
        _phraseRepository = phraseRepository;
        _maxPhrases = configuration.GetValue<int>("TalkLikeTv:MaxPhrases");
        _baseDir = configuration.GetValue<string>("TalkLikeTv:BaseDir") ?? throw new InvalidOperationException("BaseDir is not configured.");
    }
    
    private Dictionary<int, string> PauseFilePaths => new ()
    {
        { 3, $"{_baseDir}pause/3SecondsOfSilence.wav" },
        { 4, $"{_baseDir}pause/4SecondsOfSilence.wav" },
        { 5, $"{_baseDir}pause/5SecondsOfSilence.wav" },
        { 6, $"{_baseDir}pause/6SecondsOfSilence.wav" },
        { 7, $"{_baseDir}pause/7SecondsOfSilence.wav" },
        { 8, $"{_baseDir}pause/8SecondsOfSilence.wav" },
        { 9, $"{_baseDir}pause/9SecondsOfSilence.wav" },
        { 10, $"{_baseDir}pause/10SecondsOfSilence.wav" }
    };
    
    public IAudioFileService.ExtractAndValidateResult ExtractAndValidatePhraseStrings(IFormFile file)
    {
        var result = new IAudioFileService.ExtractAndValidateResult();

        try
        {
            var phraseResult = _phraseService.GetPhraseStrings(file);
            if (!phraseResult.Success)
            {
                result.Errors.Add(phraseResult.ErrorMessage ?? "Failed to extract phrases from the file.");
                return result;
            }

            if (phraseResult.Phrases == null || phraseResult.Phrases.Count > _maxPhrases)
            {
                result.Errors.Add($"Phrase count exceeds the maximum of {_maxPhrases}.");
                return result;
            }

            result.PhraseStrings = phraseResult.Phrases;
        }
        catch (Exception ex)
        {
            result.Errors.Add(ex.Message);
        }

        return result;
    }
    
    public async Task<IAudioFileService.AudioFileResult> BuildAudioFilesAsync(IAudioFileService.BuildAudioFilesParams parameters, CancellationToken cancellationToken = default)
    {
        var result = new IAudioFileService.AudioFileResult();
        try
        {
            var pattern = PatternService.GetPattern(parameters.Pattern);
            if (pattern is null)
            {
                _logger.LogError("Pattern not found: {Pattern}", parameters.Pattern);
                result.Errors.Add($"Pattern not found: {parameters.Pattern}");
                return result;
            }
            var phrases = await _phraseRepository.GetPhrasesByTitleIdAsync(parameters.Title.TitleId, cancellationToken);
            //var phrases = await _db.Phrases.Where(ph => ph.TitleId == parameters.Title.TitleId).ToListAsync();
            var phraseIdMapping = new Dictionary<int, int>();

            for (var i = 1; i < phrases.Count + 1; i++)
            {
                phraseIdMapping.Add(i, phrases[i - 1].PhraseId);
            }

            var chunkedSlice = Utilities.ChunkUtils.Chunk(pattern, 125);

            if (!PauseFilePaths.TryGetValue(parameters.Pause, out var pauseFilePath))
            {
                _logger.LogError("Invalid pause value: {Pause}", parameters.Pause);
                result.Errors.Add($"Invalid pause value: {parameters.Pause}");
                return result;
            }
            
            var last = false;
            var count = 1;
            foreach (var chunk in chunkedSlice)
            {
                var inputFilePaths = new List<string> { pauseFilePath };

                foreach (var audioToken in chunk)
                {
                    var (phraseIdKey, native) = SplitShortString(audioToken.ToString());
                    var phraseIdKeyInt = int.Parse(phraseIdKey);
                    if (phraseIdMapping.TryGetValue(phraseIdKeyInt, out var mappedPhraseId))
                    {
                        var toPath = Path.Combine(_baseDir, parameters.Title.TitleName, parameters.ToLang.Tag, parameters.ToVoice.ShortName, mappedPhraseId.ToString());
                        var fromPath = Path.Combine(_baseDir, parameters.Title.TitleName, parameters.FromLang.Tag, parameters.FromVoice.ShortName, mappedPhraseId.ToString());
                        var audioFilePath = int.Parse(native) == 0 ? toPath : fromPath;
                        inputFilePaths.Add(audioFilePath);
                        inputFilePaths.Add(pauseFilePath);

                        if (phraseIdKeyInt == parameters.Title.NumPhrases)
                        {
                            last = true;
                        }
                    }
                }

                if (!Directory.Exists(parameters.TitleOutputPath))
                {
                    Directory.CreateDirectory(parameters.TitleOutputPath);
                }
                
                var outputFilePath = Path.Combine(parameters.TitleOutputPath, $"{parameters.Title.TitleName}_{count:D2}.wav");
                count++;
                WavConcatenator.ConcatenateWavFiles(inputFilePaths, outputFilePath);
                if (last)
                {
                    break;
                }
            }

            result.Success = true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected exception occurred.");
            result.Errors.Add(ex.Message);
        }

        return result;
    }   
    
    private static (string, string) SplitShortString(string? input)
    {
        if (string.IsNullOrEmpty(input) || input.Length < 2)
        {
            throw new ArgumentException("Input string must have at least two characters.", nameof(input));
        }

        var lastDigit = input[^1].ToString();
        var remainingDigits = input.Substring(0, input.Length - 1);

        return (remainingDigits, lastDigit);
    }
}

