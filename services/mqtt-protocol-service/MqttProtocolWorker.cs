using SignalEyes.Telemetry;

public sealed class MqttProtocolWorker : BackgroundService
{
    private readonly ILogger<MqttProtocolWorker> _logger;
    private readonly TelemetryNormalizer _normalizer;

    public MqttProtocolWorker(ILogger<MqttProtocolWorker> logger, TelemetryNormalizer normalizer)
    {
        _logger = logger;
        _normalizer = normalizer;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("MQTT protocol service started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            // Replace this simulation with the production MQTT client subscription.
            var message = _normalizer.NormalizeMqttPayload("signaleyes/m100/demo-device", "{\"status\":\"online\"}");
            _logger.LogInformation("Received MQTT telemetry from {DeviceId}: {Payload}", message.DeviceId, message.RawPayload);

            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
        }
    }
}
