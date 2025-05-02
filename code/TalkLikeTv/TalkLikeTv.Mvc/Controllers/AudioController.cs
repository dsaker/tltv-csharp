using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TalkLikeTv.EntityModels;
using TalkLikeTv.Mvc.Models;
using TalkLikeTv.Services;
using TalkLikeTv.Repositories;
using TalkLikeTv.Services.Abstractions;

namespace TalkLikeTv.Mvc.Controllers;

public class AudioController : Controller
{
    private readonly ILogger<AudioController> _logger;
    private readonly ITokenService _tokenService;
    private readonly IAudioProcessingService _audioProcessingService;
    private readonly IAudioFileService _audioFileService;
    private readonly IWebHostEnvironment _env;
    private readonly ILanguageRepository _languageRepository;
    private readonly IVoiceRepository _voiceRepository;
    private readonly ITitleRepository _titleRepository;

    public AudioController(
        ILogger<AudioController> logger,
        ITokenService tokenService,
        IAudioProcessingService audioProcessingService,
        IAudioFileService audioFileService,
        ILanguageRepository languageRepository,
        IVoiceRepository voiceRepository,
        ITitleRepository titleRepository,
        IWebHostEnvironment env)
    {
        _logger = logger;
        _tokenService = tokenService;
        _audioProcessingService = audioProcessingService;
        _audioFileService = audioFileService;
        _env = env ?? throw new ArgumentNullException(nameof(env));
        _languageRepository = languageRepository;
        _voiceRepository = voiceRepository;
        _titleRepository = titleRepository;
    }

    [HttpGet]
    public IActionResult CreateTitle(Voice toVoice, Voice fromVoice, int pauseDuration, string pattern)
    {
        var model = new CreateTitleViewModel(toVoice, fromVoice, pattern, pauseDuration, false, Enumerable.Empty<string>());
        return View("CreateTitle", model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateTitle(IFormCollection form)
    {
        var (formModel, errorResult) = await ValidateCreateTitleFormModel(form);
        if (errorResult != null) return errorResult;

        try
        {
            var tokenResult = await _tokenService.CheckTokenStatus(formModel!.Token!, HttpContext.RequestAborted);
            if (!tokenResult.Success)
            {
                ModelState.AddModelError("", tokenResult.ErrorMessage ?? "Invalid token.");
                return await CreateTitleErrorView(formModel);
            }

            var result = _audioFileService.ExtractAndValidatePhraseStrings(formModel.File!);
            if (result.Errors.Any())
            {
                foreach (var error in result.Errors) ModelState.AddModelError("", error);
                return await CreateTitleErrorView(formModel);
            }

            var phraseStrings = result.PhraseStrings;
            if (phraseStrings == null || !ModelState.IsValid) return await CreateTitleErrorView(formModel);

            var (detectedLang, detectionErrors) = await _audioProcessingService.DetectLanguageAsync(phraseStrings, HttpContext.RequestAborted);
            if (detectedLang == null || detectionErrors.Any())
            {
                var errorDetails = detectionErrors.Any() ? string.Join("; ", detectionErrors) : "Language detection failed.";
                return Problem(detail: errorDetails, statusCode: StatusCodes.Status500InternalServerError);
            }

            var newTitle = await _audioProcessingService.ProcessTitleAsync(formModel.TitleName!, formModel.Description, phraseStrings, detectedLang, HttpContext.RequestAborted);

            var (zipFilePath, errors) = await _audioProcessingService.ProcessAudioRequestAsync(
                formModel.ToVoiceId ?? -99, 
                formModel.FromVoiceId ?? -99, 
                newTitle, 
                formModel.PauseDuration ?? -99, 
                formModel.Pattern ?? "", 
                HttpContext.RequestAborted);

            if (errors.Any())
            {
                _logger.LogWarning("Audio processing returned errors: {Errors}", string.Join("; ", errors));
                foreach (var error in errors) ModelState.AddModelError("", error);
                return StatusCode(StatusCodes.Status500InternalServerError, ModelState);
            }

            if (zipFilePath == null)
            {
                _logger.LogError("ProcessAudioRequestAsync returned null zipFilePath");
                ModelState.AddModelError("", "Failed to create audio files.");
                return await CreateTitleErrorView(formModel);
            }

            _logger.LogInformation("Audio processed successfully: {FileName}", zipFilePath.Name);
            if (_env.IsDevelopment()) return PhysicalFile(zipFilePath.FullName, "application/zip", zipFilePath.Name);
            
            var (markSuccess, markErrors) = await _tokenService.MarkTokenAsUsedAsync(tokenResult.Token!);
            if (markSuccess) return PhysicalFile(zipFilePath.FullName, "application/zip", zipFilePath.Name);

            foreach (var error in markErrors) ModelState.AddModelError("", error);
            return await CreateTitleErrorView(formModel);
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database error while processing title {TitleName}", formModel.TitleName);
            ModelState.AddModelError("", "An error occurred while saving changes to the database.");
            return await CreateTitleErrorView(formModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing title {TitleName}: {ErrorMessage}", formModel.TitleName, ex.Message);
            ModelState.AddModelError("", ex.Message);
            return await CreateTitleErrorView(formModel);
        }
    }

    private async Task<ViewResult> CreateTitleErrorView(CreateTitleFormModel formModel)
    {
        var toVoice = formModel.ToVoiceId.HasValue ? await _voiceRepository.RetrieveAsync(formModel.ToVoiceId.Value.ToString(), HttpContext.RequestAborted) : null;
        var fromVoice = formModel.FromVoiceId.HasValue ? await _voiceRepository.RetrieveAsync(formModel.FromVoiceId.Value.ToString(), HttpContext.RequestAborted) : null;

        if (toVoice == null || fromVoice == null) ModelState.AddModelError("", "Invalid voice id.");
        if (formModel.PauseDuration == null) ModelState.AddModelError("", "Invalid pause duration.");

        var model = new CreateTitleViewModel(toVoice!, fromVoice!, formModel.Pattern!, formModel.PauseDuration, true, ModelState.Values.SelectMany(state => state.Errors).Select(error => error.ErrorMessage));
        return View(model);
    }

    private async Task<(CreateTitleFormModel? formModel, ViewResult? errorResult)> ValidateCreateTitleFormModel(IFormCollection form)
    {
        if (!int.TryParse(form["PauseDuration"], out var pauseDuration)) ModelState.AddModelError("", "Invalid pause duration.");
        if (!int.TryParse(form["ToVoiceId"], out var toVoiceId)) ModelState.AddModelError("", "Invalid toVoiceId.");
        if (!int.TryParse(form["FromVoiceId"], out var fromVoiceId)) ModelState.AddModelError("", "Invalid fromVoiceId.");

        var formModel = new CreateTitleFormModel(toVoiceId, fromVoiceId, pauseDuration, form["Pattern"], form["Token"], form["TitleName"], form["Description"], form.Files["File"]);
        if (!TryValidateModel(formModel) || !ModelState.IsValid) return (null, await CreateTitleErrorView(formModel));

        if (await _titleRepository.RetrieveByNameAsync(formModel.TitleName!, HttpContext.RequestAborted) != null)
        {
            ModelState.AddModelError("", "Title name must be unique.");
            return (null, await CreateTitleErrorView(formModel));
        }

        return (formModel, null);
    }

    [HttpGet]
    public async Task<IActionResult> ChooseAudio(int? titleId)
    {
        _logger.LogInformation("ChooseAudio called with titleId: {TitleId}", titleId);
        var model = new ChooseAudioViewModel(
            await _languageRepository.RetrieveAllAsync(HttpContext.RequestAborted),
            new ChooseAudioFormModel(titleId, null, null, null, null),
            false,
            []);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChooseAudio(ChooseAudioFormModel modelIn)
    {
        if (modelIn.TitleId != null) return RedirectToAction("AudioFromTitle", "Titles", new { titleId = modelIn.TitleId, toVoiceId = modelIn.ToVoice, fromVoiceId = modelIn.FromVoice, pauseDuration = modelIn.PauseDuration, pattern = modelIn.Pattern });

        if (ModelState.IsValid)
        {
            var dbVoices = await _voiceRepository.RetrieveAllAsync(HttpContext.RequestAborted);
            var toVoice = dbVoices.FirstOrDefault(v => v.VoiceId == modelIn.ToVoice);
            var fromVoice = dbVoices.FirstOrDefault(v => v.VoiceId == modelIn.FromVoice);

            if (toVoice != null && fromVoice != null) return CreateTitle(toVoice, fromVoice, modelIn.PauseDuration ?? 0, modelIn.Pattern ?? "");

            ModelState.AddModelError("", "Invalid voice selection.");
            var errorModel = new ChooseAudioViewModel(await _languageRepository.RetrieveAllAsync(HttpContext.RequestAborted), modelIn, !ModelState.IsValid, ModelState.Values.SelectMany(state => state.Errors).Select(error => error.ErrorMessage));
            return View(errorModel);
        }

        var model = new ChooseAudioViewModel(await _languageRepository.RetrieveAllAsync(HttpContext.RequestAborted), modelIn, !ModelState.IsValid, ModelState.Values.SelectMany(state => state.Errors).Select(error => error.ErrorMessage));
        return View(model);
    }

    public class GetVoicesRequest
    {
        public int SelectedLanguage { get; init; }
        public bool IsFromLanguage { get; init; }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> GetVoices([FromBody] GetVoicesRequest request)
    {
        var dbVoices = await _voiceRepository.RetrieveAllAsync(HttpContext.RequestAborted);
        var filteredVoices = dbVoices.Where(v => v.LanguageId == request.SelectedLanguage);

        var voiceData = filteredVoices.Select(v => new {
            v.VoiceId,
            v.DisplayName,
            v.LocaleName,
            v.ShortName,
            v.Gender,
            v.VoiceType,
            Styles = v.Styles.Select(s => s.StyleName).ToList(),
            Scenarios = v.Scenarios.Select(s => s.ScenarioName).ToList(),
            Personalities = v.Personalities.Select(p => p.PersonalityName).ToList()
        }).ToList();

        var modelVoices = voiceData.Select(v => {
            var encoder = HtmlEncoder.Default;
            var descriptionLines = new List<string>
            {
                $"<span class='voice-label'>Gender:</span> {encoder.Encode(v.Gender)}",
                $"<span class='voice-label'>Type:</span> {encoder.Encode(v.VoiceType)}"
            };

            if (v.Styles.Any()) descriptionLines.Add($"<span class='voice-label'>Styles:</span> {string.Join(", ", v.Styles.Select(s => encoder.Encode(s)))}");
            if (v.Scenarios.Any()) descriptionLines.Add($"<span class='voice-label'>Scenarios:</span> {string.Join(", ", v.Scenarios.Select(s => encoder.Encode(s)))}");
            if (v.Personalities.Any()) descriptionLines.Add($"<span class='voice-label'>Personalities:</span> {string.Join(", ", v.Personalities.Select(p => encoder.Encode(p)))}");

            return new VoiceViewModel(v.VoiceId, v.DisplayName, v.LocaleName, v.ShortName, string.Join("<br>", descriptionLines));
        }).ToList();

        var partialViewName = request.IsFromLanguage ? "_FromVoiceSelection" : "_ToVoiceSelection";
        return PartialView(partialViewName, modelVoices);
    }
}