using TalkLikeTv.EntityModels;

namespace TalkLikeTv.Mvc.Models;

public record CreateTitleViewModel(
    IEnumerable<Language>? Languages,
    CreateTitleFormModel CreateTitleFormModel,
    bool HasErrors,
    IEnumerable<string> ValidationErrors);