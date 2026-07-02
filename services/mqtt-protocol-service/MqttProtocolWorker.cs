using Microsoft.Extensions.Options;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Protocol;
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
            "MQTT protocol service starting. Broker={Host}:{Port}, Topic={Topic}",
            _options.Host,
            _options.Port,
            _options.TelemetryTopic);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await RunMqttClientAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                _logger.LogWarning(
                    exception,
                    "MQTT client loop failed. Reconnecting in {DelaySeconds} seconds.",
                    _options.ReconnectBackoffSeconds);
                await Task.Delay(GetReconnectDelay(), stoppingToken);
            }
        }
    }

    private async Task RunMqttClientAsync(CancellationToken stoppingToken)
    {
        var factory = new MqttFactory();
        using var client = factory.CreateMqttClient();

        client.ApplicationMessageReceivedAsync += async args =>
        {
            var topic = args.ApplicationMessage.Topic ?? string.Empty;
            if (!MqttTopicParser.TryParseTelemetryTopic(topic, out _))
            {
                _logger.LogWarning(
                    "Skipping MQTT message with invalid topic {Topic}. Expected {ExpectedTopic}.",
                    topic,
                    MqttTopicParser.TelemetryTopicPattern);
                return;
            }

            var payloadBytes = args.ApplicationMessage.PayloadSegment.ToArray();
            var rawMessage = _messageFactory.Create(
                brokerHost: _options.Host,
                brokerPort: _options.Port,
                clientId: _options.ClientId,
                topic: topic,
                qos: (int)args.ApplicationMessage.QualityOfServiceLevel,
                retained: args.ApplicationMessage.Retain,
                payloadBytes: payloadBytes,
                receivedAtUtc: DateTimeOffset.UtcNow,
                metadata: new Dictionary<string, string>
                {
                    ["tlsEnabled"] = _options.TlsEnabled.ToString(System.Globalization.CultureInfo.InvariantCulture)
                });

            await _publisher.PublishAsync(rawMessage, stoppingToken);
            await _fileLogger.WriteMqttTelemetryAsync(rawMessage, stoppingToken);

            _logger.LogInformation(
                "Forwarded raw MQTT telemetry {MessageId} for {TenantId}/{SiteId}/{DeviceId}.",
                rawMessage.MessageId,
                rawMessage.TenantId,
                rawMessage.SiteId,
                rawMessage.DeviceId);
        };

        var options = BuildClientOptions();
        await client.ConnectAsync(options, stoppingToken);

        var subscribeOptions = factory.CreateSubscribeOptionsBuilder()
            .WithTopicFilter(filter => filter
                .WithTopic(_options.TelemetryTopic)
                .WithQualityOfServiceLevel(ToMqttQoS(_options.QoS)))
            .Build();

        await client.SubscribeAsync(subscribeOptions, stoppingToken);
        _logger.LogInformation("MQTT protocol service subscribed to {Topic}.", _options.TelemetryTopic);

        try
        {
            await Task.Delay(Timeout.InfiniteTimeSpan, stoppingToken);
        }
        finally
        {
            if (client.IsConnected)
            {
                await client.DisconnectAsync(cancellationToken: CancellationToken.None);
            }
        }
    }

    private MqttClientOptions BuildClientOptions()
    {
        var builder = new MqttClientOptionsBuilder()
            .WithTcpServer(_options.Host, _options.Port)
            .WithClientId(_options.ClientId)
            .WithCleanSession();

        if (!string.IsNullOrWhiteSpace(_options.Username))
        {
            builder = builder.WithCredentials(_options.Username, _options.Password);
        }

        if (_options.TlsEnabled)
        {
            builder = builder.WithTlsOptions(tls => tls.UseTls());
        }

        return builder.Build();
    }

    private TimeSpan GetReconnectDelay() =>
        TimeSpan.FromSeconds(Math.Max(1, _options.ReconnectBackoffSeconds));

    private static MqttQualityOfServiceLevel ToMqttQoS(int qos) =>
        qos switch
        {
            <= 0 => MqttQualityOfServiceLevel.AtMostOnce,
            1 => MqttQualityOfServiceLevel.AtLeastOnce,
            _ => MqttQualityOfServiceLevel.ExactlyOnce
        };
}
