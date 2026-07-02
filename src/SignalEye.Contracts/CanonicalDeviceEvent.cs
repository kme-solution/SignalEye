using System.Text.Json.Serialization;

namespace SignalEye.Contracts;

public sealed record CanonicalDeviceEvent(
    [property: JsonPropertyName("eventId")] string EventId,
    [property: JsonPropertyName("tenantId")] string TenantId,
    [property: JsonPropertyName("siteId")] string SiteId,
    [property: JsonPropertyName("deviceId")] string DeviceId,
    [property: JsonPropertyName("source")] string Source,
    [property: JsonPropertyName("protocol")] string Protocol,
    [property: JsonPropertyName("eventType")] string EventType,
    [property: JsonPropertyName("occurredAtUtc")] DateTimeOffset OccurredAtUtc,
    [property: JsonPropertyName("rawMessage")] RawMqttMessage RawMessage,
    [property: JsonPropertyName("readings")] IReadOnlyList<TelemetryReading> Readings,
    [property: JsonPropertyName("metadata")] IReadOnlyDictionary<string, string> Metadata)
{
    public const int ContractVersion = 1;
}
