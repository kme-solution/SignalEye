using SignalEye.Contracts;

namespace SignalEye.Modbus;

public interface IModbusValueMapper
{
    TelemetryReading Map(
        string nodeName,
        string rawValue,
        DateTimeOffset timestampUtc,
        IReadOnlyDictionary<string, string>? inheritedMetadata = null);
}
