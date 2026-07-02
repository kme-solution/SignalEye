using System.Text;
using SignalEye.Telemetry;
using Xunit;

namespace SignalEye.Tests;

public sealed class RawMqttMessageFactoryTests
{
    private readonly RawMqttMessageFactory _factory = new();

    [Fact]
    public void Create_preserves_utf8_payload_as_text()
    {
        var message = _factory.Create(
            "localhost",
            1883,
            "client",
            "signaleye/acme/site-a/m2000-001/telemetry",
            1,
            false,
            Encoding.UTF8.GetBytes("{\"m\":{\"node08\":541}}"),
            DateTimeOffset.UnixEpoch);

        Assert.Equal("utf-8", message.PayloadEncoding);
        Assert.Equal("{\"m\":{\"node08\":541}}", message.Payload);
        Assert.Equal("acme", message.TenantId);
    }

    [Fact]
    public void Create_encodes_non_utf8_payload_as_base64()
    {
        var message = _factory.Create(
            "localhost",
            1883,
            "client",
            "signaleye/acme/site-a/m2000-001/telemetry",
            1,
            false,
            [0xff, 0xfe],
            DateTimeOffset.UnixEpoch);

        Assert.Equal("base64", message.PayloadEncoding);
        Assert.Equal("//4=", message.Payload);
    }
}
