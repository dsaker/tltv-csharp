using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TalkLikeTv.Repositories;
using TalkLikeTv.WebApi.Models;
using TalkLikeTv.Services;

namespace TalkLikeTv.WebApi.Controllers;

// Base address: api/audio
[Route("api/[controller]")]
[ApiController]
public class AudioController : ControllerBase
{
    private readonly IAudioProcessingService _audioProcessingService;
    private readonly ITokenService _tokenService;
    private readonly ITitleRepository _titleRepository;
    private readonly ILogger<AudioController> _logger;
    
    public AudioController(
        IAudioProcessingService audioProcessingService,
        ITokenService tokenService,
        ILogger<AudioController> logger, 
        ITitleRepository titleRepository)
    {
        _audioProcessingService = audioProcessingService;
        _tokenService = tokenService;
        _logger = logger;
        _titleRepository = titleRepository;
    }

    [HttpPost("fromTitle")]
[Consumes("multipart/form-data")]
[ProducesResponseType(StatusCodes.Status200OK, Type = typeof(FileResult))]
[ProducesResponseType(StatusCodes.Status400BadRequest)]
[ProducesResponseType(StatusCodes.Status500InternalServerError)]
public async Task<IActionResult> AudioFromTitle([FromForm] AudioFromTitleApiModel model)
{
    if (!ModelState.IsValid)
    {
        return BadRequest(new ErrorResponse { Errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)) });
    }

    try
    {
        var tokenResult = await _tokenService.CheckTokenStatus(model.Token, HttpContext.RequestAborted);
        if (!tokenResult.Success)
        {
            return BadRequest(new ErrorResponse { Errors = new[] { tokenResult.ErrorMessage ?? "Invalid token." } });
        }

        var title = await _titleRepository.RetrieveAsync(model.TitleId.ToString(), HttpContext.RequestAborted);
        if (title == null)
        {
            return BadRequest(new ErrorResponse { Errors = new[] { "Title not found." } });
        }

        var (zipFilePath, errors) = await _audioProcessingService.ProcessAudioRequestAsync(
            model.ToVoiceId,
            model.FromVoiceId,
            title,
            model.PauseDuration,
            model.Pattern ?? "",
            HttpContext.RequestAborted);

        if (errors.Any())
        {
            return BadRequest(new ErrorResponse { Errors = errors });
        }

        if (zipFilePath == null)
        {
            return StatusCode(StatusCodes.Status500InternalServerError,
                new ErrorResponse { Errors = new[] { "Failed to create audio files." } });
        }

        var (markSuccess, markErrors) = await _audioProcessingService.MarkTokenAsUsedAsync(model.Token, HttpContext.RequestAborted);
        if (!markSuccess)
        {
            return BadRequest(new ErrorResponse { Errors = markErrors });
        }

        return File(System.IO.File.OpenRead(zipFilePath.FullName), "application/zip", zipFilePath.Name);
    }
    catch (DbUpdateException ex)
    {
        _logger.LogError(ex, "Database error while processing audio for title {TitleId}", model.TitleId);
        return StatusCode(StatusCodes.Status500InternalServerError,
            new ErrorResponse { Errors = new[] { "An error occurred while saving changes to the database." } });
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error processing audio for title {TitleId}: {ErrorMessage}", model.TitleId, ex.Message);
        return StatusCode(StatusCodes.Status500InternalServerError,
            new ErrorResponse { Errors = new[] { ex.Message } });
    }
}
}