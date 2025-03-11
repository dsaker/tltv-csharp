using Microsoft.EntityFrameworkCore;
using TalkLikeTv.EntityModels;
using Microsoft.CognitiveServices.Speech;
using Newtonsoft.Json.Linq;

namespace TalkLikeTv.Scripts;

public class AudioUploader(TalkliketvContext db)
{
    private const string AzureVoicesDir = "/Users/dustysaker/Documents/csharp-repos/tltv-net9/code/TalkLikeTv/TalkLikeTv.Mvc/wwwroot/voices/azure";
    private readonly TalkliketvContext _db = db ?? throw new ArgumentNullException(nameof(db));

    
    public async Task UploadAudio()
    {
        var voices = await _db.Voices.Include((v => v.Language)).ToListAsync();
        var json = File.ReadAllText("/Users/dustysaker/Documents/csharp-repos/tltv-net9/code/TalkLikeTv/TalkLikeTv.Scripts/json/translates.json");
        var jsonArray = JArray.Parse(json);
        // create dictionary to store text for each language
        var textDictionary = new Dictionary<string, string>();
        foreach (var item in jsonArray)
        {
            var language = item["Tag"]?.ToString();
            var text = item["Translations"]?.ToString();
            if (language != null && text != null)
            {
                Console.WriteLine("Adding text to dictionary: " + text);
                textDictionary.Add(language, text);
            }
            else
            {
                Console.WriteLine("Error adding text to dictionary: language or text is null");
                Environment.Exit(1);
            }
        }
        try
        {
            var config = SpeechConfig.FromSubscription(
                Environment.GetEnvironmentVariable("AZURE_SUBSCRIPTION_KEY"),
                Environment.GetEnvironmentVariable("AZURE_REGION")
            );
            
            foreach (var voice in voices)
            {
                var filePath = Path.Combine(AzureVoicesDir, $"{voice.ShortName}.wav");
                if (File.Exists(filePath))
                {
                    Console.WriteLine($"File already exists: {filePath}");
                    continue;
                }
                config.SpeechSynthesisVoiceName = voice.ShortName;
                using var synthesizer = new SpeechSynthesizer(config);
                var voiceTag = voice.Language?.Tag;
                if (voiceTag == null)
                {
                    Console.WriteLine("error: voiceTag is null for voice " + voice.DisplayName);
                    Environment.Exit(1);
                }
                var text = textDictionary[voiceTag];
                var result = await synthesizer.SpeakTextAsync(text);
                if (result.Reason == ResultReason.SynthesizingAudioCompleted)
                {
                    Console.WriteLine($"Speech synthesized to speaker for voice [{voice.DisplayName}]");
                    await File.WriteAllBytesAsync(filePath, result.AudioData);
                    Console.WriteLine($"Speech saved to file: {filePath}");
                }
                else if (result.Reason == ResultReason.Canceled)
                {
                    var cancellation = SpeechSynthesisCancellationDetails.FromResult(result);
                    Console.WriteLine($"CANCELED: Reason={cancellation.Reason}");
                    Console.WriteLine($"CANCELED: ErrorDetails=[{cancellation.ErrorDetails}]");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error synthesizing speech: {ex.Message}");
            if (ex.InnerException != null)
            {
                Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
            }
        }
    }
}