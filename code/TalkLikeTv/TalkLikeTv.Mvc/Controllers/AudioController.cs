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
        var formModel = new CreateTitleFormModel(toVoice, fromVoice, pauseDuration, pattern, null, null, null,null);
        var model = new CreateTitleViewModel(
            _db.Languages.OrderBy(l => l.Name).ThenBy(l => l.NativeName),
            formModel, 
            false, 
            Enumerable.Empty<string>());
        return View("CreateTitle", model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateTitle(IFormCollection form)
    {
        if (!int.TryParse(form["PauseDuration"], out var pauseDuration))
        {
            ModelState.AddModelError("", "Invalid pause duration.");
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
            return CreateTitleErrorView(formModel);
        }

        // Check if TitleName is unique
        if (_db.Titles.Any(t => t.TitleName == formModel.TitleName))
        {
            ModelState.AddModelError("", "Title name must be unique.");
            return CreateTitleErrorView(formModel);
        }
        
        try
        {
            var phraseStrings = ValidateTokenAndFile(formModel);
            if (phraseStrings == null || !ModelState.IsValid)
            {
                return CreateTitleErrorView(formModel);
            }
            
            var translator = new AzureTranslateService();
            var detectedCode = await translator.DetectLanguageFromPhrasesAsync(phraseStrings);

            var languageEntity = await _db.Languages
                .SingleOrDefaultAsync(l => l.Tag == detectedCode);

            if (languageEntity == null)
            {
                throw new Exception($"Language '{detectedCode}' not found.");
            }

            var languageId = languageEntity.LanguageId;            
            var newTitle = new Title
            {
                TitleName = formModel.TitleName!,
                Description = formModel.Description,
                NumPhrases = phraseStrings.Count,
                OriginalLanguageId = languageId,
            };

            _db.Titles.Add(newTitle);
            await _db.SaveChangesAsync();

            foreach (var phraseString in phraseStrings)
            {
                var phrase = new Phrase
                {
                    TitleId = newTitle.TitleId,
                };
                _db.Phrases.Add(phrase);
            }
            await _db.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            ModelState.AddModelError("", ex.Message);
            // TODO change this to Error view
            return CreateTitleErrorView(formModel);
        }
        
        return RedirectToAction("Index", "Home");    
    }
    
    private ViewResult CreateTitleErrorView(CreateTitleFormModel formModel)
    {
        var model = new CreateTitleViewModel(
            _db.Languages.OrderBy(l => l.Name).ThenBy(l => l.NativeName),
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
        if (phraseStrings == null || !ModelState.IsValid)
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
    
    private List<string>? GetPhraseStrings(IFormFile file)
    {
        ArgumentNullException.ThrowIfNull(file, nameof(file));
        
        var fileStream = file.OpenReadStream();
        if (fileStream == null)
        {
            throw new Exception("Invalid file stream.");
        }
        
        // Check if the content is single line
        if (TextFormatDetector.DetectTextFormat(fileStream) != TextFormatDetector.TextFormat.OnePhrasePerLine)
        {
            ModelState.AddModelError("", "Invalid file format. Please parse the file at the home page.");
            return null;
        }
        
        fileStream.Seek(0, SeekOrigin.Begin);
        using var reader = new StreamReader(fileStream);
        var parsedPhrases = Parse.ParseOnePhrasePerLine(reader);
        
        if (parsedPhrases.Count > 0)
        {
            return parsedPhrases;
        }

        ModelState.AddModelError("", "No phrases found in the file.");
        return null;

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