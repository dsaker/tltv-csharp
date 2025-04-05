using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TalkLikeTv.EntityModels;
using TalkLikeTv.Mvc.Models;
using TalkLikeTv.Repositories;
using TalkLikeTv.Services;

namespace TalkLikeTv.Mvc.Controllers;

public class TitlesController : Controller
{
    private readonly ILogger<AudioController> _logger;
    private readonly AudioProcessingService _audioProcessingService;
    private readonly TokenService _tokenService;
    private readonly ILanguageRepository _languageRepository;
    private readonly ITitleRepository _titleRepository;
    private readonly IVoiceRepository _voiceRepository;
    private readonly IWebHostEnvironment _env;

    public TitlesController(
        ILogger<AudioController> logger, 
        AudioProcessingService audioProcessingService,
        TokenService tokenService,
        ILanguageRepository languageRepository,
        ITitleRepository titleRepository,
        IVoiceRepository voiceRepository,
        IWebHostEnvironment env)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _audioProcessingService = audioProcessingService;
        _tokenService = tokenService;
        _languageRepository = languageRepository;
        _titleRepository = titleRepository;
        _voiceRepository = voiceRepository;
        _env = env ?? throw new ArgumentNullException(nameof(env));
    }

    [HttpGet]
    public async Task<IActionResult> SearchTitles()
    {
        var languages = await _languageRepository.RetrieveAllAsync();

        var model = new SearchTitlesViewModel
        {
            TitleLanguages = languages,
        };

        return View(model);
    }
    
    [HttpPost]
    public async Task<IActionResult> SearchTitles(SearchTitlesViewModel model)
    {
        const int pageSize = 10;
    
        // Get search results with paging
        var (titles, totalCount) = await _titleRepository.SearchTitlesAsync(
            model.OriginalLanguageId,
            model.Keyword,
            model.SearchType,
            model.PageNumber,
            pageSize,
            HttpContext.RequestAborted);
    
        // Calculate total pages
        model.TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
        model.Results = titles;
    
        // Get languages for the dropdown
        model.TitleLanguages = await _languageRepository.RetrieveAllAsync();
    
        return View(model);
    }
    
    [HttpGet]
    public async Task<IActionResult> AudioFromTitle(int titleId, int toVoiceId, int fromVoiceId, int pauseDuration, string pattern)
    {
        var title = await _titleRepository.RetrieveAsync(titleId.ToString(), HttpContext.RequestAborted);
        var toVoice = await _voiceRepository.RetrieveAsync(toVoiceId.ToString(), HttpContext.RequestAborted);
        var fromVoice = await _voiceRepository.RetrieveAsync(fromVoiceId.ToString(), HttpContext.RequestAborted);
        
        if (title == null || toVoice == null || fromVoice == null)
        {
            ModelState.AddModelError("", "Invalid titleId or voice in AudioFromTitle.");
            var formModel = new AudioFromTitleFormModel(titleId, toVoiceId, fromVoiceId, pauseDuration, pattern, "");
            return await AudioFromTitleErrorView(formModel);
        }
        
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
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AudioFromTitle(IFormCollection form)
    {
        var (formModel, errorResult) = await ValidateAudioFromTitleFormModel(form);
        if (errorResult != null)
        {
            return errorResult;
        }

        try
        {
            var validToken = await _tokenService.CheckTokenStatus(formModel!.Token!);
            if (!validToken)
            {
                ModelState.AddModelError(string.Empty, "Invalid token");
                return await AudioFromTitleErrorView(formModel);
            }

            var dbTitle = await _titleRepository.RetrieveAsync(formModel.TitleId.ToString()!, HttpContext.RequestAborted);

            if (dbTitle == null)
            {
                ModelState.AddModelError("", "Title not found.");
                return await AudioFromTitleErrorView(formModel);
            }

            // Process audio
            var (zipFilePath, errors) = await _audioProcessingService.ProcessAudioRequestAsync(
                formModel.ToVoiceId ?? -99,
                formModel.FromVoiceId ?? -99,
                dbTitle,
                formModel.PauseDuration ?? -99,
                formModel.Pattern ?? "");

            if (errors.Any())
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
            var (markSuccess, markErrors) = await _audioProcessingService.MarkTokenAsUsedAsync(formModel.Token!);
            if (markSuccess)
            {
                return PhysicalFile(zipFilePath.FullName, "application/zip", zipFilePath.Name);
            }

            foreach (var error in markErrors)
            {
                ModelState.AddModelError("", error);
            }
            return await AudioFromTitleErrorView(formModel);
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
        Voice? toVoice = null;
        if (formModel.ToVoiceId.HasValue)
        {
            toVoice = await _voiceRepository.RetrieveAsync(
                formModel.ToVoiceId.Value.ToString(),
                HttpContext.RequestAborted);
        }
    
        Voice? fromVoice = null;
        if (formModel.FromVoiceId.HasValue)
        {
            fromVoice = await _voiceRepository.RetrieveAsync(
                formModel.FromVoiceId.Value.ToString(),
                HttpContext.RequestAborted);
        }
    
        Title? title = null;
        if (formModel.TitleId.HasValue)
        {
            title = await _titleRepository.RetrieveAsync(
                formModel.TitleId.Value.ToString(),
                HttpContext.RequestAborted);
        }
    
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
}