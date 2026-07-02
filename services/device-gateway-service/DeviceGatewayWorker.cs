using SignalEye.Contracts;
using SignalEye.Infrastructure;
using SignalEye.Telemetry;

public sealed class DeviceGatewayWorker : BackgroundService
{
    private readonly ILogger<DeviceGatewayWorker> _logger;
    private readonly IRawMqttMessageConsumer _consumer;
    private readonly CanonicalDeviceEventFactory _eventFactory;
    private readonly TelemetryFileLogger _fileLogger;

    public DeviceGatewayWorker(
        ILogger<DeviceGatewayWorker> logger,
        IRawMqttMessageConsumer consumer,
        CanonicalDeviceEventFactory eventFactory,
        TelemetryFileLogger fileLogger)
    {
        _logger = logger;
        _consumer = consumer;
        _eventFactory = eventFactory;
        _fileLogger = fileLogger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Device gateway service started.");

        await foreach (var rawMessage in _consumer.ConsumeAsync(stoppingToken))
        {
            await ProcessAsync(rawMessage, stoppingToken);
        }
    }

    private async Task ProcessAsync(RawMqttMessage rawMessage, CancellationToken stoppingToken)
    {
        try
        {
            if (!IsValid(rawMessage, out var error))
            {
                await _fileLogger.WriteGatewayErrorAsync(error, stoppingToken);
                _logger.LogWarning(
                    "Skipped invalid raw MQTT message {MessageId}: {Reason}",
                    rawMessage.MessageId,
                    error.Message);
                return;
            }

            await _fileLogger.WriteGatewayReceivedAsync(rawMessage, stoppingToken);
            var deviceEvent = _eventFactory.Create(rawMessage);
            await _fileLogger.WriteGatewayProcessedAsync(deviceEvent, stoppingToken);

            _logger.LogInformation(
                "Processed canonical device event {EventId} for {TenantId}/{SiteId}/{DeviceId} with {ReadingCount} readings.",
                deviceEvent.EventId,
                deviceEvent.TenantId,
                deviceEvent.SiteId,
                deviceEvent.DeviceId,
                deviceEvent.Readings.Count);
        }
        catch (Exception exception)
        {
            var telemetryError = new TelemetryError(
                TimestampUtc: DateTimeOffset.UtcNow,
                Service: "device-gateway-service",
                EventType: "gateway.telemetry.error",
                ErrorCode: "gateway-processing-failed",
                Message: exception.Message,
                Topic: rawMessage.Topic,
                TenantId: rawMessage.TenantId,
                SiteId: rawMessage.SiteId,
                DeviceId: rawMessage.DeviceId,
                RawPayload: rawMessage.Payload,
                Metadata: new Dictionary<string, string>
                {
                    ["messageId"] = rawMessage.MessageId
                });
            await _fileLogger.WriteGatewayErrorAsync(telemetryError, stoppingToken);
            _logger.LogWarning(exception, "Failed to process raw MQTT message {MessageId}.", rawMessage.MessageId);
        }
    }

    private static bool IsValid(RawMqttMessage message, out TelemetryError error)
    {
        var missingFields = new List<string>();
        if (string.IsNullOrWhiteSpace(message.MessageId)) missingFields.Add(nameof(message.MessageId));
        if (string.IsNullOrWhiteSpace(message.TenantId)) missingFields.Add(nameof(message.TenantId));
        if (string.IsNullOrWhiteSpace(message.SiteId)) missingFields.Add(nameof(message.SiteId));
        if (string.IsNullOrWhiteSpace(message.DeviceId)) missingFields.Add(nameof(message.DeviceId));
        if (string.IsNullOrWhiteSpace(message.Topic)) missingFields.Add(nameof(message.Topic));
        if (string.IsNullOrWhiteSpace(message.PayloadEncoding)) missingFields.Add(nameof(message.PayloadEncoding));

        if (missingFields.Count == 0)
        {
            error = new TelemetryError(
                TimestampUtc: DateTimeOffset.UtcNow,
                Service: "device-gateway-service",
                EventType: "gateway.telemetry.error",
                ErrorCode: string.Empty,
                Message: string.Empty,
                Topic: message.Topic,
                TenantId: message.TenantId,
                SiteId: message.SiteId,
                DeviceId: message.DeviceId,
                RawPayload: message.Payload,
                Metadata: new Dictionary<string, string>());
            return true;
        }

        error = new TelemetryError(
            TimestampUtc: DateTimeOffset.UtcNow,
            Service: "device-gateway-service",
            EventType: "gateway.telemetry.error",
            ErrorCode: "missing-required-field",
            Message: $"Raw MQTT message is missing required fields: {string.Join(", ", missingFields)}.",
            Topic: message.Topic,
            TenantId: message.TenantId,
            SiteId: message.SiteId,
            DeviceId: message.DeviceId,
            RawPayload: message.Payload,
            Metadata: new Dictionary<string, string>
            {
                ["messageId"] = message.MessageId
            });
        return false;
    }
}
