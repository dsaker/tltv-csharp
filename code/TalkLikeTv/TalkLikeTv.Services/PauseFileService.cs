using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace TalkLikeTv.Services;

public class PauseFileService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<PauseFileService> _logger;

    public PauseFileService(IConfiguration configuration, ILogger<PauseFileService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public void EnsurePauseFilesExist()
    {
        var baseDir = _configuration.GetValue<string>("TalkLikeTv:BaseDir");
        if (string.IsNullOrEmpty(baseDir))
        {
            throw new InvalidOperationException("TalkLikeTv:BaseDir is not configured.");
        }
        
        var audioOutputDir = _configuration.GetValue<string>("TalkLikeTv:AudioOutputDir");
        if (string.IsNullOrEmpty(audioOutputDir))
        {
            throw new InvalidOperationException("TalkLikeTv:AudioOutputDir is not configured.");
        }
        
        // Ensure the base directory is writable
        _ensureDirectoryIsWritable(baseDir);
        // Ensure the audio output directory is writable
        _ensureDirectoryIsWritable(audioOutputDir);

        var pauseDir = Path.Combine(baseDir, "pause");

        // Create the pause directory if it doesn't exist
        if (!Directory.Exists(pauseDir))
        {
                throw new FileNotFoundException(
                $"Required pause directory missing: {pauseDir}. Please run: cp -R TalkLikeTv.Services/Resources/pause/ {baseDir}pause/", pauseDir);
        }

        // Check for each pause file in the AudioFileService's PauseFilePaths dictionary
        var pauseFiles = new[]
        {
            "3SecondsOfSilence.wav",
            "4SecondsOfSilence.wav",
            "5SecondsOfSilence.wav",
            "6SecondsOfSilence.wav",
            "7SecondsOfSilence.wav",
            "8SecondsOfSilence.wav",
            "9SecondsOfSilence.wav",
            "10SecondsOfSilence.wav"
        };

        foreach (var file in pauseFiles)
        {
            var filePath = Path.Combine(pauseDir, file);
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException(
                    $"Required pause directory missing: {pauseDir}. Please run: cp -R TalkLikeTv.Services/Resources/pause/ {baseDir}pause/", pauseDir);

            }
        }

        _logger.LogInformation("All required pause files verified in: {PauseDir}", pauseDir);
    }
    
    private void _ensureDirectoryIsWritable(string directory)
    {
        try
        {
            // Create a test file
            var testPath = Path.Combine(directory, ".write-test");
            File.WriteAllText(testPath, "Test");
            File.Delete(testPath);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Directory {directory} is not writable: {ex.Message}");
        }
    }
}