using System.Text.Json;
using System.Text.Json.Serialization;

namespace TalkLikeTv.Mvc.Helpers;

public static class JsonSerializerSettings
{
    public static JsonSerializerOptions Options => new JsonSerializerOptions
    {
        ReferenceHandler = ReferenceHandler.Preserve,
        WriteIndented = true
    };
}