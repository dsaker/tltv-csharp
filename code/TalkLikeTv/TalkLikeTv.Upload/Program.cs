using TalkLikeTv.EntityModels;

namespace TalkLikeTv.Upload;

internal class Program
{
    private static async Task Main(string[] args)
    {
        var selected = args.Select(arg => arg.ToLowerInvariant()).ToList();

        if (!selected.Contains("all") && !selected.Contains("languages") && !selected.Contains("voices") && !selected.Contains("delete"))
        {
            Console.WriteLine("Error: You must specify 'voices', 'languages', or 'all'.");
            PrintUsage();
            return;
        }
        
        var dbContext = new TalkliketvContext();
        if (selected.Contains("all") || selected.Contains("languages"))
        {
            Console.WriteLine("Starting languages upload...");
            var languagesUploader = new LanguagesUploader(dbContext);
            await languagesUploader.UploadJson();
        }

        if (selected.Contains("all") || selected.Contains("voices"))
        {
            Console.WriteLine("Starting voices upload...");
            var voicesUploader = new VoicesUploader(dbContext);
            await voicesUploader.UploadJson();
        }
        
        // delete languages with no voices
        if (selected.Contains("all") || selected.Contains("delete"))
        {
            Console.WriteLine("Starting delete languages with no voices...");
            var languagesWithNoVoicesDeleter = new LanguagesWithNoVoicesDeleter(dbContext);
            await languagesWithNoVoicesDeleter.DeleteLanguagesWithNoVoices();
        }

        Console.WriteLine("Upload process complete.");
    }

    private static void PrintUsage()
    {
        Console.WriteLine("Usage: Upload [voices|languages|all]");
        Console.WriteLine("Example:");
        Console.WriteLine("  Upload all models: Upload all");
        Console.WriteLine("  Upload only voices: Upload voices");
    }
}