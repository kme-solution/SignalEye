using System.Text.Json.Serialization;

namespace SignalEyes.Contracts;

public sealed record TelemetryReading(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("value")] string Value,
    [property: JsonPropertyName("unit")] string? Unit,
    [property: JsonPropertyName("quality")] string Quality,
    [property: JsonPropertyName("timestampUtc")] DateTimeOffset TimestampUtc,
    [property: JsonPropertyName("metadata")] IReadOnlyDictionary<string, string> Metadata)
{
    public const int ContractVersion = 1;
}
