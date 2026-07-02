using SignalEye.Contracts;

namespace SignalEye.Telemetry;

public sealed class CanonicalDeviceEventFactory
{
    private readonly PayloadDecoder _payloadDecoder;

    public CanonicalDeviceEventFactory(PayloadDecoder payloadDecoder)
    {
        _payloadDecoder = payloadDecoder;
    }

    public CanonicalDeviceEvent Create(RawMqttMessage rawMessage)
    {
        var decoded = _payloadDecoder.Decode(rawMessage);
        var metadata = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["topic"] = rawMessage.Topic,
            ["payloadEncoding"] = rawMessage.PayloadEncoding,
            ["brokerHost"] = rawMessage.BrokerHost,
            ["brokerPort"] = rawMessage.BrokerPort.ToString(System.Globalization.CultureInfo.InvariantCulture),
            ["clientId"] = rawMessage.ClientId
        };

        foreach (var item in rawMessage.Metadata)
        {
            metadata[item.Key] = item.Value;
        }

        foreach (var item in decoded.Metadata)
        {
            metadata[item.Key] = item.Value;
        }

        return new CanonicalDeviceEvent(
            EventId: Guid.NewGuid().ToString("N"),
            TenantId: rawMessage.TenantId,
            SiteId: rawMessage.SiteId,
            DeviceId: rawMessage.DeviceId,
            Source: "mqtt",
            Protocol: decoded.Readings.Any(reading =>
                reading.Metadata.TryGetValue("protocol", out var protocol) &&
                protocol.Equals("modbus", StringComparison.OrdinalIgnoreCase))
                ? "modbus"
                : "mqtt",
            EventType: decoded.Readings.Count > 0 ? "telemetry" : "raw-message",
            OccurredAtUtc: rawMessage.ReceivedAtUtc,
            RawMessage: rawMessage,
            Readings: decoded.Readings,
            Metadata: metadata);
    }
}
