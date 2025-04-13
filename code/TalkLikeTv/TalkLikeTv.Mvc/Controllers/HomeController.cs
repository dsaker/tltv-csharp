using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using TalkLikeTv.Mvc.Models;
using TalkLikeTv.Services.Abstractions;

namespace TalkLikeTv.Mvc.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly TalkliketvOptions _options;
    private readonly IParseService _parseService;

    public HomeController(
        ILogger<HomeController> logger, 
        IParseService parseService,
        IOptions<TalkliketvOptions> sharedSettings)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _parseService = parseService;
        _options = sharedSettings.Value ?? throw new ArgumentNullException(nameof(sharedSettings));
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
                await using (var fileStream = model.File.OpenReadStream())
                {
                    var parseResult = _parseService.ParseFile(fileStream, model.File.FileName, _options.MaxPhrases);

                    if (!parseResult.Success)
                    {
                        ViewBag.Error = parseResult.ErrorMessage;
                        return View("Index");
                    }

                    if (parseResult.File == null)
                    {
                        ViewBag.Error = "File not found.";
                        return View("Index");
                    }

                    var fileBytes = await System.IO.File.ReadAllBytesAsync(parseResult.File.FullName);
                    var fileName = Path.GetFileName(parseResult.File.FullName);

                    var fileResult = File(fileBytes, "application/zip", fileName + ".zip");
                    
                    // Delete the temporary file after we've read its contents
                    if (parseResult.File != null && System.IO.File.Exists(parseResult.File.FullName))
                    {
                        try
                        {
                            System.IO.File.Delete(parseResult.File.FullName);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning("Failed to delete temporary file {FileName}: {Exception}", 
                                parseResult.File.FullName, ex.Message);
                        }
                    }

                    return fileResult;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Error processing file {FileName}: {Exception}", model.File?.FileName, ex);
                ViewBag.Error = $"Something went wrong: {ex.Message}";
                return View("Index");
            }
        }

        _logger.LogWarning("Index post action called with no file or invalid model state");
        ViewBag.Error = "No file selected.";

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