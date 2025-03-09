using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using TalkLikeTv.Mvc.Models;
using TalkLikeTv.Mvc.Configurations;
using TalkLikeTv.Services;

namespace TalkLikeTv.Mvc.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly SharedSettings _sharedSettings;

    public HomeController(ILogger<HomeController> logger, IOptions<SharedSettings> sharedSettings)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _sharedSettings = sharedSettings.Value ?? throw new ArgumentNullException(nameof(sharedSettings));
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
                //using var fileStream = model.File.OpenReadStream() as FileStream;
                await using (var fileStream = model.File.OpenReadStream())
                {
                    var fileInfo = Parse.ParseFile(fileStream, model.File.FileName, _sharedSettings.MaxPhrases);

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