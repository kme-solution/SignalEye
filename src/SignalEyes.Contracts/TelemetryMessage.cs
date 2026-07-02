namespace SignalEyes.Contracts;

public sealed record TelemetryMessage(
    string DeviceId,
    string Source,
    DateTimeOffset ReceivedAt,
    string PayloadType,
    string RawPayload);
