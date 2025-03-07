using TalkLikeTv.EntityModels;

namespace TalkLikeTv.Mvc.Models;

public record VoiceViewModel(
    int Id,
    string DisplayName,
    string LocaleName,
    string ShortName,
    string Details);

public record CreateAudioViewModel(
    IEnumerable<Language>? Languages,
    CreateAudioFormModel CreateAudioFormModel,
    bool HasErrors,
    IEnumerable<string> ValidationErrors);