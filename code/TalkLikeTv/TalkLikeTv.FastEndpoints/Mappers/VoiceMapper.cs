using FastEndpoints;
using TalkLikeTv.EntityModels;
using TalkLikeTv.FastEndpoints.Endpoints;

namespace TalkLikeTv.FastEndpoints.Mappers;

public class VoiceMapper : Mapper<VoicesRequest, VoiceResponse[], Voice[]>
{
    public override VoiceResponse[] FromEntity(Voice[] voices)
    {
        return voices.Select(v => new VoiceResponse(
            v.VoiceId,
            v.Platform,
            v.LanguageId,
            v.DisplayName,
            v.LocalName,
            v.ShortName,
            v.Gender,
            v.Locale,
            v.LocaleName,
            v.SampleRateHertz,
            v.VoiceType,
            v.Status,
            v.WordsPerMinute,
            v.Personalities?.Select(p => p.PersonalityName).ToList(),
            v.Scenarios?.Select(s => s.ScenarioName).ToList(),
            v.Styles?.Select(st => st.StyleName).ToList()
        )).ToArray();
    }
}

public record VoiceResponse(
    int VoiceId,
    string Platform,
    int? LanguageId,
    string DisplayName,
    string LocalName,
    string ShortName,
    string Gender,
    string Locale,
    string LocaleName,
    int SampleRateHertz,
    string VoiceType,
    string Status,
    int WordsPerMinute,
    List<string>? Personalities,
    List<string>? Scenarios,
    List<string>? Styles);
    
public record VoicesRequest(string LanguageId);