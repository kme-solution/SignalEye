using SignalEyes.Contracts;
using SignalEyes.Infrastructure;
using SignalEyes.Modbus;

public sealed class DeviceGatewayWorker : BackgroundService
{
    private readonly ILogger<DeviceGatewayWorker> _logger;
    private readonly FileTelemetryLogger _fileLogger;
    private readonly M2000InputRegisterParser _m2000Parser;

    public DeviceGatewayWorker(
        ILogger<DeviceGatewayWorker> logger,
        FileTelemetryLogger fileLogger,
        M2000InputRegisterParser m2000Parser)
    {
        _logger = logger;
        _fileLogger = fileLogger;
        _m2000Parser = m2000Parser;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Device gateway service started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            // Replace this simulation with the production queue consumer.
            var message = new TelemetryMessage(
                DeviceId: "demo-m2000",
                Source: "gateway",
                ReceivedAt: DateTimeOffset.UtcNow,
                PayloadType: "m2000-input-registers",
                RawPayload: "2301,521,125,330");

            await _fileLogger.WriteAsync(message, stoppingToken);

            var snapshot = _m2000Parser.Parse(message.DeviceId, [2301, 521, 125, 330], message.ReceivedAt);
            _logger.LogInformation(
                "M2000 telemetry {DeviceId}: grid={GridVoltage}, battery={BatteryVoltage}, load={LoadCurrent}, temp={CabinetTemperature}",
                snapshot.DeviceId,
                snapshot.GridVoltage,
                snapshot.BatteryVoltage,
                snapshot.LoadCurrent,
                snapshot.CabinetTemperature);

            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
        }
    }
}
