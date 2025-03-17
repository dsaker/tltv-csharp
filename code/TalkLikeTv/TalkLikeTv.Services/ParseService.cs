using System.IO;
using System.Text.RegularExpressions;

namespace TalkLikeTv.Services;

public partial class ParseService
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

    public static FileInfo ParseFile(Stream fileStream,string fileName, int maxPhrases)
    {
        ArgumentNullException.ThrowIfNull(fileStream, nameof(fileStream));

        Console.WriteLine($"File size: {fileStream.Length} bytes");

        if (fileStream.Length > 8192*8)
        {
            throw new Exception($"File too large ({fileStream.Length} > {8192*8} bytes)");
        }

        var stringsList = _getLines(fileStream);

        var txtPath = Path.Combine("/tmp/ParseFile/", fileName);
        
        // Create outputs folder to hold all the txt files to zip
        Directory.CreateDirectory(txtPath);
        // var filePath = Path.Combine(txtPath, "tooLongPhrases.txt");
        // using var writer = new StreamWriter(filePath);
        
        var tooLongPhrases = new List<string>();
        for( var i=0; i < stringsList.Count; i++)
        {
            // 128 is the max length of a phrase in db
            if (stringsList[i].Length > 128)
            {
                tooLongPhrases.Add(stringsList[i]);
                stringsList.RemoveAt(i);
            }
        }

        if (tooLongPhrases.Count > 0)
        {
            var filePath = Path.Combine(txtPath, "tooLongPhrases.txt");
            using var writer = new StreamWriter(filePath);
            foreach (var phrase in tooLongPhrases)
            {
                writer.WriteLine(phrase);
            }
        }

        var file = ZipDirService.ZipStringsList(stringsList, maxPhrases, txtPath, fileName);

        return file;
    }

    private static List<string> _getLines(Stream fileStream)
    {
        using var reader = new StreamReader(fileStream);
        var textFormat = TextFormatDetector.DetectTextFormat(fileStream);

        fileStream.Seek(0, SeekOrigin.Begin);
        return textFormat switch
        {
            TextFormatDetector.TextFormat.Srt => _parseSrt(reader),
            TextFormatDetector.TextFormat.Paragraph => _parseParagraph(reader),
            TextFormatDetector.TextFormat.OnePhrasePerLine => ParseOnePhrasePerLine(reader),
            _ => ParseOnePhrasePerLine(reader)
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

    public static List<string> ParseOnePhrasePerLine(StreamReader reader)
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