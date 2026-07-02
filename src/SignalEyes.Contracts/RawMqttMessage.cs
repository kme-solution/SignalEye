using System.Text.Json.Serialization;

namespace SignalEyes.Contracts;

public sealed record RawMqttMessage(
    [property: JsonPropertyName("messageId")] string MessageId,
    [property: JsonPropertyName("tenantId")] string TenantId,
    [property: JsonPropertyName("siteId")] string SiteId,
    [property: JsonPropertyName("deviceId")] string DeviceId,
    [property: JsonPropertyName("brokerHost")] string BrokerHost,
    [property: JsonPropertyName("brokerPort")] int BrokerPort,
    [property: JsonPropertyName("clientId")] string ClientId,
    [property: JsonPropertyName("topic")] string Topic,
    [property: JsonPropertyName("qos")] int QoS,
    [property: JsonPropertyName("retained")] bool Retained,
    [property: JsonPropertyName("receivedAtUtc")] DateTimeOffset ReceivedAtUtc,
    [property: JsonPropertyName("payloadEncoding")] string PayloadEncoding,
    [property: JsonPropertyName("payload")] string Payload,
    [property: JsonPropertyName("metadata")] IReadOnlyDictionary<string, string> Metadata)
{
    public const int ContractVersion = 1;
}
