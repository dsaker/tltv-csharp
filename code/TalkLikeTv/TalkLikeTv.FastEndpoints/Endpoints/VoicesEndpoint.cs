using FastEndpoints;
using TalkLikeTv.EntityModels;
using TalkLikeTv.Repositories;

namespace TalkLikeTv.FastEndpoints.Endpoints;

public class VoicesEndpoint : Endpoint<VoicesRequest, Voice[]>
{
    private readonly IVoiceRepository _voiceRepository;

    public VoicesEndpoint(IVoiceRepository voiceRepository)
    {
        _voiceRepository = voiceRepository;
    }

    public override void Configure()
    {
        Verbs(Http.GET);
        Routes("/voices", "/voices/{LanguageId}");
        AllowAnonymous();
    }

    public override async Task HandleAsync(
        VoicesRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.LanguageId))
        {
            // Retrieve all voices
            var response = await _voiceRepository.RetrieveAllAsync(ct);
            await SendAsync(response, cancellation: ct);
        }
        else
        {
            var response = await RetrieveVoicesByLanguageIdAsync(
                request.LanguageId, ct);
            await SendAsync(response, cancellation: ct);
        }
    }

    private async Task<Voice[]> RetrieveVoicesByLanguageIdAsync(
        string languageId, CancellationToken ct)
    {

        var allVoices = await _voiceRepository.RetrieveAllAsync(ct);
        
        if (int.TryParse(languageId, out int langId))
        {
            return allVoices.Where(v => v.LanguageId == langId).ToArray();
        }
        
        return Array.Empty<Voice>();
    }
}

public record VoicesRequest(string LanguageId);