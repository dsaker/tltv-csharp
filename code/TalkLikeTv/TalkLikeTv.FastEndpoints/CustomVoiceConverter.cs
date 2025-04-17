using System.Text.Json;
using System.Text.Json.Serialization;
using TalkLikeTv.EntityModels;

public class CustomVoiceConverter : JsonConverter<Voice>
{
    public override Voice Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return JsonSerializer.Deserialize<Voice>(ref reader, options)!;
    }

    public override void Write(Utf8JsonWriter writer, Voice value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();

        writer.WriteNumber(nameof(value.VoiceId), value.VoiceId);
        writer.WriteString(nameof(value.DisplayName), value.DisplayName);
        writer.WriteString(nameof(value.ShortName), value.ShortName);

        writer.WritePropertyName(nameof(value.Personalities));
        WriteLimitedPersonalities(writer, value.Personalities, options);

        writer.WritePropertyName(nameof(value.Scenarios));
        WriteLimitedScenarios(writer, value.Scenarios, options);

        writer.WritePropertyName(nameof(value.Styles));
        WriteLimitedStyles(writer, value.Styles, options);

        writer.WriteEndObject();
    }

    private void WriteLimitedPersonalities(Utf8JsonWriter writer, ICollection<Personality> personalities, JsonSerializerOptions options)
    {
        writer.WriteStartArray();
        foreach (var personality in personalities)
        {
            writer.WriteStartObject();
            writer.WriteNumber(nameof(personality.PersonalityId), personality.PersonalityId);
            writer.WriteString(nameof(personality.PersonalityName), personality.PersonalityName);
            writer.WriteEndObject();
        }
        writer.WriteEndArray();
    }

    private void WriteLimitedScenarios(Utf8JsonWriter writer, ICollection<Scenario> scenarios, JsonSerializerOptions options)
    {
        writer.WriteStartArray();
        foreach (var scenario in scenarios)
        {
            writer.WriteStartObject();
            writer.WriteNumber(nameof(scenario.ScenarioId), scenario.ScenarioId);
            writer.WriteString(nameof(scenario.ScenarioName), scenario.ScenarioName);
            writer.WriteEndObject();
        }
        writer.WriteEndArray();
    }

    private void WriteLimitedStyles(Utf8JsonWriter writer, ICollection<Style> styles, JsonSerializerOptions options)
    {
        writer.WriteStartArray();
        foreach (var style in styles)
        {
            writer.WriteStartObject();
            writer.WriteNumber(nameof(style.StyleId), style.StyleId);
            writer.WriteString(nameof(style.StyleName), style.StyleName);
            writer.WriteEndObject();
        }
        writer.WriteEndArray();
    }
}