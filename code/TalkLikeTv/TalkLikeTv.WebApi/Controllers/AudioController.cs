using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TalkLikeTv.EntityModels;
using TalkLikeTv.Repositories;
using TalkLikeTv.WebApi.Models;
using TalkLikeTv.Services;
using TalkLikeTv.Services.Abstractions;

namespace TalkLikeTv.WebApi.Controllers;

// Base address: api/languages
[Route("api/[controller]")]
[ApiController]
public class AudioController : ControllerBase
{
    private readonly IAudioFileService _audioFileService;
    private readonly IAudioProcessingService _audioProcessingService;
    private readonly ITokenService _tokenService;
    private readonly ILogger<AudioController> _logger;
    private readonly IWebHostEnvironment _env;
    
    public AudioController(
        IAudioFileService audioFileService,
        IAudioProcessingService audioProcessingService,
        ITokenService tokenService,
        ILogger<AudioController> logger,
        IWebHostEnvironment env)
    {
        _audioFileService = audioFileService;
        _audioProcessingService = audioProcessingService;
        _tokenService = tokenService;
        _logger = logger;
        _env = env;
    }
    
 
}