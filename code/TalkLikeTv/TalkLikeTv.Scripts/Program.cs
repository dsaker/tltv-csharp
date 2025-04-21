using DotEnv.Core;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.DependencyInjection;
using TalkLikeTv.EntityModels;
using TalkLikeTv.Repositories;
using TalkLikeTv.Services;

namespace TalkLikeTv.Scripts;

internal class Program
{
    private static bool _isValidSelection(string[] selected)
    {
        return selected.Any(s => s == "all" || s == "languages" || s == "voices" || s == "delete" || s == "audio" || s == "translates" || s == "tokens");
    }
    
    private static async Task Main(string[] args)
    {
        new EnvLoader().AddEnvFile("../TalkLikeTv.Mvc/.env").Load();
        
        var selected = args.Select(arg => arg.ToLowerInvariant()).ToArray();

        if (!_isValidSelection(selected))
        {
            Console.WriteLine("Error: You must specify 'voices', 'languages', 'translates', 'audio', or 'tokens'.");
            PrintUsage();
            return;
        }
        
        // Configure services
        var serviceCollection = new ServiceCollection();
        ConfigureServices(serviceCollection);
        var serviceProvider = serviceCollection.BuildServiceProvider();

        // Resolve dependencies
        var dbContext = serviceProvider.GetRequiredService<TalkliketvContext>();
        var tokenService = serviceProvider.GetRequiredService<TokenService>();
        
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
        
        if (selected.Contains("tokens"))
        {
            Console.WriteLine("Enter the number of tokens to generate:");
            if (!int.TryParse(Console.ReadLine(), out var numTokens) || numTokens <= 0)
            {
                Console.WriteLine("Invalid number. Please enter a positive integer.");
                return;
            }

            var tokens = new List<Token>();
            var plaintexts = new List<string>();

            for (var i = 0; i < numTokens; i++)
            {
                var (token, plaintext) = tokenService.GenerateToken();
                tokens.Add(token);
                plaintexts.Add(plaintext);
            }

            await dbContext.Tokens.AddRangeAsync(tokens);
            await dbContext.SaveChangesAsync();

            Console.WriteLine("Generated tokens:");
            plaintexts.ForEach(Console.WriteLine);
        }

        Console.WriteLine("Upload process complete.");
    }

    private static void PrintUsage()
    {
        Console.WriteLine("Usage: Upload [voices|languages|all|audio|translates|tokens]");
        Console.WriteLine("Example:");
        Console.WriteLine("  Upload all models: Upload all");
        Console.WriteLine("  Upload only voices: Upload voices");
        Console.WriteLine("  Generate tokens: Upload tokens");
    }
    
    private static void ConfigureServices(IServiceCollection services)
    {
        // Register DbContext
        services.AddDbContext<TalkliketvContext>();

        // Register repositories
        services.AddScoped<ITokenRepository, TokenRepository>(); // Register the interface and its implementation

        // Register services
        services.AddScoped<TokenService>();

        // Add HybridCache
        #pragma warning disable EXTEXP0018
        services.AddHybridCache(options =>
        {
            options.DefaultEntryOptions = new HybridCacheEntryOptions
            {
                Expiration = TimeSpan.FromSeconds(120),
                LocalCacheExpiration = TimeSpan.FromSeconds(60)
            };
        });
        #pragma warning restore EXTEXP0018
    }
}