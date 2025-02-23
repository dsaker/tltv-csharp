using TalkLikeTv.EntityModels;

namespace TalkLikeTv.Upload;

internal class Program
{
    private static async Task Main(string[] args)
    {
        var selectedModels = args.Select(arg => arg.ToLowerInvariant()).ToList();

        if (!selectedModels.Contains("all") && !selectedModels.Contains("languages") && !selectedModels.Contains("voices"))
        {
            Console.WriteLine("Error: You must specify 'voices', 'languages', or 'all'.");
            PrintUsage();
            return;
        }
        
        var dbContext = new TalkliketvContext();
        if (selectedModels.Contains("all") || selectedModels.Contains("languages"))
        {
            Console.WriteLine("Starting languages upload...");
            var languagesUploader = new LanguagesUploader(dbContext);
            await languagesUploader.UploadJson();
        }

        if (selectedModels.Contains("all") || selectedModels.Contains("voices"))
        {
            Console.WriteLine("Starting voices upload...");
            var voicesUploader = new VoicesUploader(dbContext);
            await voicesUploader.UploadJson();
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