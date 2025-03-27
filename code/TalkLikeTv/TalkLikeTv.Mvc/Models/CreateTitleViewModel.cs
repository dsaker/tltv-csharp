using TalkLikeTv.EntityModels;

namespace TalkLikeTv.Mvc.Models;

public record CreateTitleViewModel(
    Voice ToVoice,
    Voice FromVoice,
    string Pattern,
    int? PauseDuration,
    bool HasErrors,
    IEnumerable<string> ValidationErrors);