namespace SignalEyes.Modbus;

public interface IModbusMappingProvider
{
    bool TryGetByNodeName(string nodeName, out ModbusRegisterMapping mapping);
}
