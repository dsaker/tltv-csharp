using TalkLikeTv.EntityModels;

namespace TalkLikeTv.Mvc.Models;

public record VoiceViewModel(
    int Id,
    string DisplayName,
    string LocaleName,
    string ShortName,
    string Details);

public record ChooseAudioViewModel(
    IEnumerable<Language>? Languages,
    ChooseAudioFormModel ChooseAudioFormModel,
    bool HasErrors,
    IEnumerable<string> ValidationErrors);