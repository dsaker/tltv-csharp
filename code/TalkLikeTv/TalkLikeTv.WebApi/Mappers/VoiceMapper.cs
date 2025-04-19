using TalkLikeTv.EntityModels;

namespace TalkLikeTv.WebApi.Mappers;

public class VoiceMapper
{
    public record VoiceResponse(
        int VoiceId,
        string DisplayName,
        string Gender,
        string Locale,
        int? LanguageId,
        List<string>? Personalities,
        List<string>? Scenarios,
        List<string>? Styles
    );
    
    public static VoiceResponse ToResponse(Voice voice)
    {
        return new VoiceResponse(
            voice.VoiceId,
            voice.DisplayName,
            voice.Gender,
            voice.Locale,
            voice.LanguageId,
            voice.Personalities.Select(p => p.PersonalityName).ToList(),
            voice.Scenarios.Select(s => s.ScenarioName).ToList(),
            voice.Styles.Select(st => st.StyleName).ToList()
        );
    }

    public static IEnumerable<VoiceResponse> ToResponseList(IEnumerable<Voice> voices)
    {
        return voices.Select(ToResponse);
    }
}