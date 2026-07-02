using SignalEyes.Contracts;

namespace SignalEyes.Modbus;

public sealed class M2000InputRegisterParser
{
    public M2000TelemetrySnapshot Parse(string deviceId, IReadOnlyList<ushort> registers, DateTimeOffset receivedAt)
    {
        return new M2000TelemetrySnapshot(
            DeviceId: deviceId,
            GridVoltage: ReadScaled(registers, M2000InputRegisterMap.GridVoltage, 10),
            BatteryVoltage: ReadScaled(registers, M2000InputRegisterMap.BatteryVoltage, 10),
            LoadCurrent: ReadScaled(registers, M2000InputRegisterMap.LoadCurrent, 10),
            CabinetTemperature: ReadScaled(registers, M2000InputRegisterMap.CabinetTemperature, 10),
            ReceivedAt: receivedAt);
    }

    private static decimal? ReadScaled(IReadOnlyList<ushort> registers, int index, decimal scale)
    {
        if (index < 0 || index >= registers.Count)
        {
            return null;
        }

        return registers[index] / scale;
    }
}
