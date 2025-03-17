using System.Text.Json;

namespace TalkLikeTv.Services;

public static class PatternService
{
    private static Dictionary<string, List<float>> _patterns = new();

    public static void LoadPatterns()
    {
        var patternsFilePath = Path.Combine(AppContext.BaseDirectory, "json", "patterns.json");
        _patterns = LoadPatternsFromFile(patternsFilePath);
    }

    private static Dictionary<string, List<float>> LoadPatternsFromFile(string filePath)
    {
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"Patterns file not found: {filePath}");
        }

        var json = File.ReadAllText(filePath);
        return JsonSerializer.Deserialize<Dictionary<string, List<float>>>(json) 
               ?? new Dictionary<string, List<float>>();
    }

    public static List<float>? GetPattern(string patternName)
    {
        return _patterns.TryGetValue(patternName, out var pattern) ? pattern : null;
    }
}
