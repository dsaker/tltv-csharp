using TalkLikeTv.EntityModels;

namespace TalkLikeTv.Scripts;

internal class Program
{
    private static bool _isValidSelection(string[] selected)
    {
        return selected.Any(s => s == "all" || s == "languages" || s == "voices" || s == "delete" || s == "audio" || s == "translates");
    }
    
    private static async Task Main(string[] args)
    {
        var selected = args.Select(arg => arg.ToLowerInvariant()).ToArray();

        if (!_isValidSelection(selected))
        {
            Console.WriteLine("Error: You must specify 'voices', 'languages', 'translates', or 'audio'.");
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
        
        if (selected.Contains("audio"))
        {
            Console.WriteLine("Starting audio upload...");
            var audioUploader = new AudioUploader(dbContext);
            await audioUploader.UploadAudio();
        }
        
        if (selected.Contains("translates"))
        {
            Console.WriteLine("Starting translates upload...");
            var translatesUploader = new TranslatesUploader(dbContext);
            await translatesUploader.UploadTranslates();
        }

        Console.WriteLine("Upload process complete.");
    }

    private static void PrintUsage()
    {
        Console.WriteLine("Usage: Upload [voices|languages|all|audio|translates]");
        Console.WriteLine("Example:");
        Console.WriteLine("  Upload all models: Upload all");
        Console.WriteLine("  Upload only voices: Upload voices");
    }
}