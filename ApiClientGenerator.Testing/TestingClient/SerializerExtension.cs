using System.Text.Json;
using System.Text.Json.Serialization;

namespace TestingClient;

public static class SerializerExtension
{
    public static void WithAllPossiblyNecessarySettings(this JsonSerializerOptions jsonSerializerOptions)
    {
        jsonSerializerOptions.PropertyNameCaseInsensitive = true;
        //jsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        //jsonSerializerOptions.NumberHandling = JsonNumberHandling.AllowReadingFromString;
        //jsonSerializerOptions.IgnoreReadOnlyFields = true;
        //jsonSerializerOptions.IgnoreReadOnlyProperties = true;
        jsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    }
}
