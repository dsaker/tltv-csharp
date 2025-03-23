using Microsoft.EntityFrameworkCore;
using TalkLikeTv.EntityModels;
using TalkLikeTv.Mvc.Models; 

namespace TalkLikeTv.Services;

public class ProcessTitleService
{
    private readonly DbContext _db;

    public ProcessTitleService(DbContext db)
    {
        _db = db;
    }

    private async Task<Title> ProcessTitleAsync(CreateTitleFormModel formModel, List<string> phraseStrings, Language detectedLanguage)
    {
        var languageId = detectedLanguage.LanguageId;
        var newTitle = new Title
        {
            TitleName = formModel.TitleName!,
            Description = formModel.Description,
            NumPhrases = phraseStrings.Count,
            OriginalLanguageId = languageId,
        };

        _db.Titles.Add(newTitle);
        await _db.SaveChangesAsync();

        var phrases = phraseStrings.Select(_ => new Phrase
        {
            TitleId = newTitle.TitleId,
        }).ToList();

        _db.Phrases.AddRange(phrases);
        await _db.SaveChangesAsync();

        return newTitle;
    }
}