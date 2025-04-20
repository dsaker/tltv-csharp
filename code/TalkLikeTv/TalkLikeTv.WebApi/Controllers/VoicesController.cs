using Microsoft.AspNetCore.Mvc;
using TalkLikeTv.EntityModels;
using TalkLikeTv.Repositories;
using TalkLikeTv.WebApi.Mappers;
using TalkLikeTv.WebApi.Models;

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
    [ProducesResponseType(200, Type = typeof(IEnumerable<VoiceMapper.VoiceResponse>))]
    [ProducesResponseType(500, Type = typeof(ErrorResponse))]
    [ResponseCache(Duration = 3600, // Cache-Control: max-age=5
        Location = ResponseCacheLocation.Any // Cache-Control: public
    )]
    public async Task<ActionResult<IEnumerable<VoiceMapper.VoiceResponse>>> GetVoices()
    {
        try
        {
            var voices = await _repo.RetrieveAllAsync(HttpContext.RequestAborted);
            var response = VoiceMapper.ToResponseList(voices);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving all voices.");
            return StatusCode(StatusCodes.Status500InternalServerError, new ErrorResponse
            {
                Errors = new[] { "An error occurred while processing your request." }
            });
        }
    }

    // GET: api/voices/{id}
    [HttpGet("{id}", Name = nameof(GetVoice))]
    [ProducesResponseType(200, Type = typeof(Voice))]
    [ProducesResponseType(404, Type = typeof(ErrorResponse))]
    [ProducesResponseType(500, Type = typeof(ErrorResponse))]
    [ResponseCache(Duration = 3600, // Cache-Control: max-age=5
        Location = ResponseCacheLocation.Any // Cache-Control: public
    )]
    public async Task<IActionResult> GetVoice(string id)
    {
        try
        {
            var voice = await _repo.RetrieveAsync(id, HttpContext.RequestAborted);
            if (voice == null)
            {
                return NotFound(new ErrorResponse
                {
                    Errors = new[] { $"Voice with ID {id} was not found." }
                });
            }
            return Ok(voice);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving voice with ID {Id}.", id);
            return StatusCode(StatusCodes.Status500InternalServerError, new ErrorResponse
            {
                Errors = new[] { "An error occurred while processing your request." }
            });
        }
    }
}