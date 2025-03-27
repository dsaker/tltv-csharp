using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using TalkLikeTv.EntityModels;
using TalkLikeTv.Mvc.Configurations;
using TalkLikeTv.Mvc.Models;
using TalkLikeTv.Services;
using Exception = System.Exception;

namespace TalkLikeTv.Mvc.Controllers;

public class AudioController : Controller
{
    private readonly ILogger<AudioController> _logger;
    private readonly TalkliketvContext _db;
    private readonly TokenService _tokenService;
    private readonly AudioProcessingService _audioProcessingService;
    private readonly AudioFileService _audioFileService;
    private readonly IWebHostEnvironment _env;
    
    public AudioController(
        ILogger<AudioController> logger,
        TalkliketvContext db,
        TokenService tokenService,
        AudioProcessingService audioProcessingService,
        AudioFileService audioFileService,
        IWebHostEnvironment env)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _db = db ?? throw new ArgumentNullException(nameof(db));
        _tokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenService));
        _audioProcessingService = audioProcessingService ?? throw new ArgumentNullException(nameof(audioProcessingService));
        _audioFileService = audioFileService ?? throw new ArgumentNullException(nameof(audioFileService));
        _env = env ?? throw new ArgumentNullException(nameof(env));
    }
    
    [HttpGet]
    public IActionResult CreateTitle(Voice toVoice, Voice fromVoice, int pauseDuration, string pattern)
    {
        var model = new CreateTitleViewModel(
            toVoice,
            fromVoice,
            pattern,
            pauseDuration,
            false, 
            Enumerable.Empty<string>());
        return View("CreateTitle", model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateTitle(IFormCollection form)
    {
        var (formModel, errorResult) = ValidateCreateTitleFormModel(form);
        if (errorResult != null)
        {
            return await errorResult;
        }

        try
        {
            // Validate token and file, get phrases
            if (!_tokenService.CheckTokenStatus(formModel!.Token!))
            {
                ModelState.AddModelError("", "Invalid token.");
                return await CreateTitleErrorView(formModel);
            }

            // Get the result object from ExtractAndValidatePhraseStrings
            var result = _audioFileService.ExtractAndValidatePhraseStrings(formModel!.File!);
            if (result.Errors.Count > 0)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error);
                }
                return await CreateTitleErrorView(formModel);
            }

            var phraseStrings = result.PhraseStrings;
            if (phraseStrings == null || !ModelState.IsValid)
            {
                return await CreateTitleErrorView(formModel);
            }

            // Detect language
            var detectedLanguage = await _audioProcessingService.DetectLanguageAsync(phraseStrings, ModelState);
            if (detectedLanguage == null)
            {
                return StatusCode(StatusCodes.Status503ServiceUnavailable, ModelState);
            }

            // Process title and create DB objects
            var newTitle = await _audioProcessingService.ProcessTitleAsync(
                formModel!.TitleName!, formModel.Description, phraseStrings, detectedLanguage);

            // Process audio
            var (zipFilePath, errors) = await _audioProcessingService.ProcessAudioRequestAsync(
                formModel.ToVoiceId ?? -99,
                formModel.FromVoiceId ?? -99,
                newTitle,
                formModel.PauseDuration ?? -99,
                formModel.Pattern ?? "");

            if (errors.Count > 0)
            {
                foreach (var error in errors)
                {
                    ModelState.AddModelError("", error);
                }
                return StatusCode(StatusCodes.Status503ServiceUnavailable, ModelState);
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
            return await CreateTitleErrorView(formModel);
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "An error occurred while saving changes to the database.");
            ModelState.AddModelError("", "An error occurred while saving changes to the database.");
            return await CreateTitleErrorView(formModel!);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            ModelState.AddModelError("", ex.Message);
            return await CreateTitleErrorView(formModel!);
        }
    }
    
    private async Task<ViewResult> CreateTitleErrorView(CreateTitleFormModel formModel)
    {
        var toVoice = await _db.Voices.SingleOrDefaultAsync(v => v.VoiceId == formModel.ToVoiceId);
        var fromVoice = await _db.Voices.SingleOrDefaultAsync(v => v.VoiceId == formModel.FromVoiceId);
        
        if (toVoice == null || fromVoice == null)
        {
            ModelState.AddModelError("", "Invalid voice id.");
        }
        
        if(formModel.PauseDuration == null)
        {
            ModelState.AddModelError("", "Invalid pause duration.");
        }

        var model = new CreateTitleViewModel(
            ToVoice: toVoice!,
            FromVoice: fromVoice!,
            Pattern: formModel.Pattern!,
            PauseDuration: formModel.PauseDuration,
            HasErrors: true,
            ValidationErrors: ModelState.Values
                .SelectMany(state => state.Errors)
                .Select(error => error.ErrorMessage)
        );
        return View(model);
    }
    
    private (CreateTitleFormModel? formModel, Task<ViewResult>? errorResult) ValidateCreateTitleFormModel(IFormCollection form)
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

        var formModel = new CreateTitleFormModel(toVoiceId, fromVoiceId, pauseDuration, form["Pattern"], form["Token"], form["TitleName"], form["Description"], form.Files["File"]);
        if (!TryValidateModel(formModel) || !ModelState.IsValid)
        {
            return (null, CreateTitleErrorView(formModel));
        }

        // Check if TitleName is unique
        if (_db.Titles.Any(t => t.TitleName == formModel.TitleName))
        {
            ModelState.AddModelError("", "Title name must be unique.");
            return (null, CreateTitleErrorView(formModel));
        }
        
        return (formModel, null);
    }
    
    [HttpGet]
    public IActionResult ChooseAudio(int? titleId)
    {
        var model = new ChooseAudioViewModel(
            _db.Languages.OrderBy(l => l.Name).ThenBy(l => l.NativeName),
            new ChooseAudioFormModel(titleId, null, null, null, null),
            HasErrors: false,
            ValidationErrors: []
        );
        return View(model);
    }
    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChooseAudio(ChooseAudioFormModel modelIn)
    {
        if (modelIn.TitleId != null)
        {
            return RedirectToAction("AudioFromTitle", "Titles", new { titleId = modelIn.TitleId, toVoiceId = modelIn.ToVoice, fromVoiceId = modelIn.FromVoice, pauseDuration = modelIn.PauseDuration, pattern = modelIn.Pattern });
        }
        
        if (ModelState.IsValid)
        {
            var dbToVoice = await _db.Voices
                .Include(v => v.Personalities)
                .Include(v => v.Styles)
                .Include(v => v.Scenarios)
                .SingleOrDefaultAsync(v => v.VoiceId == modelIn.ToVoice);
            var dbFromVoice = await _db.Voices
                .Include(v => v.Personalities)
                .Include(v => v.Styles)
                .Include(v => v.Scenarios)
                .SingleOrDefaultAsync(v => v.VoiceId == modelIn.FromVoice);
        
            if (dbToVoice == null || dbFromVoice == null)
            {
                ModelState.AddModelError("", "Invalid voice selection.");
                var errorModel = new ChooseAudioViewModel(
                    _db.Languages.OrderBy(l => l.Name).ThenBy(l => l.NativeName),
                    modelIn,
                   !ModelState.IsValid,
                    ModelState.Values
                        .SelectMany(state => state.Errors)
                        .Select(error => error.ErrorMessage)
                );
                return View(errorModel);
            }
            
            return CreateTitle(dbToVoice, dbFromVoice, modelIn.PauseDuration ?? 0, modelIn.Pattern ?? "");
        }

        var model = new ChooseAudioViewModel(
            _db.Languages.OrderBy(l => l.Name).ThenBy(l => l.NativeName),
            modelIn,
            !ModelState.IsValid,
            ModelState.Values
                .SelectMany(state => state.Errors)
                .Select(error => error.ErrorMessage)
        );
        
        return View(model);
    }
    
    public class GetVoicesRequest
    {
        public int SelectedLanguage { get; set; }
        public bool IsFromLanguage { get; set; }
    }
    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult GetVoices([FromBody] GetVoicesRequest request)
    {
        var dbVoices = _db.Voices
            .Include(v => v.Styles)
            .Include(v => v.Scenarios)
            .Include(v => v.Personalities)
            .Where(v => v.LanguageId == request.SelectedLanguage)
            .OrderBy(v => v.DisplayName)
            .AsSplitQuery(); // Configure QuerySplittingBehavior to SplitQuery;

        var voiceData = dbVoices
            .Select(v => new {
                v.VoiceId,
                v.DisplayName,
                v.LocaleName,
                v.ShortName,
                v.Gender,
                v.VoiceType,
                Styles = v.Styles.Select(s => s.StyleName).ToList(),
                Scenarios = v.Scenarios.Select(s => s.ScenarioName).ToList(),
                Personalities = v.Personalities.Select(p => p.PersonalityName).ToList()
            })
            .ToList();

        var modelVoices = voiceData
            .Select(v => new VoiceViewModel(
                v.VoiceId,
                v.DisplayName,
                v.LocaleName,
                v.ShortName,
                string.Join("<br>", new List<string?> {
                    "Gender:&nbsp;" + v.Gender,
                    "Type:&nbsp;" + v.VoiceType,
                    v.Styles.Count > 0 ? "Styles:&nbsp;" + string.Join(",&nbsp;", v.Styles) : null,
                    v.Scenarios.Count > 0 ? "Scenarios:&nbsp;" + string.Join(",&nbsp;", v.Scenarios) : null,
                    v.Personalities.Count > 0 ? "Personalities:&nbsp;" + string.Join(",&nbsp;", v.Personalities) : null
                }.Where(s => !string.IsNullOrEmpty(s)))
            ))
            .ToList();

        var partialViewName = request.IsFromLanguage ? "_FromVoiceSelection" : "_ToVoiceSelection";
        return PartialView(partialViewName, modelVoices);
    }
}