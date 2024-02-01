using System.Text.Json;
using System.Text.Json.Serialization;

namespace APU.WebApp.Utils;

public static class Json
{
    public static JsonSerializerOptions Options => new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        ReferenceHandler = ReferenceHandler.IgnoreCycles,
        WriteIndented = true, 
        PropertyNameCaseInsensitive = true,
        IgnoreReadOnlyProperties = true,
        IgnoreReadOnlyFields = true
    };
}