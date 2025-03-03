using System.Diagnostics;
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
        var model = new HomeIndexViewModel
        {
            VisitorCount = Random.Shared.Next(1, 1001),
        };
        return View(model);
    }
    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Upload(HomeIndexViewModel model)
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
    
    
    public IActionResult CreateAudio()
    {
        var dbVoices = _db.Voices
            .Include(v => v.Styles)
            .Include(v => v.Scenarios)
            .Include(v => v.Personalities)
            .OrderBy(v => v.DisplayName);

        var modelVoices = new List<VoiceViewModel>();
        foreach (var v in dbVoices)
        {
            var vStyles = "";
            if (v.Styles.Count > 0)
            {
                vStyles = "Styles:&nbsp;" + string.Join(",&nbsp;", v.Styles.Select(s => s.StyleName)) + "<br>";
            }
            var vScenarios = "";
            if (v.Scenarios.Count > 0)
            {
                vScenarios = "Scenarios:&nbsp;" + string.Join(",&nbsp;", v.Scenarios.Select(s => s.ScenarioName)) + "<br>";
            }
            var vPersonalities = "";
            if (v.Personalities.Count > 0)
            {
                vPersonalities = "Personalities:&nbsp;" + string.Join(",&nbsp;", v.Personalities.Select(p => p.PersonalityName)) + "<br>";
            }
            var vDetails = $"Gender:&nbsp;{v.Gender}<br>Type:&nbsp;{v.VoiceType}{vStyles}{vScenarios}{vPersonalities}";
            modelVoices.Add(new VoiceViewModel(v.VoiceId, v.DisplayName, v.LocaleName, v.ShortName, vDetails));
        }
        
        CreateAudioInputsModel model = new(
            _db.Languages
                .OrderBy(l => l.Name)
                .ThenBy(l => l.NativeName),
            modelVoices);
        var viewModel = new CreateAudioViewModel
        {
            CreateAudioInputsModel = model
        };
        return View(viewModel);
    }
    

    // public IActionResult LanguageDetail(int? id)
    // {
    //     if (!id.HasValue)
    //     {
    //         return BadRequest("You must pass a language ID in the route, for example, /Home/LanguageDetail/21");
    //     }
    //     var model = _db.Languages.SingleOrDefault(p => p.LanguageId == id);
    //     if (model is null)
    //     {
    //         return NotFound($"Language {id} not found.");
    //     }
    //     return View(model); // Pass model to view and then return result.
    // }
    
    // public IActionResult VoiceDetail(int? id)
    // {
    //     if (!id.HasValue)
    //     {
    //         return BadRequest("You must pass a voice ID in the route, fo rexample, /Home/VoiceDetail/21");
    //     }
    //     var model = _db.Voices.Include(v => v.Language)
    //         .Include(v => v.Styles)
    //         .Include(v => v.Scenarios)
    //         .Include(v => v.Personalities)
    //         .SingleOrDefault(p => p.VoiceId == id);
    //     if (model is null)
    //     {
    //         return NotFound($"Voice {id} not found.");
    //     }
    //     return View(model); // Pass model to view and then return result.
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