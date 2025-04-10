using Microsoft.AspNetCore.Mvc;
using TalkLikeTv.EntityModels;
using TalkLikeTv.Repositories;

namespace TalkLikeTv.WebApi.Controllers;

// Base address: api/languages
[Route("api/[controller]")]
[ApiController]
public class LanguagesController : ControllerBase
{
    private readonly ILanguageRepository _repo;
    private readonly ILogger<LanguagesController> _logger;

    // Constructor injects repository and logger
    public LanguagesController(ILanguageRepository repo, ILogger<LanguagesController> logger)
    {
        _repo = repo;
        _logger = logger;
    }

    // GET: api/languages
    [HttpGet]
    [ProducesResponseType(200, Type = typeof(IEnumerable<Language>))]
    [ProducesResponseType(500)]
    public async Task<ActionResult<IEnumerable<Language>>> GetLanguages(CancellationToken cancel = default)
    {
        try
        {
            var languages = await _repo.RetrieveAllAsync(cancel);
            return Ok(languages);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving all languages.");
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing your request.");
        }
    }

    // GET: api/languages/{id}
    [HttpGet("{id}", Name = nameof(GetLanguage))]
    [ProducesResponseType(200, Type = typeof(Language))]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> GetLanguage(string id, CancellationToken cancel = default)
    {
        try
        {
            var language = await _repo.RetrieveAsync(id, cancel);
            if (language == null)
            {
                return NotFound(); // 404 Resource not found
            }
            return Ok(language); // 200 OK with language in body
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving language with ID {Id}.", id);
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing your request.");
        }
    }

    // GET: api/languages/tag/{code}
    [HttpGet("tag/{code}")]
    [ProducesResponseType(200, Type = typeof(Language))]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> GetLanguageByTag(string code, CancellationToken cancel = default)
    {
        try
        {
            var language = await _repo.RetrieveByTagAsync(code, cancel);
            if (language == null)
            {
                return NotFound(); // 404 Resource not found
            }
            return Ok(language); // 200 OK with language in body
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving language with tag {Code}.", code);
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing your request.");
        }
    }
}