using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TalkLikeTv.EntityModels;
using TalkLikeTv.Mvc.Models;

namespace TalkLikeTv.Mvc.Controllers;

public class TitlesController : Controller
{
    private readonly TalkliketvContext _db;

    public TitlesController(TalkliketvContext db)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
    }

    [HttpGet]
    public async Task<IActionResult> SearchTitles()
    {
        var languages = await _db.Languages
            .Where(l => l.Titles.Any())
            .OrderBy(l => l.Name)
            .ToListAsync();

        var model = new SearchTitlesViewModel
        {
            TitleLanguages = languages
        };

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> SearchTitles(SearchTitlesViewModel model)
    {
        const int pageSize = 10;
        IQueryable<Title> query;

        if (model.SearchType == "Language")
        {
            query = _db.Titles.Include(t => t.OriginalLanguage).Where(t => t.OriginalLanguageId == model.OriginalLanguageId);
        }
        else if (model.SearchType == "Keyword")
        {
            query = _db.Titles.Include(t => t.OriginalLanguage).Where(t => (t.TitleName ?? "").Contains(model.Keyword ?? "") || (t.Description ?? "").Contains(model.Keyword ?? ""));
        }
        else // Both
        {
            query = _db.Titles.Include(t => t.OriginalLanguage).Where(t => t.OriginalLanguageId == model.OriginalLanguageId && ((t.TitleName ?? "").Contains(model.Keyword ?? "") || (t.Description ?? "").Contains(model.Keyword ?? "")));
        }

        model.TotalPages = (int)Math.Ceiling(await query.CountAsync() / (double)pageSize);
        model.Results = await query
            .Skip((model.PageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        // Reload languages for the dropdown
        model.TitleLanguages = await _db.Languages
            .Where(l => l.Titles.Any())
            .OrderBy(l => l.Name)
            .ToListAsync();

        return View(model);
    }
}