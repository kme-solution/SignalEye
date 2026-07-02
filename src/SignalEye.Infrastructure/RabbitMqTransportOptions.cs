namespace SignalEye.Infrastructure;

public sealed class RabbitMqTransportOptions
{
    public string Host { get; init; } = "localhost";

    public int Port { get; init; } = 5672;

    public string? Username { get; init; }

    public string? Password { get; init; }

    public string RawMqttExchange { get; init; } = "signaleye.raw-mqtt";

    public string RawMqttQueue { get; init; } = "signaleye.raw-mqtt.device-gateway";

    public string RoutingKey { get; init; } = "raw-mqtt";
}
