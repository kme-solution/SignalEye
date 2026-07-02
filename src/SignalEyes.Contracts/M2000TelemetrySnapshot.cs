namespace SignalEyes.Contracts;

public sealed record M2000TelemetrySnapshot(
    string DeviceId,
    decimal? GridVoltage,
    decimal? BatteryVoltage,
    decimal? LoadCurrent,
    decimal? CabinetTemperature,
    DateTimeOffset ReceivedAt);
