using FastEndpoints;
using TalkLikeTv.EntityModels;
using TalkLikeTv.Repositories;

namespace TalkLikeTv.FastEndpoints.Endpoints;

// Original TitlesEndpoint with reduced routes
public class TitlesEndpoint : Endpoint<TitlesRequest, Title[]>
{
    private readonly ITitleRepository _titleRepository;

    public TitlesEndpoint(ITitleRepository titleRepository)
    {
        _titleRepository = titleRepository;
    }

    public override void Configure()
    {
        Verbs(Http.GET);
        Routes("/titles", "/titles/{Id}");
        AllowAnonymous();
    }

    public override async Task HandleAsync(TitlesRequest request, CancellationToken ct)
    {
        if (!string.IsNullOrWhiteSpace(request.Id))
        {
            // Retrieve a specific title by ID
            var response = await _titleRepository.RetrieveAsync(request.Id, ct);
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
            // Retrieve all titles
            var response = await _titleRepository.RetrieveAllAsync(ct);
            await SendAsync(response, cancellation: ct);
        }
    }
}

// New separate endpoint for name-based lookup
public class TitlesByNameEndpoint : Endpoint<TitlesByNameRequest, Title[]>
{
    private readonly ITitleRepository _titleRepository;

    public TitlesByNameEndpoint(ITitleRepository titleRepository)
    {
        _titleRepository = titleRepository;
    }

    public override void Configure()
    {
        Verbs(Http.GET);
        Routes("/titles/name/{Name}");
        AllowAnonymous();
    }

    public override async Task HandleAsync(TitlesByNameRequest request, CancellationToken ct)
    {
        var response = await _titleRepository.RetrieveByNameAsync(request.Name, ct);
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

// POST endpoint for creating a title
public class CreateTitleEndpoint : Endpoint<Title, Title>
{
    private readonly ITitleRepository _titleRepository;

    public CreateTitleEndpoint(ITitleRepository titleRepository)
    {
        _titleRepository = titleRepository;
    }

    public override void Configure()
    {
        Verbs(Http.POST);
        Routes("/titles");
        AllowAnonymous();
    }

    public override async Task HandleAsync(Title request, CancellationToken ct)
    {
        var createdTitle = await _titleRepository.CreateAsync(request, ct);
        
        // Explicitly set the Location header
        var locationUrl = $"/titles/{createdTitle.TitleId}";
        HttpContext.Response.Headers.Location = locationUrl;
        
        // Send the response with status code 201 (Created)
        await SendAsync(createdTitle, 201, cancellation: ct);
    }
}

// PUT endpoint for updating a title
public class UpdateTitleEndpoint : Endpoint<UpdateTitleRequest, IResult>
{
    private readonly ITitleRepository _titleRepository;

    public UpdateTitleEndpoint(ITitleRepository titleRepository)
    {
        _titleRepository = titleRepository;
    }

    public override void Configure()
    {
        Verbs(Http.PUT);
        Routes("/titles/{Id}");
        AllowAnonymous();
    }

    public override async Task HandleAsync(UpdateTitleRequest request, CancellationToken ct)
    {
        var title = new Title
        {
            TitleName = request.Title.TitleName,
            Description = request.Title.Description,
            OriginalLanguageId = request.Title.OriginalLanguageId,
            NumPhrases = request.Title.NumPhrases,
            Popularity = request.Title.Popularity
        };

        var updated = await _titleRepository.UpdateAsync(request.Id, title, ct);
        if (updated)
        {
            await SendOkAsync(ct);
        }
        else
        {
            await SendNotFoundAsync(ct);
        }
    }
}

// DELETE endpoint for deleting a title
public class DeleteTitleEndpoint : Endpoint<TitlesRequest, IResult>
{
    private readonly ITitleRepository _titleRepository;

    public DeleteTitleEndpoint(ITitleRepository titleRepository)
    {
        _titleRepository = titleRepository;
    }

    public override void Configure()
    {
        Verbs(Http.DELETE);
        Routes("/titles/{Id}");
        AllowAnonymous();
    }

    public override async Task HandleAsync(TitlesRequest request, CancellationToken ct)
    {
        var deleted = await _titleRepository.DeleteAsync(request.Id, ct);
        if (deleted)
        {
            await SendNoContentAsync(ct);
        }
        else
        {
            await SendNotFoundAsync(ct);
        }
    }
}

// SEARCH endpoint for searching titles
public class SearchTitlesEndpoint : Endpoint<SearchTitlesRequest, SearchTitlesResponse>
{
    private readonly ITitleRepository _titleRepository;

    public SearchTitlesEndpoint(ITitleRepository titleRepository)
    {
        _titleRepository = titleRepository;
    }

    public override void Configure()
    {
        Verbs(Http.GET);
        Routes("/titles/search");
        AllowAnonymous();
    }

    public override async Task HandleAsync(SearchTitlesRequest request, CancellationToken ct)
    {
        var (titles, totalCount) = await _titleRepository.SearchTitlesAsync(
            request.LanguageId,
            request.Keyword,
            request.SearchType,
            request.PageNumber,
            request.PageSize,
            ct
        );

        await SendAsync(new SearchTitlesResponse
        {
            Titles = titles,
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        }, cancellation: ct);
    }
}

// Request and response models
public record TitlesRequest(string Id);
public record TitlesByNameRequest(string Name);

public class SearchTitlesRequest
{
    public string? LanguageId { get; set; }
    public string? Keyword { get; set; }
    public string SearchType { get; set; } = "Default";
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

public class SearchTitlesResponse
{
    public Title[] Titles { get; set; } = Array.Empty<Title>();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
}

public record UpdateTitleRequest(string Id, UpdateTitleDto Title);

public class UpdateTitleDto
{
    public string TitleName { get; set; }
    public string Description { get; set; }
    public int NumPhrases { get; set; }
    public int Popularity { get; set; }
    public int OriginalLanguageId { get; set; }
}