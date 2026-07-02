using SignalEye.Modbus;
using Xunit;

namespace SignalEye.Tests;

public sealed class ModbusMappingTests
{
    private const string MappingPath = "config/modbus/edge-EN.csv";

    [Fact]
    public void CsvModbusMappingProvider_loads_active_node_subset()
    {
        var provider = new CsvModbusMappingProvider(MappingPath);

        var found = provider.TryGetByNodeName("node08", out var mapping);

        Assert.True(found);
        Assert.Equal(4, mapping.FunctionCode);
        Assert.Equal(8, mapping.RegisterAddress);
        Assert.Equal("uint16", mapping.DataType);
        Assert.Equal("Rectifier Bus Voltage", mapping.DisplayName);
        Assert.Equal("Volt", mapping.Unit);
        Assert.Equal("/10", mapping.Formula);
    }

    [Fact]
    public void ModbusValueMapper_applies_formula_and_metadata()
    {
        var mapper = new ModbusValueMapper(new CsvModbusMappingProvider(MappingPath));

        var reading = mapper.Map("node08", "541", DateTimeOffset.UnixEpoch);

        Assert.Equal("Rectifier Bus Voltage", reading.Name);
        Assert.Equal("54.1", reading.Value);
        Assert.Equal("Volt", reading.Unit);
        Assert.Equal("modbus", reading.Metadata["protocol"]);
        Assert.Equal("8", reading.Metadata["registerAddress"]);
        Assert.Equal("node08", reading.Metadata["nodeName"]);
    }

    [Fact]
    public void ModbusValueMapper_keeps_unmapped_nodes()
    {
        var mapper = new ModbusValueMapper(new CsvModbusMappingProvider(MappingPath));

        var reading = mapper.Map("unknown", "123", DateTimeOffset.UnixEpoch);

        Assert.Equal("unknown", reading.Name);
        Assert.Equal("123", reading.Value);
        Assert.Equal("unmapped", reading.Metadata["mappingStatus"]);
    }
}
