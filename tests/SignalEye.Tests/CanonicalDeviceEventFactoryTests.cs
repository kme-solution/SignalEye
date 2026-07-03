using System.Text;
using SignalEye.Modbus;
using SignalEye.Telemetry;
using Xunit;

namespace SignalEye.Tests;

public sealed class CanonicalDeviceEventFactoryTests
{
    [Fact]
    public void Create_maps_object_metrics_to_modbus_readings()
    {
        var rawMessage = new RawMqttMessageFactory().Create(
            "localhost",
            1883,
            "client",
            "signaleye/acme/site-a/m2000-001/telemetry",
            1,
            false,
            Encoding.UTF8.GetBytes("{\"m\":{\"node08\":541}}"),
            DateTimeOffset.UnixEpoch);
        var mapper = new ModbusValueMapper(new CsvModbusMappingProvider("config/modbus/edge-EN.csv"));
        var decoder = new PayloadDecoder(mapper);
        var factory = new CanonicalDeviceEventFactory(decoder);

        var deviceEvent = factory.Create(rawMessage);

        Assert.Equal("acme", deviceEvent.TenantId);
        Assert.Equal("site-a", deviceEvent.SiteId);
        Assert.Equal("m2000-001", deviceEvent.DeviceId);
        Assert.Equal("modbus", deviceEvent.Protocol);
        Assert.Single(deviceEvent.Readings);
        Assert.Equal("Rectifier Bus Voltage", deviceEvent.Readings[0].Name);
        Assert.Equal("54.1", deviceEvent.Readings[0].Value);
    }

    [Fact]
    public void Create_maps_grouped_device_metrics_and_preserves_device_context()
    {
        var rawMessage = new RawMqttMessageFactory().Create(
            "localhost",
            1883,
            "m100-001",
            "signaleye/acme/site-a/m100-001/telemetry",
            1,
            false,
            Encoding.UTF8.GetBytes("{\"device01\":{\"p\":1,\"s\":1,\"d\":\"m2000\",\"fc\":4,\"m\":{\"node08\":541}}}"),
            DateTimeOffset.UnixEpoch);
        var mapper = new ModbusValueMapper(new CsvModbusMappingProvider("config/modbus/edge-EN.csv"));
        var factory = new CanonicalDeviceEventFactory(new PayloadDecoder(mapper));

        var deviceEvent = factory.Create(rawMessage);

        Assert.Equal("m100-001", deviceEvent.DeviceId);
        Assert.Single(deviceEvent.Readings);
        Assert.Equal("device01", deviceEvent.Readings[0].Metadata["deviceKey"]);
        Assert.Equal("m2000", deviceEvent.Readings[0].Metadata["deviceModel"]);
        Assert.Equal("1", deviceEvent.Readings[0].Metadata["port"]);
        Assert.Equal("1", deviceEvent.Readings[0].Metadata["slaveAddress"]);
        Assert.Equal("4", deviceEvent.Readings[0].Metadata["functionCode"]);
    }
}
