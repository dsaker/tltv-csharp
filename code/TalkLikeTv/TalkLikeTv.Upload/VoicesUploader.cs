using Microsoft.Data.SqlClient;
using TalkLikeTv.EntityModels;
using Newtonsoft.Json;

namespace TalkLikeTv.Upload;

public class VoicesUploader(TalkliketvContext db)
{
    private const string JsonFilePath = "/Users/dustysaker/Documents/csharp-repos/tltv-net9/code/TalkLikeTv/TalkLikeTv.Upload/json/voices.json";
    
    public async Task UploadJson()
    {
        try
        {
            // Read the JSON file
            string jsonString = await File.ReadAllTextAsync(JsonFilePath);

            // Deserialize JSON into a List of Product entities
            List<Voice>? voices = JsonConvert.DeserializeObject<List<Voice>>(jsonString);

            if (voices != null)
            {
                // Print out the loaded entities
                foreach (var voice in voices)
                {
                    Console.WriteLine($"DisplayName: {voice.DisplayName}, Gender: {voice.Gender}, LocaleName: {voice.LocaleName}");
                    try
                    {
                        db.Voices.Add(voice);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error adding voice: {ex.Message}");
                    }
                }
            }
            else
            {
                Console.WriteLine("No voices found in JSON file.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }
}