namespace SignalEyes.Infrastructure;

public sealed class RabbitMqTransportOptions
{
    public string Host { get; init; } = "localhost";

    public int Port { get; init; } = 5672;

    public string? Username { get; init; }

    public string? Password { get; init; }

    public string RawMqttExchange { get; init; } = "signaleyes.raw-mqtt";

    public string RawMqttQueue { get; init; } = "signaleyes.raw-mqtt.device-gateway";

    public string RoutingKey { get; init; } = "raw-mqtt";
}
