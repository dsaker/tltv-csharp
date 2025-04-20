using TalkLikeTv.EntityModels;

namespace TalkLikeTv.Mvc.Models;

public class SearchTitlesViewModel
{
    public string? OriginalLanguageId { get; set; }
    public string? Keyword { get; set; }
    public IEnumerable<Title>? Results { get; set; }
    public IEnumerable<Language>? TitleLanguages { get; set; }
    public string SearchType { get; set; } = "Language";
    public int PageNumber { get; set; } = 1;
    public int TotalPages { get; set; }
}