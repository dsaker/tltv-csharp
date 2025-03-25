using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TalkLikeTv.EntityModels;
using TalkLikeTv.Mvc.Models;
using TalkLikeTv.Services;

namespace TalkLikeTv.Mvc.Controllers;

public class TitlesController : Controller
{
    private readonly TalkliketvContext _db;
    private static List<Language>? _cachedLanguages;
    private readonly ILogger<AudioController> _logger;
    private readonly AudioProcessingService _audioProcessingService;
    private readonly IWebHostEnvironment _env;

    public TitlesController(
        ILogger<AudioController> logger, 
        TalkliketvContext db,
        AudioProcessingService audioProcessingService,
        IWebHostEnvironment env)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _db = db ?? throw new ArgumentNullException(nameof(db));
        _audioProcessingService = audioProcessingService ?? throw new ArgumentNullException(nameof(audioProcessingService));
        _env = env ?? throw new ArgumentNullException(nameof(env));
    }

    [HttpGet]
    public async Task<IActionResult> SearchTitles()
    {
        _cachedLanguages ??= await _db.Languages
                .Where(l => l.Titles.Any())
                .OrderBy(l => l.Name)
                .ToListAsync();

        var model = new SearchTitlesViewModel
        {
            TitleLanguages = _cachedLanguages
        };

        return View(model);
    }
    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AudioFromTitle(IFormCollection form)
    {
        var (formModel, errorResult) = await ValidateAudioFromTitleFormModel(form);
        if (errorResult != null)
        {
            return errorResult;
        }

        if (formModel == null)
        {
            ModelState.AddModelError("", "Invalid form model.");
            return await AudioFromTitleErrorView(formModel!);
        }

        try
        {
            if (!await _audioProcessingService.ValidateAndMarkTokenAsync(formModel.Token!, ModelState))
            {
                return await AudioFromTitleErrorView(formModel);
            }

            var dbTitle = await _db.Titles
                .Include(t => t.Phrases)
                .Include(t => t.OriginalLanguage)
                .SingleOrDefaultAsync(t => t.TitleId == formModel.TitleId);

            if (dbTitle == null)
            {
                ModelState.AddModelError("", "Title not found.");
                return StatusCode(StatusCodes.Status404NotFound, ModelState);
            }
           
            // Process audio
            var (zipFilePath, errors) = await _audioProcessingService.ProcessAudioRequestAsync(
                formModel.ToVoiceId ?? 0,
                formModel.FromVoiceId ?? 0,
                dbTitle,
                formModel.PauseDuration ?? 0,
                formModel.Pattern ?? "");

            if (errors.Count > 0)
            {
                foreach (var error in errors)
                {
                    ModelState.AddModelError("", error);
                }
                return await AudioFromTitleErrorView(formModel);
            }

            if (zipFilePath == null)
            {
                ModelState.AddModelError("", "Failed to create audio files.");
                return await AudioFromTitleErrorView(formModel);
            }

            if (_env.IsDevelopment())
            {
                return PhysicalFile(zipFilePath.FullName, "application/zip", zipFilePath.Name);
            }
            
            // Mark token as used
            if (!await _audioProcessingService.ValidateAndMarkTokenAsync(formModel.Token!, ModelState))
            {
                return StatusCode(StatusCodes.Status503ServiceUnavailable, ModelState);
            }

            return PhysicalFile(zipFilePath.FullName, "application/zip", zipFilePath.Name);

        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "An error occurred while saving changes to the database.");
            ModelState.AddModelError("", "An error occurred while saving changes to the database.");
            return await AudioFromTitleErrorView(formModel!);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            ModelState.AddModelError("", ex.Message);
            return await AudioFromTitleErrorView(formModel!);
        }
    }
    
    private async Task<ViewResult> AudioFromTitleErrorView(AudioFromTitleFormModel formModel)
    {
        var toVoice = await _db.Voices.SingleOrDefaultAsync(v => v.VoiceId == formModel.ToVoiceId);
        var fromVoice = await _db.Voices.SingleOrDefaultAsync(v => v.VoiceId == formModel.FromVoiceId);
        var title = await _db.Titles.SingleOrDefaultAsync(t => t.TitleId == formModel.TitleId);
        if (toVoice == null || fromVoice == null)
        {
            ModelState.AddModelError("", "Invalid voice id.");
        }

        var model = new AudioFromTitleViewModel(
            Title: title,
            ToVoice: toVoice,
            FromVoice: fromVoice,
            Pattern: formModel.Pattern,
            PauseDuration: formModel.PauseDuration,
            HasErrors: true,
            ValidationErrors: ModelState.Values
                .SelectMany(state => state.Errors)
                .Select(error => error.ErrorMessage)
        );
        return View(model);
    }


    private async Task<(AudioFromTitleFormModel? formModel, IActionResult? errorResult)> ValidateAudioFromTitleFormModel(IFormCollection form)
    {
        if (!int.TryParse(form["PauseDuration"], out var pauseDuration))
        {
            ModelState.AddModelError("", "Invalid pause duration.");
        }
        
        if (!int.TryParse(form["ToVoiceId"], out var toVoiceId))
        {
            ModelState.AddModelError("", "Invalid toVoiceId.");
        }
        
        if (!int.TryParse(form["FromVoiceId"], out var fromVoiceId))
        {
            ModelState.AddModelError("", "Invalid fromVoiceId.");
        }
        
        if (!int.TryParse(form["TitleId"], out var titleId))
        {
            ModelState.AddModelError("", "Invalid TitleId.");
        }

        var formModel = new AudioFromTitleFormModel(titleId, toVoiceId, fromVoiceId, pauseDuration, form["Pattern"], form["Token"]);
        if (!TryValidateModel(formModel) || !ModelState.IsValid)
        {
            var errorViewResult = await AudioFromTitleErrorView(formModel);
            return (null, errorViewResult);
        }
        return (formModel, null);
    }

    
    [HttpGet]
    public IActionResult AudioFromTitle(Title title, Voice toVoice, Voice fromVoice, int? pauseDuration, string? pattern)
    {
        var model = new AudioFromTitleViewModel(
            Title: title,
            FromVoice: fromVoice,
            ToVoice: toVoice,
            PauseDuration: pauseDuration,
            Pattern: pattern,
            false, 
            Enumerable.Empty<string>());
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
            .OrderBy(t => t.Popularity)
            .ToListAsync();

        // Use cached languages for the dropdown
        if (_cachedLanguages == null)
        {
            _cachedLanguages = await _db.Languages
                .Where(l => l.Titles.Any())
                .OrderBy(l => l.Name)
                .ToListAsync();
        }
        model.TitleLanguages = _cachedLanguages;

        return View(model);
    }
}