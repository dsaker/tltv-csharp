using Microsoft.EntityFrameworkCore;
using TalkLikeTv.EntityModels;

namespace TalkLikeTv.Upload;

public class LanguagesWithNoVoicesDeleter(TalkliketvContext db)
{
    
    public async Task DeleteLanguagesWithNoVoices()
    {
        try
        {
            // Find languages with no associated voices
            var languagesWithNoVoices = await db.Languages
                .Where(l => !db.Voices.Any(v => v.LanguageId == l.LanguageId))
                .ToListAsync();

            if (languagesWithNoVoices.Any())
            {
                // Print out the languages to be deleted
                Console.WriteLine("Languages with no voices:");
                foreach (var language in languagesWithNoVoices)
                {
                    Console.WriteLine($"- {language.Name} ({language.Tag})");
                }
                // Delete the languages
                db.Languages.RemoveRange(languagesWithNoVoices);
                await db.SaveChangesAsync();
                Console.WriteLine($"Deleted {languagesWithNoVoices.Count} languages with no voices.");
            }
            else
            {
                Console.WriteLine("No languages found with no voices.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error deleting languages: {ex.Message}");
            if (ex.InnerException != null)
            {
                Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
            }
        }
    }
}