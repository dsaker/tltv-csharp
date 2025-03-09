using System.Text.Json;
using System.Text.Json.Serialization;
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
    private readonly SharedSettings _sharedSettings;

    public AudioController(ILogger<AudioController> logger, TalkliketvContext db, TokenService tokenService, IOptions<SharedSettings> sharedSettings)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _db = db ?? throw new ArgumentNullException(nameof(db));
        _tokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenService));
        _sharedSettings = sharedSettings.Value ?? throw new ArgumentNullException(nameof(sharedSettings));
    }
    
    [HttpGet]
    public IActionResult CreateTitle(Voice toVoice, Voice fromVoice, int? pauseDuration, string? pattern)
    {
        var formModel = new CreateTitleFormModel(toVoice, fromVoice, pauseDuration, pattern, null, null, null, null);
        var model = new CreateTitleViewModel(formModel, false, Enumerable.Empty<string>());
        return View("CreateTitle", model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult CreateTitle(IFormCollection form)
    {
        if (!int.TryParse(form["PauseDuration"], out var pauseDuration))
        {
            ModelState.AddModelError("", "Invalid pause duration.");
            return View("Error");
        }

        var toVoice = DeserializeVoice(form["ToVoice"]);
        var fromVoice = DeserializeVoice(form["FromVoice"]);
        if (toVoice == null || fromVoice == null )
        {
            ModelState.AddModelError("", "Invalid voice data.");
        }
        
        var formModel = new CreateTitleFormModel(toVoice, fromVoice, pauseDuration, form["Pattern"], form["Token"], form["TitleName"], form["Description"], form.Files["File"]);
        if (!TryValidateModel(formModel) || !ModelState.IsValid)
        {
            return ReturnErrorView(formModel);
        }
        
        try
        {
            var phraseStrings = ValidateTokenAndFile(formModel);
            if (phraseStrings == null || !ModelState.IsValid)
            {
                return ReturnErrorView(formModel);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            ModelState.AddModelError("", ex.Message);
            return ReturnErrorView(formModel);
        }
        
        return RedirectToAction("Index", "Home");    
    }
    
    private IActionResult ReturnErrorView(CreateTitleFormModel formModel)
    {
        var model = new CreateTitleViewModel(
            CreateTitleFormModel: formModel,
            HasErrors: true,
            ValidationErrors: ModelState.Values
                .SelectMany(state => state.Errors)
                .Select(error => error.ErrorMessage)
        );
        return View(model);
    }
    
    private List<string>? ValidateTokenAndFile(CreateTitleFormModel formModel)
    {
        if (formModel.Token == null || !_tokenService.CheckTokenStatus(formModel.Token))
        {
            ModelState.AddModelError("", "Invalid token.");
            return null;
        }

        if (formModel.File == null)
        {
            ModelState.AddModelError("", "File is invalid");
            return null;
        }

        var phraseStrings = GetPhraseStrings(formModel.File);
        if (!ModelState.IsValid)
        {
            return null;
        }
        
        if (phraseStrings.Count > _sharedSettings.MaxPhrases)
        {
            ModelState.AddModelError("", $"Phrase count exceeds the maximum of {_sharedSettings.MaxPhrases}.");
            return null;
        }

        return phraseStrings;
    }
    
    private Voice? DeserializeVoice(string voiceJson)
    {
        try
        {
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true, ReferenceHandler = ReferenceHandler.Preserve };
            return JsonSerializer.Deserialize<Voice>(voiceJson, options);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to deserialize voice JSON.");
            ModelState.AddModelError("", "Invalid voice data.");
            return null;
        }
    }
    
    private List<string> GetPhraseStrings(IFormFile file)
    {
        ArgumentNullException.ThrowIfNull(file, nameof(file));
        
        var fileStream = file.OpenReadStream();
        if (fileStream == null)
        {
            ModelState.AddModelError("", "Invalid file stream.");
            throw new Exception("Invalid file stream.");
        }
        
        // Check if the content is single line
        if (TextFormatDetector.DetectTextFormat(fileStream) != TextFormatDetector.TextFormat.OnePhrasePerLine)
        {
            ModelState.AddModelError("", "Invalid file format.");
            throw new Exception("Invalid file format. Please parse the file at the home page.");
        }

        using var reader = new StreamReader(fileStream);
        // Split the content into lines and return as a list of strings
        return Parse.ParseOnePhrasePerLine(reader);
    }
    
    [HttpGet]
    public IActionResult CreateAudio()
    {
        var model = new CreateAudioViewModel(
            _db.Languages.OrderBy(l => l.Name).ThenBy(l => l.NativeName),
            new CreateAudioFormModel(null, null, null, null),
            HasErrors: false,
            ValidationErrors: []
        );
        return View(model);
    }
    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateAudio(CreateAudioFormModel modelIn)
    {
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
                var errorModel = new CreateAudioViewModel(
                    _db.Languages.OrderBy(l => l.Name).ThenBy(l => l.NativeName),
                    modelIn,
                   !ModelState.IsValid,
                    ModelState.Values
                        .SelectMany(state => state.Errors)
                        .Select(error => error.ErrorMessage)
                );
                return View(errorModel);
            }
            
            return CreateTitle(dbToVoice, dbFromVoice, modelIn.PauseDuration, modelIn.Pattern);
        }

        var model = new CreateAudioViewModel(
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
            .OrderBy(v => v.DisplayName);

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