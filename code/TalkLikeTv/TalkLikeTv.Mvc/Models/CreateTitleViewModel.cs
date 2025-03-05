using TalkLikeTv.EntityModels;

namespace TalkLikeTv.Mvc.Models;

public record CreateTitleViewModel(
    Voice ToVoice, 
    Voice FromVoice, 
    AudioFormModel AudioFormModel, 
    bool HasErrors,
    IEnumerable<string> ValidationErrors);