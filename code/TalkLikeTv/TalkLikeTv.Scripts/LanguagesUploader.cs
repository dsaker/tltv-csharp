using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using TalkLikeTv.EntityModels;

namespace TalkLikeTv.Scripts;

public class LanguagesUploader(TalkliketvContext db)
{
    private const string JsonFilePath = "/Users/dustysaker/Documents/csharp-repos/tltv-net9/code/TalkLikeTv/TalkLikeTv.Scripts/json/languages.json";

    public class Translation
    {
        [JsonPropertyName("translation")]
        public required Dictionary<string, LanguageDetails> Languages { get; set; }
    }

    public class LanguageDetails
    {
        [JsonPropertyName("name")]
        public required string Name { get; set; }

        [JsonPropertyName("nativeName")]
        public required string NativeName { get; set; }

        [JsonPropertyName("dir")]
        public required string Direction { get; set; }
    }


    public async Task UploadJson()
    {
        try
        {
            // Read the JSON file
            var jsonString = await File.ReadAllTextAsync(JsonFilePath);

            var translation = JsonSerializer.Deserialize<Translation>(jsonString);

            if (translation != null)
            {
                // Print out the loaded entities
                foreach (var language in translation.Languages)
                {
                    try
                    {
                        var existingLanguage = await db.Languages.FirstOrDefaultAsync(l => l.Name == language.Value.Name);
                        if (existingLanguage == null)
                        {
                            try
                            {
                                var entity = new Language
                                {
                                    Name = language.Value.Name,
                                    NativeName = language.Value.NativeName,
                                    Tag = language.Key,
                                };
                                db.Languages.Add(entity);
                                await db.SaveChangesAsync();
                                Console.WriteLine($"Added language: {language.Value.Name}");
                            }
                            catch (DbUpdateException ex)
                            {
                                Console.WriteLine($"Error adding language: {ex.Message}");
                                if (ex.InnerException != null)
                                {
                                    Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                                }
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"Error adding language: {ex.Message}");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Language {language.Value.Name} already exists.");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error finding voice: {ex.Message}");
                    }
                }
            }
            else
            {
                Console.WriteLine("No products found in JSON file.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }
}