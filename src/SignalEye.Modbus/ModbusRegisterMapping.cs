namespace SignalEye.Modbus;

public sealed record ModbusRegisterMapping(
    string NodeName,
    int FunctionCode,
    int RegisterAddress,
    string DataType,
    string? Unit,
    string? DisplayName,
    string? Formula);
