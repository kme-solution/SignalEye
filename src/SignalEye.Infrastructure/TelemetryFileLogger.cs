using SignalEye.Contracts;

namespace SignalEye.Infrastructure;

public sealed class TelemetryFileLogger
{
    private readonly JsonLineFileWriter _mqttTelemetryWriter;
    private readonly JsonLineFileWriter _gatewayReceivedWriter;
    private readonly JsonLineFileWriter _gatewayProcessedWriter;
    private readonly JsonLineFileWriter _gatewayErrorWriter;

    public TelemetryFileLogger(string rootDirectory)
    {
        _mqttTelemetryWriter = new JsonLineFileWriter(
            Path.Combine(rootDirectory, "mqtt-protocol-service"),
            "mqtt-telemetry");
        _gatewayReceivedWriter = new JsonLineFileWriter(
            Path.Combine(rootDirectory, "device-gateway-service"),
            "gateway-received");
        _gatewayProcessedWriter = new JsonLineFileWriter(
            Path.Combine(rootDirectory, "device-gateway-service"),
            "gateway-processed");
        _gatewayErrorWriter = new JsonLineFileWriter(
            Path.Combine(rootDirectory, "device-gateway-service"),
            "gateway-error");
    }

    public Task WriteMqttTelemetryAsync(RawMqttMessage message, CancellationToken cancellationToken) =>
        _mqttTelemetryWriter.WriteAsync(new
        {
            timestamp = DateTimeOffset.UtcNow,
            service = "mqtt-protocol-service",
            eventType = "mqtt.telemetry.received",
            message.TenantId,
            message.SiteId,
            message.DeviceId,
            topic = message.Topic,
            qos = message.QoS,
            retained = message.Retained,
            payloadEncoding = message.PayloadEncoding,
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
            message.DeviceId,
            source = "mqtt",
            topic = message.Topic,
            payloadEncoding = message.PayloadEncoding,
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
            deviceEvent.DeviceId,
            deviceType = deviceEvent.Metadata.TryGetValue("deviceType", out var deviceType) ? deviceType : "m2000",
            deviceEvent.Protocol,
            readings = deviceEvent.Readings
        }, cancellationToken);

    public Task WriteGatewayErrorAsync(TelemetryError error, CancellationToken cancellationToken) =>
        _gatewayErrorWriter.WriteAsync(error, cancellationToken);
}
