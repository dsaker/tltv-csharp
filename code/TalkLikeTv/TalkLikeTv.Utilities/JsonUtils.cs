using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using TalkLikeTv.EntityModels;

namespace TalkLikeTv.Utilities;

public class JsonUtils
{
    public class DeserializeResult<T>
    {
        public T? Result { get; set; }
        public List<string> Errors { get; set; } = new();
    }

    public static DeserializeResult<Voice> DeserializeVoice(string voiceJson, ILogger logger)
    {
        var result = new DeserializeResult<Voice>();
        try
        {
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                ReferenceHandler = ReferenceHandler.Preserve
            };
            result.Result = JsonSerializer.Deserialize<Voice>(voiceJson, options);
        }
        catch (JsonException ex)
        {
            logger.LogError(ex, "Failed to deserialize voice JSON.");
            result.Errors.Add("Invalid voice data.");
        }

        return result;
    }
}