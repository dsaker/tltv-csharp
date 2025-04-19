using FastEndpoints;
using TalkLikeTv.EntityModels;
using TalkLikeTv.Repositories;

namespace TalkLikeTv.FastEndpoints.Endpoints;

public class LanguagesByTagEndpoint : Endpoint<LanguagesByTagRequest, Language[]>
{
    private readonly ILanguageRepository _languageRepository;

    public LanguagesByTagEndpoint(ILanguageRepository languageRepository)
    {
        _languageRepository = languageRepository;
    }

    public override void Configure()
    {
        Verbs(Http.GET);
        Routes("/languages/tag/{Tag}");
        AllowAnonymous();
    }

    public override async Task HandleAsync(LanguagesByTagRequest request, CancellationToken ct)
    {
        var response = await _languageRepository.RetrieveByTagAsync(request.Tag, ct);
        if (response is not null)
        {
            await SendAsync(new[] { response }, cancellation: ct);
        }
        else
        {
            await SendNotFoundAsync(ct);
        }
    }
}

public class LanguagesEndpoint : Endpoint<LanguagesRequest, Language[]>
{
    private readonly ILanguageRepository _languageRepository;

    public LanguagesEndpoint(ILanguageRepository languageRepository)
    {
        _languageRepository = languageRepository;
    }

    public override void Configure()
    {
        Verbs(Http.GET);
        Routes("/languages", "/languages/{Id}");
        AllowAnonymous();
    }

    public override async Task HandleAsync(LanguagesRequest request, CancellationToken ct)
    {
        if (!string.IsNullOrWhiteSpace(request.Id))
        {
            // Retrieve a specific language by ID
            var response = await _languageRepository.RetrieveAsync(request.Id, ct);
            if (response is not null)
            {
                await SendAsync(new[] { response }, cancellation: ct);
            }
            else
            {
                await SendNotFoundAsync(ct);
            }
        }
        else
        {
            // Retrieve all languages
            var response = await _languageRepository.RetrieveAllAsync(ct);
            await SendAsync(response, cancellation: ct);
        }
    }
}

public record LanguagesRequest(string Id);
public record LanguagesByTagRequest(string Tag);
