using System.Text.Json.Serialization;

namespace SignalEyes.Contracts;

public sealed record TelemetryError(
    [property: JsonPropertyName("timestampUtc")] DateTimeOffset TimestampUtc,
    [property: JsonPropertyName("service")] string Service,
    [property: JsonPropertyName("eventType")] string EventType,
    [property: JsonPropertyName("errorCode")] string ErrorCode,
    [property: JsonPropertyName("message")] string Message,
    [property: JsonPropertyName("topic")] string? Topic,
    [property: JsonPropertyName("tenantId")] string? TenantId,
    [property: JsonPropertyName("siteId")] string? SiteId,
    [property: JsonPropertyName("deviceId")] string? DeviceId,
    [property: JsonPropertyName("rawPayload")] string? RawPayload,
    [property: JsonPropertyName("metadata")] IReadOnlyDictionary<string, string> Metadata);
