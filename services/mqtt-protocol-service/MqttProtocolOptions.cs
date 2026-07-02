public sealed class MqttProtocolOptions
{
    public string Host { get; init; } = "localhost";

    public int Port { get; init; } = 1883;

    public string ClientId { get; init; } = "signaleyes-mqtt-protocol-service";

    public string TelemetryTopic { get; init; } = "signaleyes/+/+/+/telemetry";

    public string? Username { get; init; }

    public string? Password { get; init; }

    public bool TlsEnabled { get; init; }

    public int QoS { get; init; } = 1;

    public int ReconnectBackoffSeconds { get; init; } = 5;
}
