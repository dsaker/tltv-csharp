using Microsoft.AspNetCore.Mvc;
using TalkLikeTv.EntityModels;
using TalkLikeTv.Repositories;

namespace TalkLikeTv.WebApi.Controllers;

// Base address: api/voices
[Route("api/[controller]")]
[ApiController]
public class VoicesController : ControllerBase
{
    private readonly IVoiceRepository _repo;
    private readonly ILogger<VoicesController> _logger;

    // Constructor injects repository and logger
    public VoicesController(IVoiceRepository repo, ILogger<VoicesController> logger)
    {
        _repo = repo;
        _logger = logger;
    }

    // GET: api/voices
    [HttpGet]
    [ProducesResponseType(200, Type = typeof(IEnumerable<Voice>))]
    [ProducesResponseType(500)]
    public async Task<ActionResult<IEnumerable<Voice>>> GetVoices()
    {
        try
        {
            var voices = await _repo.RetrieveAllAsync(HttpContext.RequestAborted);
            return Ok(voices);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving all voices.");
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing your request.");
        }
    }

    // GET: api/voices/{id}
    [HttpGet("{id}", Name = nameof(GetVoice))]
    [ProducesResponseType(200, Type = typeof(Voice))]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> GetVoice(string id)
    {
        try
        {
            var voice = await _repo.RetrieveAsync(id, HttpContext.RequestAborted);
            if (voice == null)
            {
                return NotFound(); // 404 Resource not found
            }
            return Ok(voice); // 200 OK with voice in body
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving voice with ID {Id}.", id);
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing your request.");
        }
    }
}