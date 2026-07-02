using SignalEyes.Contracts;

namespace SignalEyes.Telemetry;

public sealed record DecodedPayload(
    string PayloadText,
    IReadOnlyList<TelemetryReading> Readings,
    IReadOnlyDictionary<string, string> Metadata);
