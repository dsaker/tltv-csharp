using FastEndpoints;
using TalkLikeTv.EntityModels;
using TalkLikeTv.FastEndpoints.Mappers;
using TalkLikeTv.Repositories;

namespace TalkLikeTv.FastEndpoints.Endpoints;

public class VoicesEndpoint : Endpoint<VoicesRequest, VoiceResponse[], VoiceMapper>
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

    public override async Task HandleAsync(VoicesRequest request, CancellationToken ct)
    {
        var voices = string.IsNullOrWhiteSpace(request.LanguageId)
            ? await _voiceRepository.RetrieveAllAsync(ct)
            : await RetrieveVoicesByLanguageIdAsync(request.LanguageId, ct);

        var response = Map.FromEntity(voices);
        await SendAsync(response, cancellation: ct);
    }

    private async Task<Voice[]> RetrieveVoicesByLanguageIdAsync(string languageId, CancellationToken ct)
    {

        var allVoices = await _voiceRepository.RetrieveAllAsync(ct);
        
        if (int.TryParse(languageId, out int langId))
        {
            return allVoices.Where(v => v.LanguageId == langId).ToArray();
        }
        
        return Array.Empty<Voice>();
    }
}
