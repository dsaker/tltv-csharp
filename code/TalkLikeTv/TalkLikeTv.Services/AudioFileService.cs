using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TalkLikeTv.EntityModels;

namespace TalkLikeTv.Services;

public class AudioFileService
{
    private readonly ILogger<AudioFileService> _logger;
    private readonly string _baseDir;
    private readonly TalkliketvContext _db;

    public AudioFileService(ILogger<AudioFileService> logger, TalkliketvContext db)
    {
        _logger = logger;
        _db = db;
        _baseDir = Environment.GetEnvironmentVariable("BASE_DIR") ?? throw new InvalidOperationException("BASE_DIR is not configured.");
    }
    
    public class AudioFileResult
    {
        public bool Success { get; set; }
        public List<string> Errors { get; set; } = [];
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
    
    public class BuildAudioFilesParams
    {
        public required Title Title { get; set; }
        public required Voice ToVoice { get; set; }
        public required Voice FromVoice { get; set; }
        public required Language ToLang { get; set; }
        public required Language FromLang { get; set; }
        public required int Pause { get; set; }
        public required string Pattern { get; set; }
        public required string TitleOutputPath { get; set; }
    }
    
    public async Task<AudioFileResult> BuildAudioFilesAsync(BuildAudioFilesParams parameters)
    {
        var result = new AudioFileResult();
        try
        {
            var pattern = PatternService.GetPattern(parameters.Pattern);
            if (pattern is null)
            {
                _logger.LogError("Pattern not found: {Pattern}", parameters.Pattern);
                result.Errors.Add($"Pattern not found: {parameters.Pattern}");
                return result;
            }

            var phrases = await _db.Phrases.Where(ph => ph.TitleId == parameters.Title.TitleId).ToListAsync();
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

