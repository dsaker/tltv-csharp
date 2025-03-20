using System.Text.RegularExpressions;

namespace TalkLikeTv.Services;

public partial class TextFormatDetector
{
    public enum TextFormat
    {
        Paragraph,
        OnePhrasePerLine,
        Srt
    }
    
    // SRT format: 00:00:00,000 --> 00:00:00,000
    [GeneratedRegex(@"\d{2}:\d{2}:\d{2},\d{3} --> \d{2}:\d{2}:\d{2},\d{3}")]
    private static partial Regex SrtFormatCheck();  
    private static bool _srtFormatCheck(string input) => SrtFormatCheck().IsMatch(input);

    public static TextFormat DetectTextFormat(Stream fileStream)
    {
        ArgumentNullException.ThrowIfNull(fileStream, nameof(fileStream));
        var reader = new StreamReader(fileStream);
        string? line;
        // check if the file is in SRT format
        for (var i = 0; i < 15 && (line = reader.ReadLine()) != null; i++)
        {
            if (_srtFormatCheck(line))
            {
                return TextFormat.Srt;
            }
        }
        
        // Seek back to the beginning of the file
        fileStream.Seek(0, SeekOrigin.Begin);
        reader = new StreamReader(fileStream);
        
        var lines = reader.ReadToEnd().Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        var lineCount = lines.Length;
        var averageLineLength = lines.Sum(l => l.Length) / (double)lineCount;

        // Heuristics: if the average line length is significantly shorter than a typical paragraph length
        // and the number of lines is greater than a typical paragraph count (e.g., more than 3 lines),
        // we consider it as one phrase per line.
        if (lineCount > 3 && averageLineLength < 80)
        {
            return TextFormat.OnePhrasePerLine;
        }
        fileStream.Seek(0, SeekOrigin.Begin);
        return TextFormat.Paragraph;
    }
    
}