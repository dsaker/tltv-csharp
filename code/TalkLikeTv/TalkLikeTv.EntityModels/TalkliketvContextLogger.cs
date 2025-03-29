using static System.Environment;

namespace TalkLikeTv.EntityModels;

public class TalkliketvContextLogger
{
    public static void WriteLine(string message)
    {
        var folder = Path.Combine(GetFolderPath(
            SpecialFolder.DesktopDirectory), "book-logs");

        if (!Directory.Exists(folder))
            Directory.CreateDirectory(folder);

        var dateTimeStamp = DateTime.Now.ToString(
            "yyyyMMdd_HHmmss");

        var path = Path.Combine(folder,
            $"talkliketvlog-{dateTimeStamp}.txt");

        var textFile = File.AppendText(path);
        textFile.WriteLine(message);
        textFile.Close();
    }
}