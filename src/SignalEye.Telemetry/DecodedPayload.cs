using SignalEye.Contracts;

namespace SignalEye.Telemetry;

public sealed record DecodedPayload(
    string PayloadText,
    IReadOnlyList<TelemetryReading> Readings,
    IReadOnlyDictionary<string, string> Metadata);
