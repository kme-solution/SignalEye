public sealed class MqttProtocolOptions
{
    public int Port { get; init; } = 1883;

    public string? Username { get; init; }

    public string? Password { get; init; }
}
