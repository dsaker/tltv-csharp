using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace TranslateTextArray;

class Program
{
    private static readonly string endpoint = "https://api.cognitive.microsofttranslator.com/translate"; // e.g., "https://api.cognitive.microsofttranslator.com/translate"
	private readonly string? _subscriptionKey = Environment.GetEnvironmentVariable("AZURE_TRANSLATE_SUBSCRIPTION_KEY");
    if (_subscriptionKey == null)
    {
        Console.Error.WriteLine("Error: AZURE_TRANSLATE_SUBSCRIPTION_KEY environment variable is not set.");
        Environment.Exit(1);
    }

    static async Task Main()
    {
        List<string> texts = new List<string>
        {
            "Hello, how are you?",
            "This is an example.",
            "Azure Translator is powerful."
        };

        var translations = await TranslateTextAsync(texts, "en", "es");

        Console.WriteLine("Translated Texts:");
        foreach (var translation in translations)
        {
            Console.WriteLine(translation);
        }
    }

    static async Task<List<string>> TranslateTextAsync(List<string> texts, string fromLanguage, string toLanguage)
    {
        using (HttpClient client = new HttpClient())
        {
            // Construct the request URL
            string route = $"{endpoint}?api-version=3.0&from={fromLanguage}&to={toLanguage}";

            // Set headers
            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", subscriptionKey);
            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Region", region);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            // Prepare request body
            var requestBody = new List<object>();
            foreach (var text in texts)
            {
                requestBody.Add(new { text });
            }
            string requestJson = JsonSerializer.Serialize(requestBody);
            StringContent content = new StringContent(requestJson, Encoding.UTF8, "application/json");

            // Make the API request
            HttpResponseMessage response = await client.PostAsync(route, content);

            // Parse response
            if (response.IsSuccessStatusCode)
            {
                string responseBody = await response.Content.ReadAsStringAsync();
                var translationResponse = JsonSerializer.Deserialize<List<TranslationResponse>>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                // Extract translated texts
                List<string> translatedTexts = new List<string>();
                foreach (var item in translationResponse)
                {
                    translatedTexts.Add(item.Translations[0].Text);
                }
                return translatedTexts;
            }
            else
            {
                throw new Exception($"Error: {response.StatusCode}, {await response.Content.ReadAsStringAsync()}");
            }
        }
    }

    // Model for API response
    public class TranslationResponse
    {
        public List<Translation> Translations { get; set; }
    }

    public class Translation
    {
        public string Text { get; set; }
        public string To { get; set; }
    }
}