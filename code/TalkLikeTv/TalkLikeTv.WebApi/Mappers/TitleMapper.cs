using TalkLikeTv.EntityModels;

namespace TalkLikeTv.WebApi.Mappers;

public class TitleMapper
{
    public record TitleResponse(
        int TitleId,
        string TitleName,
        string? Description,
        int NumPhrases,
        int Popularity,
        int? OriginalLanguageId
    );
    
    public static TitleResponse ToResponse(Title title)
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

    public static IEnumerable<TitleResponse> ToResponseList(IEnumerable<Title> titles)
    {
        return titles.Select(ToResponse);
    }
}