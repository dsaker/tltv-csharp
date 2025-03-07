using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TalkLikeTv.EntityModels;
using TalkLikeTv.Mvc.Models;
using TalkLikeTv.FileService;

namespace TalkLikeTv.Mvc.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly TalkliketvContext _db;

    public HomeController(ILogger<HomeController> logger, TalkliketvContext db)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _db = db ?? throw new ArgumentNullException(nameof(db));
    }

    public IActionResult Index()
    {
        return View(new HomeIndexViewModel());
    }
 
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(HomeIndexViewModel model)
    {
        if (ModelState.IsValid && model.File?.Length > 0)
        {
            try
            {
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads", model.File.FileName);

                await using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await model.File.CopyToAsync(stream);
                }

                // Reopen the file and pass the stream to another function
                await using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                {
                    var fileInfo = Parse.ParseFile(fileStream);

                    // Read the file content
                    var fileBytes = await System.IO.File.ReadAllBytesAsync(fileInfo.FullName);
                    var fileName = Path.GetFileName(fileInfo.FullName);

                    // Return the file as a response
                    return File(fileBytes, "application/zip", fileName);
                }
            }
            catch (Exception ex)
            {
                return BadRequest($"Something went wrong: {ex.Message}");
            }
        }

        ViewBag.Message = "No file selected.";

        return View("Index");
    }
    
    // public IActionResult CreateTitle(Voice toVoice, Voice fromVoice, int? pauseDuration, string? pattern)
    // {
    //     var formModel = new CreateTitleFormModel(
    //         toVoice,
    //         fromVoice,
    //         pauseDuration,
    //        pattern,
    //         null,
    //         null,
    //         null,
    //         null);
    //     CreateTitleViewModel model = new(
    //         formModel,
    //         HasErrors: false,
    //         ValidationErrors: []);
    //     return View("CreateTitle", model);
    // }
    //
    // [HttpPost]
    // [ValidateAntiForgeryToken]
    // public IActionResult CreateTitle(IFormCollection form)
    // {
    //     var toVoiceJson = form["ToVoice"];
    //     var fromVoiceJson = form["FromVoice"];
    //     var pattern = form["Pattern"];
    //     var pauseDuration = int.Parse(form["PauseDuration"]);
    //     var token = form["Token"];
    //     var titleName = form["TitleName"];
    //     var description = form["Description"];
    //     var file = form.Files["File"];
    //
    //     var options = new JsonSerializerOptions
    //     {
    //         PropertyNameCaseInsensitive = true,
    //         ReferenceHandler = ReferenceHandler.Preserve
    //     };
    //
    //     var toVoice = JsonSerializer.Deserialize<Voice>(toVoiceJson, options);
    //     var fromVoice = JsonSerializer.Deserialize<Voice>(fromVoiceJson, options);
    //     var formModel = new CreateTitleFormModel(
    //         toVoice,
    //         fromVoice,
    //         pauseDuration,
    //         pattern,
    //         token,
    //         titleName,
    //         description,
    //         file
    //     );
    //     
    //     if (!TryValidateModel(formModel))
    //     {
    //         CreateTitleViewModel model = new(
    //             CreateTitleFormModel: formModel,
    //             HasErrors: true,
    //             ValidationErrors: ModelState.Values
    //                 .SelectMany(state => state.Errors)
    //                 .Select(error => error.ErrorMessage)
    //         );
    //
    //         return View(model);
    //     }
    //     
    //
    //     WriteLine("The model is valid. Continue processing the file.");
    //     return View("Index");
    // }
  
    // [HttpGet]
    // public IActionResult CreateAudio()
    // {
    //     var model = new CreateAudioViewModel(
    //         _db.Languages.OrderBy(l => l.Name).ThenBy(l => l.NativeName),
    //         new CreateAudioFormModel(null, null, null, null),
    //         HasErrors: false,
    //         ValidationErrors: []
    //     );
    //     return View(model);
    // }
    //
    // [HttpPost]
    // [ValidateAntiForgeryToken]
    // public async Task<IActionResult> CreateAudio(CreateAudioFormModel modelIn)
    // {
    //     if (ModelState.IsValid)
    //     {
    //         var dbToVoice = await _db.Voices
    //             .Include(v => v.Personalities)
    //             .Include(v => v.Styles)
    //             .Include(v => v.Scenarios)
    //             .SingleOrDefaultAsync(v => v.VoiceId == modelIn.ToVoice);
    //         var dbFromVoice = await _db.Voices
    //             .Include(v => v.Personalities)
    //             .Include(v => v.Styles)
    //             .Include(v => v.Scenarios)
    //             .SingleOrDefaultAsync(v => v.VoiceId == modelIn.FromVoice);
    //     
    //         if (dbToVoice == null || dbFromVoice == null)
    //         {
    //             ModelState.AddModelError("", "Invalid voice selection.");
    //             var errorModel = new CreateAudioViewModel(
    //                 _db.Languages.OrderBy(l => l.Name).ThenBy(l => l.NativeName),
    //                 modelIn,
    //                !ModelState.IsValid,
    //                 ModelState.Values
    //                     .SelectMany(state => state.Errors)
    //                     .Select(error => error.ErrorMessage)
    //             );
    //             return View(errorModel);
    //         }
    //         
    //         return RedirectToAction("CreateTitle", "Audio", new 
    //         {
    //             toVoice = dbToVoice,
    //             fromVoice = dbFromVoice,
    //             pauseDuration = modelIn.PauseDuration,
    //             pattern = modelIn.Pattern
    //         });
    //         
    //         // return CreateTitle(dbToVoice, dbFromVoice, modelIn.PauseDuration, modelIn.Pattern);
    //     }
    //
    //     var model = new CreateAudioViewModel(
    //         _db.Languages.OrderBy(l => l.Name).ThenBy(l => l.NativeName),
    //         modelIn,
    //         !ModelState.IsValid,
    //         ModelState.Values
    //             .SelectMany(state => state.Errors)
    //             .Select(error => error.ErrorMessage)
    //     );
    //     
    //     return View(model);
    // }
    //
    // public class GetVoicesRequest
    // {
    //     public int SelectedLanguage { get; set; }
    //     public bool IsFromLanguage { get; set; }
    // }
    //
    // [HttpPost]
    // [ValidateAntiForgeryToken]
    // public IActionResult GetVoices([FromBody] GetVoicesRequest request)
    // {
    //     var dbVoices = _db.Voices
    //         .Include(v => v.Styles)
    //         .Include(v => v.Scenarios)
    //         .Include(v => v.Personalities)
    //         .Where(v => v.LanguageId == request.SelectedLanguage)
    //         .OrderBy(v => v.DisplayName);
    //
    //     var modelVoices = new List<VoiceViewModel>();
    //     foreach (var v in dbVoices)
    //     {
    //         var vStyles = "";
    //         if (v.Styles.Count > 0)
    //         {
    //             vStyles = "Styles:&nbsp;" + string.Join(",&nbsp;", v.Styles.Select(s => s.StyleName)) + "<br>";
    //         }
    //         var vScenarios = "";
    //         if (v.Scenarios.Count > 0)
    //         {
    //             vScenarios = "Scenarios:&nbsp;" + string.Join(",&nbsp;", v.Scenarios.Select(s => s.ScenarioName)) + "<br>";
    //         }
    //         var vPersonalities = "";
    //         if (v.Personalities.Count > 0)
    //         {
    //             vPersonalities = "Personalities:&nbsp;" + string.Join(",&nbsp;", v.Personalities.Select(p => p.PersonalityName));
    //         }
    //         var vDetails = $"Gender:&nbsp;{v.Gender}<br>Type:&nbsp;{v.VoiceType}<br>{vStyles}{vScenarios}{vPersonalities}";
    //         modelVoices.Add(new VoiceViewModel(v.VoiceId, v.DisplayName, v.LocaleName, v.ShortName, vDetails));
    //     }
    //
    //     var partialViewName = request.IsFromLanguage ? "_FromVoiceSelection" : "_ToVoiceSelection";
    //     return PartialView(partialViewName, modelVoices);
    // }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}