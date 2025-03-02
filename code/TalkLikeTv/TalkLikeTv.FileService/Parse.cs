using System.Text.RegularExpressions;

namespace TalkLikeTv.FileService;

public partial class Parse
{
    [GeneratedRegex(@"\[.*?\]|\{.*?\}|<.*?>|♪|-|\""")]
    private static partial Regex ReplaceFmt();
    private static string _replaceFmt(string input) => ReplaceFmt().Replace(input, "");

    [GeneratedRegex("(?<=[!.?,;])")]
    private static partial Regex SplitOnPunctuationBreak();
    private static string[] _splitOnPunctuationBreak(string input) => SplitOnPunctuationBreak().Split(input);

    [GeneratedRegex("[!.?]")]
    private static partial Regex SplitOnEndingPunctuation();
    private static string[] _splitOnEndingPunctuation(string input) => SplitOnEndingPunctuation().Split(input);

    public static FileInfo ParseFile(FileStream? fileStream)
    {
        ArgumentNullException.ThrowIfNull(fileStream, nameof(fileStream));

        Console.WriteLine($"File size: {fileStream.Length} bytes");

        if (fileStream.Length > 8192*8)
        {
            throw new Exception($"File too large ({fileStream.Length} > {8192*8} bytes)");
        }

        var stringsList = _getLines(fileStream);

        var filename = Path.GetFileNameWithoutExtension(fileStream.Name);
        var txtPath = Path.Combine("/tmp/ParseFile/", filename);
        var file = ZipFile.ZipStringsList(stringsList, 100, txtPath, filename);

        return file;
    }

    private static List<string> _getLines(FileStream fileStream)
    {
        using var reader = new StreamReader(fileStream);
        var textFormat = TextFormatDetector.DetectTextFormat(fileStream);

        fileStream.Seek(0, SeekOrigin.Begin);
        return textFormat switch
        {
            TextFormatDetector.TextFormat.Srt => _parseSrt(reader),
            TextFormatDetector.TextFormat.Paragraph => _parseParagraph(reader),
            TextFormatDetector.TextFormat.OnePhrasePerLine => _parseOnePhrasePerLine(reader),
            _ => _parseOnePhrasePerLine(reader)
        };
    }

    private static List<string> _parseSrt(StreamReader reader)
    {
        ArgumentNullException.ThrowIfNull(reader, nameof(reader));
        var stringsSlice = new List<string>();
        var line = reader.ReadLine();
        while (line != null)
        {
            line = line.Trim();
            if (
                string.IsNullOrEmpty(line) ||
                char.IsDigit(line[0]) ||
                (line[0] == '[' && line.Last() == ']') ||
                line.Contains("<font") ||
                line.Contains("font>")
                )
            {
                line = reader.ReadLine();
                continue;
            }
            // if the line is not empty then it is another line of the same phrase
            var nextLine = reader.ReadLine();
            if (!string.IsNullOrEmpty(nextLine))
            {
                line = line.Replace("\n", "") + " " + nextLine;
            }

            line = _replaceFmt(line);

            var phrases = _splitLongPhrases(line);
            // TODO return list of skipped phrases to user
            if (phrases != null)
            {
                stringsSlice.AddRange(phrases);
            }
            line = reader.ReadLine();
        }

        return stringsSlice;
    }

    private static List<string> _parseParagraph(StreamReader reader)
    {
        ArgumentNullException.ThrowIfNull(reader, nameof(reader));

        var all = reader.ReadToEnd();
        var allLines = _splitOnEndingPunctuation(all);
        var stringsSlice = new List<string>();
        foreach (var line in allLines)
        {
            var phrases = _splitLongPhrases(line);
            if (phrases != null)
            {
                stringsSlice.AddRange(phrases);
            }
        }
        return stringsSlice;
    }

    private static List<string> _parseOnePhrasePerLine(StreamReader reader)
    {
        ArgumentNullException.ThrowIfNull(reader, nameof(reader));

        var stringsSlice = new List<string>();
        var line = reader.ReadLine();
        while (line != null)
        {
            line = line.Trim();
            if (!string.IsNullOrEmpty(line))
            {
                var phrases = _splitLongPhrases(line);
                if (phrases != null)
                {
                    stringsSlice.AddRange(phrases);
                }
            }
            line = reader.ReadLine();
        }
        return stringsSlice;
    }

    private static List<string>? _splitLongPhrases(string line)
    {
        line = line.Trim();
        var words = line.Split(' ');

        if (words.Length <= 3)
        {
            return null;
        }
        
        if (words.Length < 8)
        {
            return new List<string> {line};
        }

        var splitString = _splitOnPunctuationBreak(line).
            Where(s => !string.IsNullOrEmpty(s) && s != ".").ToList();
        var keptStrings = new List<string>();
        

        for (var i = 0; i < splitString.Count; i++)
        {
            if (splitString[i].Split(' ').Length > 4)
            {
                keptStrings.Add(splitString[i]);
            }
            else if (i < splitString.Count - 1)
            {
                var combined = splitString[i] + " " + splitString[i + 1];
                i++;
                if (combined.Split(' ').Length > 4)
                {
                    keptStrings.Add(combined);
                }
                else if (splitString[i].Split(' ').Length < 4 && i < splitString.Count - 1)
                {
                    combined =  combined + " " + splitString[i + 1];
                    keptStrings.Add(combined);
                    i++;
                }
            }
            else if (splitString[i].Split(' ').Length < 4 && i > 0)
            {
                keptStrings[^1] += " " + splitString[i];
            }
            else
            {
                keptStrings.Add(splitString[i]);
            }
        }

        return keptStrings.Select(s => s.Replace("  ", " ").Trim()).ToList();
    }
}