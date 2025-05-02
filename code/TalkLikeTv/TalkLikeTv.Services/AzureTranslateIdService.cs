using Azure;
using Azure.Identity;
using TalkLikeTv.Services.Abstractions;
using Azure.AI.Translation.Text;

namespace TalkLikeTv.Services;

public class AzureTranslateIdService : IAzureTranslateService
{
    private readonly TextTranslationClient _client;
    

    public AzureTranslateIdService()
    {
        // Get environment name
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";
        Console.WriteLine($"Current environment: {environment}");
        var endpoint = Environment.GetEnvironmentVariable("AZURE_TRANSLATE_ENDPOINT");
        if (endpoint == null)
        {
            throw new InvalidOperationException("AZURE_TRANSLATE_ENDPOINT must be set in environment variables.");
        }
        
        if (environment == "Production")
        {
            // system-assigned managed identity
            var credential = new DefaultAzureCredential();
            _client = new TextTranslationClient(credential, new Uri(endpoint));;
        }
        else if (environment == "GitHub")
        {
            // service principal 
            var tenantId = Environment.GetEnvironmentVariable("AZURE_TENANT_ID");
            var clientId = Environment.GetEnvironmentVariable("AZURE_CLIENT_ID");
            var clientSecret = Environment.GetEnvironmentVariable("AZURE_CLIENT_SECRET");
            
            var credential = new ClientSecretCredential(tenantId, clientId, clientSecret);
            _client = new TextTranslationClient(credential, new Uri(endpoint));
        }
        else
        {
            // user-assigned managed identity
            var credential = new DefaultAzureCredential(
                new DefaultAzureCredentialOptions
                {
                    ManagedIdentityClientId = Environment.GetEnvironmentVariable("AZURE_CLIENT_ID")
                });
            _client = new TextTranslationClient(credential, new Uri(endpoint));
        }
    }

    private async Task<string> DetectLanguageAsync(string textToDetect, CancellationToken cancellationToken = default)
    {
        try
        {
            Response<IReadOnlyList<TranslatedTextItem>> response = await _client.TranslateAsync("en", textToDetect, null, cancellationToken);
            var translations = response.Value;
            var translation = translations[0];

            if (translation.DetectedLanguage != null)
            {
                Console.WriteLine($"Detected languages of the input text: {translation.DetectedLanguage.Language} with score: {translation.DetectedLanguage.Confidence}.");
                Console.WriteLine($"Text was translated to: '{translation.Translations?[0]?.TargetLanguage}' and the result is: '{translation.Translations?[0]?.Text}'.");
                return translation.DetectedLanguage.Language;
            }
            throw new Exception("Failed to detect language.");
        }
        catch (RequestFailedException exception)
        {
            Console.WriteLine($"Error Code: {exception.ErrorCode}");
            Console.WriteLine($"Message: {exception.Message}");
            throw;
        }
    }

    public async Task<string> DetectLanguageFromPhrasesAsync(List<string> phraseStrings, CancellationToken cancellationToken = default)
    {
        if (phraseStrings.Count < 3)
        {
            throw new ArgumentException("At least three phrases are required.");
        }

        var languages = new List<string>();
        for (var i = 0; i < 3; i++)
        {
            var language = await DetectLanguageAsync(phraseStrings[i], cancellationToken);
            languages.Add(language);
        }

        var languageGroups = languages.GroupBy(l => l).Where(g => g.Count() >= 2).ToList();
        if (languageGroups.Count == 0)
        {
            throw new Exception("All three phrases are detected to be different languages.");
        }

        return languageGroups.First().Key;
    }

    public async Task<List<string>> TranslatePhrasesAsync(List<string> phrases, string fromLanguage, string toLanguage, CancellationToken cancellationToken = default)
    {
        try
        {
            Response<IReadOnlyList<TranslatedTextItem>> response = await _client.TranslateAsync(toLanguage, phrases, fromLanguage, cancellationToken).ConfigureAwait(false);
            var translations = response.Value;
            
            var result = new List<string>();
            foreach (var translation in translations)
            {
                if (translation.Translations != null && translation.Translations.Any())
                {
                    result.Add(translation.Translations[0].Text);
                }
            }
            
            return result;
        }
        catch (RequestFailedException exception)
        {
            Console.WriteLine($"Error Code: {exception.ErrorCode}");
            Console.WriteLine($"Message: {exception.Message}");
            throw;
        }
    }
}