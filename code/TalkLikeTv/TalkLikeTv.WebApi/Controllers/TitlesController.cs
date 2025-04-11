// To use [Route], [ApiController], ControllerBase and so on.
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TalkLikeTv.EntityModels; // To use Title.
using TalkLikeTv.Repositories;
using TalkLikeTv.Services;
using TalkLikeTv.Services.Abstractions; // To use ITitleRepository.
using TalkLikeTv.WebApi.Models;

namespace TalkLikeTv.WebApi.Controllers;

// Base address: api/titles
[Route("api/[controller]")]
[ApiController]
public class TitlesController : ControllerBase
{
    private readonly ITitleRepository _repo;
    private readonly IAudioFileService _audioFileService;
    private readonly IAudioProcessingService _audioProcessingService;
    private readonly ITokenService _tokenService;
    private readonly ILogger<AudioController> _logger;
    private readonly IWebHostEnvironment _env;

    // Constructor injects repository registered in Program.cs.
    public TitlesController(
        ITitleRepository repo,
        IAudioFileService audioFileService,
        IAudioProcessingService audioProcessingService,
        ITokenService tokenService,
        ILogger<AudioController> logger)
    
    {
        _repo = repo;
        _audioFileService = audioFileService;
        _audioProcessingService = audioProcessingService;
        _tokenService = tokenService;
        _logger = logger;
    }

    // GET: api/titles
    // GET: api/titles/?originallanguageid=[originallanguageid]
    // this will always return a list of titles (but it might be empty)
    [HttpGet]
    [ProducesResponseType(200, Type = typeof(IEnumerable<Title>))]
    [ProducesResponseType(400)]
    public async Task<ActionResult<IEnumerable<Title>>> GetTitles(string? originallanguageid)
    {
        if (string.IsNullOrWhiteSpace(originallanguageid))
        {
            return await _repo.RetrieveAllAsync(HttpContext.RequestAborted);
        }

        if (!int.TryParse(originallanguageid, out var originalId))
        {
            // Return BadRequest for invalid input instead of empty collection
            return BadRequest($"Invalid originalLanguageId format: {originallanguageid}");
        }

        return (await _repo.RetrieveAllAsync(HttpContext.RequestAborted))
            .Where(title => title.OriginalLanguageId == originalId)
            .ToArray();
    }

    // GET: api/titles/search?languageId=1&keyword=test&searchType=Both&pageNumber=1&pageSize=10
    [HttpGet("search")]
    [ProducesResponseType(200, Type = typeof(PaginatedResult<Title>))]
    public async Task<IActionResult> SearchTitles(
        [FromQuery] string? languageId,
        [FromQuery] string? keyword,
        [FromQuery] string searchType = "Both",
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        if (pageNumber < 1) pageNumber = 1;
        if (pageSize < 1) pageSize = 10;
        if (pageSize > 100) pageSize = 100; // Limit max page size

        var (titles, totalCount) = await _repo.SearchTitlesAsync(
            languageId,
            keyword,
            searchType,
            pageNumber,
            pageSize,
            HttpContext.RequestAborted);

        var result = new PaginatedResult<Title>
        {
            Items = titles,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        return Ok(result);
    }

    // GET: api/titles/[id]
    [HttpGet("{id}", Name = nameof(GetTitle))] // Named route.
    [ProducesResponseType(200, Type = typeof(Title))]
    [ProducesResponseType(404)]
    [ResponseCache(Duration = 5, // Cache-Control: max-age=5
    Location = ResponseCacheLocation.Any, // Cache-Control: public
    VaryByHeader = "User-Agent" // Vary: User-Agent
    )]
    public async Task<IActionResult> GetTitle(string id)
    {
        var title = await _repo.RetrieveAsync(id, HttpContext.RequestAborted);
        if (title == null)
        {
            return NotFound(); // 404 Resource not found.
        }
        return Ok(title); // 200 OK with title in body.
    }

    [HttpPost("fromFile")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Title))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateTitleFromFile([FromForm] CreateTitleFromFileApiModel model)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var tokenResult = await _tokenService.CheckTokenStatus(model.Token, HttpContext.RequestAborted);
            if (!tokenResult.Success)
            {
                return BadRequest(new { error = tokenResult.ErrorMessage ?? "Invalid token." });
            }

            var result = _audioFileService.ExtractAndValidatePhraseStrings(model.File);
            if (result.Errors.Any())
            {
                return BadRequest(new { errors = result.Errors });
            }

            var phraseStrings = result.PhraseStrings;
            if (phraseStrings == null)
            {
                return BadRequest(new { error = "Failed to extract phrase strings from the file." });
            }

            var (detectedLang, detectionErrors) = await _audioProcessingService.DetectLanguageAsync(phraseStrings, HttpContext.RequestAborted);
            if (detectedLang == null || detectionErrors.Any())
            {
                var errorDetails = detectionErrors.Any() ? string.Join("; ", detectionErrors) : "Language detection failed.";
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = errorDetails });
            }

            var newTitle = await _audioProcessingService.ProcessTitleAsync(model.TitleName, model.Description, phraseStrings, detectedLang, HttpContext.RequestAborted);

            // Mark the token as used
            var (markSuccess, markErrors) = await _audioProcessingService.MarkTokenAsUsedAsync(model.Token);
            if (!markSuccess)
            {
                return BadRequest(new { errors = markErrors });
            }

            // Return the created title
            return Ok(newTitle);
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database error while processing title {TitleName}", model.TitleName);
            return StatusCode(StatusCodes.Status500InternalServerError, new { error = "An error occurred while saving changes to the database." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing title {TitleName}: {ErrorMessage}", model.TitleName, ex.Message);
            return StatusCode(StatusCodes.Status500InternalServerError, new { error = ex.Message });
        }
    }

    [HttpPut("{id}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Update(string id, [FromBody] Title t)
    {
        if (t.TitleId.ToString() != id) // Compare as string
        {
            return BadRequest(); // 400 Bad request.
        }
        
        var existing = await _repo.RetrieveAsync(id, HttpContext.RequestAborted);
        if (existing == null)
        {
            return NotFound(); // 404 Resource not found.
        }
        
        await _repo.UpdateAsync(id, t, HttpContext.RequestAborted);
        return new NoContentResult(); // 204 No content.
    }

    // DELETE: api/titles/[id]
    [HttpDelete("{id}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Delete(string id)
    {
        var existing = await _repo.RetrieveAsync(id, HttpContext.RequestAborted);
        if (existing == null)
        {
            return NotFound(); // 404 Resource not found.
        }
        
        bool? deleted = await _repo.DeleteAsync(id, HttpContext.RequestAborted);
        if (deleted.Value) // Short circuit AND.
        {
            return new NoContentResult(); // 204 No content.
        }
        
        return BadRequest( // 400 Bad request.
            $"Title {id} was found but failed to delete.");
    }
}