using System.Text.Json;
using SignalEye.Contracts;

namespace SignalEye.Infrastructure;

public sealed class TelemetryFileLogger
{
    private readonly JsonLineFileWriter _mqttTelemetryWriter;
    private readonly JsonLineFileWriter _gatewayReceivedWriter;
    private readonly JsonLineFileWriter _gatewayProcessedWriter;
    private readonly JsonLineFileWriter _gatewayErrorWriter;

    public TelemetryFileLogger(
        string rootDirectory,
        long maxDirectorySizeBytes = JsonLineFileWriter.DefaultMaxDirectorySizeBytes,
        int retentionDays = 7)
    {
        var retentionAge = TimeSpan.FromDays(retentionDays);
        _mqttTelemetryWriter = new JsonLineFileWriter(
            Path.Combine(rootDirectory, "mqtt-protocol-service"),
            "mqtt-telemetry",
            maxDirectorySizeBytes,
            retentionAge);
        _gatewayReceivedWriter = new JsonLineFileWriter(
            Path.Combine(rootDirectory, "device-gateway-service"),
            "gateway-received",
            maxDirectorySizeBytes,
            retentionAge);
        _gatewayProcessedWriter = new JsonLineFileWriter(
            Path.Combine(rootDirectory, "device-gateway-service"),
            "gateway-processed",
            maxDirectorySizeBytes,
            retentionAge);
        _gatewayErrorWriter = new JsonLineFileWriter(
            Path.Combine(rootDirectory, "device-gateway-service"),
            "gateway-error",
            maxDirectorySizeBytes,
            retentionAge);
    }

    public Task WriteMqttTelemetryAsync(RawMqttMessage message, CancellationToken cancellationToken) =>
        _mqttTelemetryWriter.WriteAsync(new
        {
            timestamp = DateTimeOffset.UtcNow,
            service = "mqtt-protocol-service",
            eventType = "mqtt.telemetry.received",
            message.TenantId,
            message.SiteId,
            gatewayId = message.DeviceId,
            topic = message.Topic,
            clientId = message.ClientId,
            qos = message.QoS,
            retained = message.Retained,
            payloadEncoding = message.PayloadEncoding,
            payload = ParsePayload(message),
            rawPayload = message.Payload
        }, cancellationToken);

    public Task WriteGatewayReceivedAsync(RawMqttMessage message, CancellationToken cancellationToken) =>
        _gatewayReceivedWriter.WriteAsync(new
        {
            timestamp = DateTimeOffset.UtcNow,
            service = "device-gateway-service",
            eventType = "gateway.telemetry.received",
            message.TenantId,
            message.SiteId,
            gatewayId = message.DeviceId,
            source = "mqtt",
            topic = message.Topic,
            clientId = message.ClientId,
            payloadEncoding = message.PayloadEncoding,
            payload = ParsePayload(message),
            rawPayload = message.Payload
        }, cancellationToken);

    public Task WriteGatewayProcessedAsync(CanonicalDeviceEvent deviceEvent, CancellationToken cancellationToken) =>
        _gatewayProcessedWriter.WriteAsync(new
        {
            timestamp = DateTimeOffset.UtcNow,
            service = "device-gateway-service",
            eventType = "gateway.telemetry.processed",
            deviceEvent.TenantId,
            deviceEvent.SiteId,
            gatewayId = deviceEvent.DeviceId,
            deviceEvent.Protocol,
            devices = GroupReadingsByDevice(deviceEvent)
        }, cancellationToken);

    public Task WriteGatewayErrorAsync(TelemetryError error, CancellationToken cancellationToken) =>
        _gatewayErrorWriter.WriteAsync(error, cancellationToken);

    private static object ParsePayload(RawMqttMessage message)
    {
        if (!message.PayloadEncoding.Equals("utf-8", StringComparison.OrdinalIgnoreCase))
        {
            return message.Payload;
        }

        try
        {
            using var document = JsonDocument.Parse(message.Payload);
            return document.RootElement.Clone();
        }
        catch (JsonException)
        {
            return message.Payload;
        }
    }

    private static object[] GroupReadingsByDevice(CanonicalDeviceEvent deviceEvent) =>
        deviceEvent.Readings
            .GroupBy(reading => GetMetadata(reading, "deviceKey") ?? deviceEvent.DeviceId)
            .Select(group => new
            {
                deviceKey = group.Key,
                deviceModel = GetMetadata(group.First(), "deviceModel"),
                port = GetMetadata(group.First(), "port"),
                slaveAddress = GetMetadata(group.First(), "slaveAddress"),
                functionCode = GetMetadata(group.First(), "functionCode"),
                readings = group.ToArray()
            })
            .Cast<object>()
            .ToArray();

    private static string? GetMetadata(TelemetryReading reading, string key) =>
        reading.Metadata.TryGetValue(key, out var value) ? value : null;
}
