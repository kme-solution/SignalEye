using System.Text;
using SignalEyes.Modbus;
using SignalEyes.Telemetry;
using Xunit;

namespace SignalEyes.Tests;

public sealed class CanonicalDeviceEventFactoryTests
{
    [Fact]
    public void Create_maps_object_metrics_to_modbus_readings()
    {
        var rawMessage = new RawMqttMessageFactory().Create(
            "localhost",
            1883,
            "client",
            "signaleyes/acme/site-a/m2000-001/telemetry",
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
}
