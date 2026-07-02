namespace SignalEye.Telemetry;

public sealed record MqttTopicParts(
    string TenantId,
    string SiteId,
    string DeviceId);
