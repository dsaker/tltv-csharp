using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using TalkLikeTv.EntityModels;

namespace TalkLikeTv.Services;

public class AudioFileService
{
    private readonly ILogger<AudioFileService> _logger;
    private readonly string _baseDir;
    private readonly string _audioOutputDir;
    private readonly TalkliketvContext _db;

    public AudioFileService(ILogger<AudioFileService> logger, TalkliketvContext db, IConfiguration configuration)
    {
        _logger = logger;
        _db = db;
        _baseDir = configuration["BaseDir"] ?? throw new ArgumentNullException(configuration["BaseDir"], "BaseDir is not configured.");
        _audioOutputDir = configuration["AudioOutputDir"] ?? throw new ArgumentNullException(configuration["AudioOutputDir"], "AudioOutputDir is not configured.");

    }
    
    public class AudioFileResult
    {
        public bool Success { get; set; }
        public List<string> Errors { get; set; } = [];
    }
    
    private Dictionary<int, string> PauseFilePaths => new ()
    {
        { 3, $"{_baseDir}pause/3SecSilence.mp3" },
        { 4, $"{_baseDir}pause/4SecSilence.mp3" },
        { 5, $"{_baseDir}pause/5SecSilence.mp3" },
        { 6, $"{_baseDir}pause/6SecSilence.mp3" },
        { 7, $"{_baseDir}pause/7SecSilence.mp3" },
        { 8, $"{_baseDir}pause/8SecSilence.mp3" },
        { 9, $"{_baseDir}pause/9SecSilence.mp3" },
        { 10, $"{_baseDir}pause/10SecSilence.mp3" }
    };
    
    public async Task<AudioFileResult> BuildAudioFilesAsync(Title title, Language toLang, Language fromLang, Voice toVoice, Voice fromVoice, int pause, string p)
    {
        var result = new AudioFileResult();
        var maxP = title.NumPhrases - 1;

        var pattern = PatternService.GetPattern(p);
        if (pattern is null)
        {
            _logger.LogError("Pattern not found: {Pattern}", p);
            result.Errors.Add($"Pattern not found: {p}");
            return result;
        }

        // Retrieve all phrases for the title and map their PhraseId starting from zero
        var phrases = await _db.Phrases.Where(ph => ph.TitleId == title.TitleId).ToListAsync();
        var phraseIdMapping = phrases.Select((phrase, index) => new { phrase.PhraseId, Index = index })
            .ToDictionary(x => x.PhraseId, x => x.Index);

        var chunkedSlice = Chunk(pattern, 125);
        var last = false;

        if (!PauseFilePaths.TryGetValue(pause, out var pauseFilePath))
        {
            _logger.LogError("Invalid pause value: {Pause}", pause);
            result.Errors.Add($"Invalid pause value: {pause}");
            return result;
        }

        foreach (var chunk in chunkedSlice)
        {
            var inputFilePaths = new List<string>();
            try
            {
                inputFilePaths.Add(pauseFilePath);

                foreach (var audioFloat in chunk)
                {
                    var stringFloat = audioFloat.ToString("F1");
                    var phraseNative = stringFloat.Split('.');
                    var native = phraseNative.Length == 2;
                    var originalPhraseId = int.Parse(phraseNative[0]);

                    if (!phraseIdMapping.TryGetValue(originalPhraseId, out var mappedPhraseId))
                    {
                        _logger.LogError("PhraseId not found in mapping: {PhraseId}", originalPhraseId);
                        result.Errors.Add($"PhraseId not found in mapping: {originalPhraseId}");
                        return result;
                    }

                    if (mappedPhraseId == maxP)
                    {
                        last = true;
                    }
                    var toPath = Path.Combine(_baseDir, title.TitleName, toLang.Tag, toVoice.ShortName, mappedPhraseId.ToString());
                    var fromPath = Path.Combine(_baseDir, title.TitleName, fromLang.Tag, fromVoice.ShortName, mappedPhraseId.ToString());
                    var audioFilePath = native ? toPath : fromPath;
                    inputFilePaths.Add(audioFilePath);
                    inputFilePaths.Add(pauseFilePath);
                }

                inputFilePaths.Add(pauseFilePath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating audio input file");
                result.Errors.Add($"Error creating audio input file: {ex.Message}");
                return result;
            }

            // Call ConcatenateWavFiles for each chunk
            var outputFilePath = Path.Combine(_audioOutputDir, $"{title.TitleName}_chunk_{chunkedSlice.IndexOf(chunk)}.wav");
            WavConcatenator.ConcatenateWavFiles(inputFilePaths, outputFilePath);

            if (last)
            {
                break;
            }
        }

        result.Success = true;
        return result;
    }

    private List<List<float>> Chunk(List<float> source, int chunkSize)
    {
        return source
            .Select((x, i) => new { Index = i, Value = x })
            .GroupBy(x => x.Index / chunkSize)
            .Select(x => x.Select(v => v.Value).ToList())
            .ToList();
    }
}

