using SignalEyes.Contracts;

namespace SignalEyes.Telemetry;

public sealed class TelemetryNormalizer
{
    public TelemetryMessage NormalizeMqttPayload(string topic, string payload)
    {
        var deviceId = ExtractDeviceId(topic);

        return new TelemetryMessage(
            DeviceId: deviceId,
            Source: "mqtt",
            ReceivedAt: DateTimeOffset.UtcNow,
            PayloadType: "raw",
            RawPayload: payload);
    }

    private static string ExtractDeviceId(string topic)
    {
        var segments = topic.Split('/', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        return segments.Length > 0 ? segments[^1] : "unknown-device";
    }
}
