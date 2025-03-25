using TalkLikeTv.EntityModels;

namespace TalkLikeTv.Mvc.Models;

public record AudioFromTitleViewModel(
    Title? Title,
    Voice? FromVoice,
    Voice? ToVoice,
    int? PauseDuration,
    string? Pattern,
    bool HasErrors,
    IEnumerable<string> ValidationErrors);