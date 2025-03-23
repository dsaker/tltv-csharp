namespace TalkLikeTv.Utilities;

using System.Text;

public static class StringUtils
{
    public static string MakeHintString(string s)
    {
        var hintString = new StringBuilder();
        var words = s.Split(' ');

        foreach (var word in words)
        {
            hintString.Append(word[0]);
            bool punctuation = char.IsPunctuation(word[0]);

            for (int i = 1; i < word.Length; i++)
            {
                if (punctuation)
                {
                    hintString.Append(word[i]);
                    punctuation = false;
                }
                else if (char.IsLetter(word[i]))
                {
                    hintString.Append('_');
                }
                else
                {
                    hintString.Append(word[i]);
                }
            }

            hintString.Append(' ');
        }

        return hintString.ToString();
    }
}
