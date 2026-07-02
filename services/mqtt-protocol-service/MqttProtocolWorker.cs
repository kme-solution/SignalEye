using Microsoft.Extensions.Options;
using MQTTnet;
using MQTTnet.Protocol;
using MQTTnet.Server;
using SignalEye.Infrastructure;
using SignalEye.Telemetry;

public sealed class MqttProtocolWorker : BackgroundService
{
    private readonly ILogger<MqttProtocolWorker> _logger;
    private readonly MqttProtocolOptions _options;
    private readonly RawMqttMessageFactory _messageFactory;
    private readonly IRawMqttMessagePublisher _publisher;
    private readonly TelemetryFileLogger _fileLogger;

    public MqttProtocolWorker(
        ILogger<MqttProtocolWorker> logger,
        IOptions<MqttProtocolOptions> options,
        RawMqttMessageFactory messageFactory,
        IRawMqttMessagePublisher publisher,
        TelemetryFileLogger fileLogger)
    {
        _logger = logger;
        _options = options.Value;
        _messageFactory = messageFactory;
        _publisher = publisher;
        _fileLogger = fileLogger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "MQTTnet server starting on port {Port}. AuthenticationRequired={AuthenticationRequired}",
            _options.Port,
            !string.IsNullOrWhiteSpace(_options.Username));

        var factory = new MqttFactory();
        var serverOptions = new MqttServerOptionsBuilder()
            .WithDefaultEndpoint()
            .WithDefaultEndpointPort(_options.Port)
            .Build();
        using var server = factory.CreateMqttServer(serverOptions);

        server.ValidatingConnectionAsync += args =>
        {
            if (IsConnectionAllowed(args.UserName, args.Password))
            {
                return Task.CompletedTask;
            }

            args.ReasonCode = MqttConnectReasonCode.BadUserNameOrPassword;
            _logger.LogWarning("Rejected MQTT connection from client {ClientId}.", args.ClientId);
            return Task.CompletedTask;
        };

        server.InterceptingPublishAsync += args => ProcessPublishedMessageAsync(
            args.ClientId,
            args.ApplicationMessage,
            args.CancellationToken);

        await server.StartAsync();
        _logger.LogInformation("MQTTnet server listening on port {Port}.", _options.Port);

        try
        {
            await Task.Delay(Timeout.InfiniteTimeSpan, stoppingToken);
        }
        finally
        {
            await server.StopAsync();
        }
    }

    private bool IsConnectionAllowed(string? username, string? password)
    {
        if (string.IsNullOrWhiteSpace(_options.Username)) return true;

        return string.Equals(username, _options.Username, StringComparison.Ordinal)
            && string.Equals(password, _options.Password, StringComparison.Ordinal);
    }

    private async Task ProcessPublishedMessageAsync(
        string clientId,
        MqttApplicationMessage applicationMessage,
        CancellationToken cancellationToken)
    {
        var topic = applicationMessage.Topic ?? string.Empty;
        if (!MqttTopicParser.TryParseTelemetryTopic(topic, out _))
        {
            _logger.LogWarning(
                "Skipping MQTT message with invalid topic {Topic}. Expected {ExpectedTopic}.",
                topic,
                MqttTopicParser.TelemetryTopicPattern);
            return;
        }

        var rawMessage = _messageFactory.Create(
            brokerHost: Environment.MachineName,
            brokerPort: _options.Port,
            clientId: clientId,
            topic: topic,
            qos: (int)applicationMessage.QualityOfServiceLevel,
            retained: applicationMessage.Retain,
            payloadBytes: applicationMessage.PayloadSegment.ToArray(),
            receivedAtUtc: DateTimeOffset.UtcNow,
            metadata: new Dictionary<string, string> { ["mqttServer"] = "MQTTnet" });

        await _publisher.PublishAsync(rawMessage, cancellationToken);
        await _fileLogger.WriteMqttTelemetryAsync(rawMessage, cancellationToken);

        _logger.LogInformation(
            "Forwarded raw MQTT telemetry {MessageId} for {TenantId}/{SiteId}/{DeviceId} from client {ClientId}.",
            rawMessage.MessageId,
            rawMessage.TenantId,
            rawMessage.SiteId,
            rawMessage.DeviceId,
            clientId);
    }
}
