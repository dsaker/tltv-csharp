using Microsoft.EntityFrameworkCore;
using TalkLikeTv.EntityModels;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace TalkLikeTv.Scripts;

public class TranslatesUploader(TalkliketvContext db)
{

    private readonly TalkliketvContext _db = db ?? throw new ArgumentNullException(nameof(db));
    private static readonly string SubscriptionKey = Environment.GetEnvironmentVariable("AZURE_TRANSLATOR_KEY") 
                                                     ?? throw new InvalidOperationException("AZURE_TRANSLATOR_KEY environment variable is not set.");
    private const string Endpoint = "https://api.cognitive.microsofttranslator.com/";
    private static readonly string Region = Environment.GetEnvironmentVariable("AZURE_REGION") 
                                     ?? throw new InvalidOperationException("AZURE_REGION environment variable is not set.");
    
    public async Task UploadTranslates()
    {
        var languages = await _db.Languages.ToListAsync();

        // Read the existing JSON file into a JArray
        var jsonFilePath = "/Users/dustysaker/Documents/csharp-repos/tltv-net9/code/TalkLikeTv/TalkLikeTv.Scripts/json/translates.json";
        var jsonArray = new JArray();
        
        //using var synthesizer = new SpeechSynthesizer(config);
        foreach (var language in languages)
        {
            var textToTranslate = "This is what I sound like when I am speaking in " + language.Name.Split(" ")[0];

            try
            {
                // Input and output languages are defined as parameters.
                var route = "/translate?api-version=3.0&from=en&to=" + language.Tag;
                object[] body = [new { Text = textToTranslate }];
                var requestBody = JsonConvert.SerializeObject(body);

                using (var client = new HttpClient())
                using (var request = new HttpRequestMessage())
                {
                    // Build the request.
                    request.Method = HttpMethod.Post;
                    request.RequestUri = new Uri(Endpoint + route);
                    request.Content = new StringContent(requestBody, Encoding.UTF8, "application/json");
                    request.Headers.Add("Ocp-Apim-Subscription-Key", SubscriptionKey);
                    // location required if you're using a multi-service or regional (not global) resource.
                    request.Headers.Add("Ocp-Apim-Subscription-Region", Region);

                    // Send the request and get response.
                    var response = await client.SendAsync(request).ConfigureAwait(false);
                    // Read response as a string.
                    var result = await response.Content.ReadAsStringAsync();
                    //Console.WriteLine(result);

                    // Parse the response JSON
                    var responseJson = JArray.Parse(result);
                    var translations = responseJson[0]["translations"][0]["text"];

                    // Append the new translation result to the JSON array
                    var newData = new JObject
                    {
                        {"Tag", language.Tag },
                        { "Language", language.Name },
                        { "Translations", translations }
                    };
                    Console.WriteLine(newData);
                    jsonArray.Add(newData);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error translating text: {ex.Message}");
                Console.WriteLine($"Exception details: {ex}");
            }
        }
        
        File.WriteAllText(jsonFilePath, jsonArray.ToString());
    }
    
}