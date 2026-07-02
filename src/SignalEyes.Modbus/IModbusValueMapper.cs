using SignalEyes.Contracts;

namespace SignalEyes.Modbus;

public interface IModbusValueMapper
{
    TelemetryReading Map(
        string nodeName,
        string rawValue,
        DateTimeOffset timestampUtc,
        IReadOnlyDictionary<string, string>? inheritedMetadata = null);
}
