using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
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
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads", model.File.FileName);
            
            await using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await model.File.CopyToAsync(stream);
            }
            
            // Reopen the file and pass the stream to another function
            await using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                Parse.ParseFile(fileStream);
            }

            ViewBag.Message = "File uploaded successfully.";
        }
        else
        {
            ViewBag.Message = "No file selected.";
        }

        return View("Index");
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