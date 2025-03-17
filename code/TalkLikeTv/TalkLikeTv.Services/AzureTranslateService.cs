using System.Net.Http;
using System.Text;
using System.Text.Json;
using Newtonsoft.Json;

namespace TalkLikeTv.Services;

public class AzureTranslateService
{
    private const string Endpoint = "https://api.cognitive.microsofttranslator.com";
    private static readonly HttpClient HttpClient = new();
    private static readonly string SubscriptionKey = Environment.GetEnvironmentVariable("AZURE_TRANSLATE_KEY") 
                                                     ?? throw new InvalidOperationException("AZURE_TRANSLATE_KEY environment variable is not set.");
    private static readonly string Region = Environment.GetEnvironmentVariable("AZURE_REGION") 
                                            ?? throw new InvalidOperationException("AZURE_REGION environment variable is not set.");    

    private static async Task<string> DetectLanguageAsync(string textToDetect)
    {
        const string route = "/detect?api-version=3.0";
        // Input and output languages are defined as parameters.
        object[] body = { new { Text = textToDetect } };
        var requestBody = JsonConvert.SerializeObject(body);

        // Build the request.
        var request = new HttpRequestMessage
        {
            Method = HttpMethod.Post,
            RequestUri = new Uri(Endpoint + route),
            Content = new StringContent(requestBody, Encoding.UTF8, "application/json")
        };
        request.Headers.Add("Ocp-Apim-Subscription-Key", SubscriptionKey);
        // location required if you're using a multi-service or regional (not global) resource.
        request.Headers.Add("Ocp-Apim-Subscription-Region", Region);
        //var requestBody = JsonSerializer.Serialize(new object[] { new { Text = textToDetect } });
        var response = await HttpClient.SendAsync(request).ConfigureAwait(false);
        // Read response as a string.
        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"Failed to detect language. Status code: {response.StatusCode}");
        }
        
        var jsonResponse = await response.Content.ReadAsStringAsync();
        var resultDocument = JsonDocument.Parse(jsonResponse);
        var languageElement = resultDocument.RootElement[0].GetProperty("language");
        if (languageElement.ValueKind == JsonValueKind.Null || languageElement.GetString() == null)
        {
            throw new Exception("Failed to detect language.");
        }

        return languageElement.GetString()!;
    }

    public async Task<string> DetectLanguageFromPhrasesAsync(List<string> phraseStrings)
    {
        if (phraseStrings.Count < 3)
        {
            throw new ArgumentException("At least three phrases are required.");
        }

        var languages = new List<string>();
        for (var i = 0; i < 3; i++)
        {
            var language = await DetectLanguageAsync(phraseStrings[i]);
            languages.Add(language);
        }

        var languageGroups = languages.GroupBy(l => l).Where(g => g.Count() >= 2).ToList();
        if (languageGroups.Count == 0)
        {
            throw new Exception("All three phrases are detected to be different languages.");
        }

        return languageGroups.First().Key;
    }
    
    public async Task<List<string>> TranslatePhrasesAsync(List<string> phrases, string fromLanguage, string toLanguage)
    {
        const string route = "/translate?api-version=3.0";
        var uri = $"{Endpoint}{route}&from={fromLanguage}&to={toLanguage}";

        var requestBody = JsonConvert.SerializeObject(phrases.Select(phrase => new { Text = phrase }).ToArray());

        var request = new HttpRequestMessage
        {
            Method = HttpMethod.Post,
            RequestUri = new Uri(uri),
            Content = new StringContent(requestBody, Encoding.UTF8, "application/json")
        };
        request.Headers.Add("Ocp-Apim-Subscription-Key", SubscriptionKey);
        request.Headers.Add("Ocp-Apim-Subscription-Region", Region);

        var response = await HttpClient.SendAsync(request).ConfigureAwait(false);
        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"Failed to translate phrases. Status code: {response.StatusCode}");
        }

        var jsonResponse = await response.Content.ReadAsStringAsync();
        var resultDocument = JsonDocument.Parse(jsonResponse);
        var translations = resultDocument.RootElement
            .EnumerateArray()
            .Select(element => element.GetProperty("translations")[0].GetProperty("text").GetString())
            .ToList();

        return translations!;
    }
}