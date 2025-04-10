// To use [Route], [ApiController], ControllerBase and so on.
using Microsoft.AspNetCore.Mvc;
using TalkLikeTv.EntityModels; // To use Title.
using TalkLikeTv.Repositories;
using TalkLikeTv.Services; // To use ITitleRepository.
using TalkLikeTv.WebApi.Models;

namespace TalkLikeTv.WebApi.Controllers;

// Base address: api/titles
[Route("api/[controller]")]
[ApiController]
public class TitlesController : ControllerBase
{
    private readonly ITitleRepository _repo;
    private readonly ITitleValidationService _validationService;

    // Constructor injects repository registered in Program.cs.
    public TitlesController(ITitleRepository repo, ITitleValidationService validationService)
    {
        _repo = repo;
        _validationService = validationService;
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
            return await _repo.RetrieveAllAsync();
        }

        if (!int.TryParse(originallanguageid, out var originalId))
        {
            // Return BadRequest for invalid input instead of empty collection
            return BadRequest($"Invalid originalLanguageId format: {originallanguageid}");
        }

        return (await _repo.RetrieveAllAsync())
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
            pageSize);

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
        var title = await _repo.RetrieveAsync(id);
        if (title == null)
        {
            return NotFound(); // 404 Resource not found.
        }
        return Ok(title); // 200 OK with title in body.
    }

    // POST: api/titles
    [HttpPost]
    [ProducesResponseType(201, Type = typeof(Title))]
    [ProducesResponseType(400)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> Create([FromBody] Title t)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
        
            // Use validation service for business rules
            var (isValid, errors) = await _validationService.ValidateAsync(t);
            if (!isValid)
            {
                return BadRequest(errors);
            }
        
            var addedTitle = await _repo.CreateAsync(t);
        
            return CreatedAtRoute(
                routeName: nameof(GetTitle),
                routeValues: new { id = addedTitle.TitleId.ToString() },
                value: addedTitle);
        }
        catch (Exception ex)
        {
            // Log the exception
            return StatusCode(StatusCodes.Status500InternalServerError, 
                "Error creating title: " + ex.Message);
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
        
        var existing = await _repo.RetrieveAsync(id);
        if (existing == null)
        {
            return NotFound(); // 404 Resource not found.
        }
        
        await _repo.UpdateAsync(id, t);
        return new NoContentResult(); // 204 No content.
    }

    // DELETE: api/titles/[id]
    [HttpDelete("{id}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Delete(string id)
    {
        var existing = await _repo.RetrieveAsync(id);
        if (existing == null)
        {
            return NotFound(); // 404 Resource not found.
        }
        
        bool? deleted = await _repo.DeleteAsync(id);
        if (deleted.Value) // Short circuit AND.
        {
            return new NoContentResult(); // 204 No content.
        }
        
        return BadRequest( // 400 Bad request.
            $"Title {id} was found but failed to delete.");
    }
}