using TalkLikeTv.EntityModels;

namespace TalkLikeTv.Mvc.Models;

public record CreateTitleViewModel(
    CreateTitleFormModel CreateTitleFormModel,
    bool HasErrors,
    IEnumerable<string> ValidationErrors);