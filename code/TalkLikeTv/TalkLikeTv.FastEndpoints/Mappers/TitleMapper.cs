using FastEndpoints;
using TalkLikeTv.EntityModels;

namespace TalkLikeTv.Utilities.Mappers;

public class TitleMapper : Mapper<CreateTitleRequest, TitleResponse, Title>
{
    public override TitleResponse FromEntity(Title title)
    {
        return new TitleResponse(
            title.TitleId,
            title.TitleName,
            title.Description,
            title.NumPhrases,
            title.Popularity,
            title.OriginalLanguageId
        );
    }
}

public record TitleResponse(
    int TitleId,
    string TitleName,
    string? Description,
    int NumPhrases,
    int Popularity,
    int? OriginalLanguageId
);

public record CreateTitleRequest(
    string TitleName,
    string Description,
    int NumPhrases,
    int OriginalLanguageId
);