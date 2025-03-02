using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using TalkLikeTv.EntityModels;
using Newtonsoft.Json;

namespace TalkLikeTv.Scripts;

public class VoicesUploader(TalkliketvContext db)
{
    private const string JsonFilePath = "/Users/dustysaker/Documents/csharp-repos/tltv-net9/code/TalkLikeTv/TalkLikeTv.Scripts/json/voices.json";
    
    // Define the structure of the JSON file for desearlization
    private class JsonVoice
    {
        public required string DisplayName { get; set; }
        public required string LocalName { get; set; }
        public required string ShortName { get; set; }
        public required string Gender { get; set; }
        public required string Locale { get; set; }
        public required string LocaleName { get; set; }
        public List<string> StyleList { get; set; } = new();
        public int SampleRateHertz { get; set; }
        public required string VoiceType { get; set; }
        public required string Status { get; set; }
        public required VoiceTag VoiceTag { get; set; }
        public int WordsPerMinute { get; set; }
    }

    private class VoiceTag
    {
        public List<string> TailoredScenarios { get; set; } = new();
        public List<string> VoicePersonalities { get; set; } = new();
    }
    public async Task UploadJson()
    {
        try
        {
            // Read the JSON file
            string jsonString = await File.ReadAllTextAsync(JsonFilePath);

            // Deserialize JSON into a List of Product entities
            List<JsonVoice>? jsonVoices = JsonConvert.DeserializeObject<List<JsonVoice>>(jsonString);

            if (jsonVoices != null)
            {
                // Print out the loaded entities
                 foreach (var jsonVoice in jsonVoices)
                {
                    try
                    {
                        var existingVoice = await db.Voices.FirstOrDefaultAsync(v => v.DisplayName == jsonVoice.DisplayName);
                        if (existingVoice == null)
                        {
                            var dbVoice = new Voice
                            {
                                DisplayName = jsonVoice.DisplayName,
                                LocalName = jsonVoice.LocalName,
                                ShortName = jsonVoice.ShortName,
                                Locale = jsonVoice.Locale,
                                LocaleName = jsonVoice.LocaleName,
                                Gender = jsonVoice.Gender,
                                SampleRateHertz = jsonVoice.SampleRateHertz,
                                VoiceType = jsonVoice.VoiceType,
                                Status = jsonVoice.Status,
                                WordsPerMinute = jsonVoice.WordsPerMinute,
                            };
                                
                            var insertedVoice = await InsertVoice(dbVoice);
                            if (insertedVoice != null)
                            {
                                if (jsonVoice.StyleList.Count > 0)
                                {
                                    await InsertStyles(insertedVoice, jsonVoice.StyleList);
                                }
                                if (jsonVoice.VoiceTag?.TailoredScenarios?.Count > 0)
                                {
                                    await InsertScenarios(insertedVoice, jsonVoice.VoiceTag.TailoredScenarios);
                                }
                                if (jsonVoice.VoiceTag?.VoicePersonalities?.Count > 0)
                                {
                                    await InsertPersonalities(insertedVoice, jsonVoice.VoiceTag.VoicePersonalities);
                                }
                            }
                            else
                            {
                                Console.WriteLine($"Failed to insert voice: {jsonVoice.DisplayName}");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Voice {jsonVoice.DisplayName} already exists.");
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
                Console.WriteLine("No voices found in JSON file.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }
    
    private async Task<Voice?> InsertVoice(Voice voice)
    {
        try
        {
            // Get the language from the locale 
            var tag = voice.Locale;
            var language = await db.Languages.FirstOrDefaultAsync(l => l.Tag == tag);
            if (language == null)
            {
                tag = voice.Locale.Split('-')[0];
                language = await db.Languages.FirstOrDefaultAsync(l => l.Tag == tag);
            }
            if (language != null && voice.Status != "Deprecated")
            {
                voice.LanguageId = language.LanguageId;
                var dbVoice = db.Voices.Add(voice);
                await db.SaveChangesAsync();
                Console.WriteLine($"Added Voice: {voice.DisplayName}");
                return dbVoice.Entity;
            }

            Console.WriteLine($"Language with tag {tag} not found for voice {voice.DisplayName}");
            return null;
        }
        catch (DbUpdateException ex)
        {
            Console.WriteLine($"Error adding Voice {voice.DisplayName}: {ex.Message}");
            if (ex.InnerException != null)
            {
                Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
            }

            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error adding Voice: {ex.Message}");
            return null;
        }
    }

    private async Task InsertStyles(Voice voice, List<string> styleList)
    {
        foreach (var style in styleList)
        {
            var existingStyle = await db.Styles.FirstOrDefaultAsync(s => s.StyleName == style);
            if (existingStyle == null)
            {
                var dbStyle = new Style
                {
                    StyleName = style,
                };
                var insertedStyle = db.Styles.Add(dbStyle);
                await db.SaveChangesAsync();
                Console.WriteLine($"Added Style: {style}");
                // Add the style to the voice
                voice.Styles.Add(insertedStyle.Entity);
            }
            else
            {
                voice.Styles.Add(existingStyle);
            }
            
            Console.WriteLine($"Added Style {style} to Voice {voice.DisplayName}");
        }
    }
    
    private async Task InsertScenarios(Voice voice, List<string> scenarios)
    {
        foreach (var scenario in scenarios)
        {
            var existingScenario = await db.Scenarios.FirstOrDefaultAsync(s => s.ScenarioName == scenario);
            if (existingScenario == null)
            {
                var dbScenario = new Scenario
                {
                    ScenarioName = scenario,
                };
                var insertedScenario = db.Scenarios.Add(dbScenario);
                await db.SaveChangesAsync();
                Console.WriteLine($"Added Scenario: {scenario}");
                // Add the style to the voice
                voice.Scenarios.Add(insertedScenario.Entity);
            }
            else
            {
                voice.Scenarios.Add(existingScenario);
            }
            
            Console.WriteLine($"Added Scenario {scenario} to Voice {voice.DisplayName}");
        }
    }
    
    private async Task InsertPersonalities(Voice voice, List<string> personalities)
    {
        // Add the personalities to the voice
        foreach (var personality in personalities) 
        {
            var existingPersonality = await db.Personalities.FirstOrDefaultAsync(p => p.PersonalityName == personality);
            if (existingPersonality == null)
            {
                var dbPersonality = new Personality
                {
                    PersonalityName = personality,
                };
                var insertedPersonality = db.Personalities.Add(dbPersonality);
                await db.SaveChangesAsync();
                Console.WriteLine($"Added Personality: {personality}");
                // Add the style to the voice
                voice.Personalities.Add(insertedPersonality.Entity);
            }
            else
            {
                voice.Personalities.Add(existingPersonality);
            }
            
            Console.WriteLine($"Added Personality {personality} to Voice {voice.DisplayName}");
        }
    }
}




