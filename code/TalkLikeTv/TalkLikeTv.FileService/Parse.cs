using System.Text.RegularExpressions;

namespace TalkLikeTv.FileService;

public partial class Parse
{
    [GeneratedRegex(@"\[.*?\]|\{.*?\}|<.*?>|♪|-|\""")]
    private static partial Regex ReplaceFmt();  
    private static string _replaceFmt(string input) => ReplaceFmt().Replace(input, "");
    
    [GeneratedRegex("[!.?,;]")]
    private static partial Regex SplitOnPunctuationBreak();  
    private static string[] _splitOnPunctuationBreak(string input) => SplitOnPunctuationBreak().Split(input);
    
    [GeneratedRegex("[!.?]")]
    private static partial Regex SplitOnEndingPunctuation();  
    private static string[] _splitOnEndingPunctuation(string input) => SplitOnEndingPunctuation().Split(input);
    
    public static List<string> ParseFile(FileStream? fileStream)
    {
        ArgumentNullException.ThrowIfNull(fileStream, nameof(fileStream));

        Console.WriteLine($"File size: {fileStream.Length} bytes");

        if (fileStream.Length > 8192)
        {
            throw new Exception($"File too large ({fileStream.Length} > 8192 bytes)");
        }

        return _getLines(fileStream);
    }

    private static List<string> _getLines(FileStream fileStream)
    {
        var reader = new StreamReader(fileStream);
        var textFormat = TextFormatDetector.DetectTextFormat(fileStream);
       
        fileStream.Seek(0, SeekOrigin.Begin);
        if (textFormat == TextFormatDetector.TextFormat.Srt)
        {
            return _parseSrt(reader);
        }
        if (textFormat == TextFormatDetector.TextFormat.Paragraph)
        {
            return _parseParagraph(reader);
        }
        if (textFormat == TextFormatDetector.TextFormat.OnePhrasePerLine)
        {
            return _parseOnePhrasePerLine(reader);
        }

        fileStream.Seek(0, SeekOrigin.Begin);
        return _parseSrt(reader);

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
        var words = line.Split(' ');

        // if phrase is too short skip it
        if (words.Length <= 3)
        {
            return null;
        }
        
        var splitString = _splitOnPunctuationBreak(line);
        var keptStrings = new List<string>();

        for (var i = 0; i < splitString.Length; i++)
        {
            // if before last string see if combining with next string is less than 10 words
            if (i < splitString.Length - 1)
            {
                var combined = splitString[i] + " " + splitString[i + 1];
                if (splitString[i].Split(' ').Length < 4 || combined.Split(' ').Length <= 10)
                {
                    keptStrings.Add(combined);
                    i++;
                }
                else
                {
                    keptStrings.Add(splitString[i]);
                }
            }
            // if last string is less than 4 words combine with previous string
            else if (splitString[i].Split(' ').Length < 4 && i > 0)
            {
                keptStrings[^1] += " " + splitString[i];
            }
            else
            {
                keptStrings.Add(splitString[i]);
            }
        }

        // replace any double spaces with single spaces
        return keptStrings.Select(s => s.Replace("  ", " ").Trim()).ToList();
    }
}