namespace SignalEye.Modbus;

public interface IModbusMappingProvider
{
    bool TryGetByNodeName(string nodeName, out ModbusRegisterMapping mapping);

    bool TryGetByProfileAndNodeName(
        string profile,
        string nodeName,
        out ModbusRegisterMapping mapping);
}
