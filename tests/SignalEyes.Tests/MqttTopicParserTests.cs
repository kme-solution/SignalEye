using SignalEyes.Telemetry;
using Xunit;

namespace SignalEyes.Tests;

public sealed class MqttTopicParserTests
{
    [Fact]
    public void TryParseTelemetryTopic_accepts_tenant_site_device_topic()
    {
        var parsed = MqttTopicParser.TryParseTelemetryTopic(
            "signaleyes/acme/site-a/m2000-001/telemetry",
            out var parts);

        Assert.True(parsed);
        Assert.Equal("acme", parts.TenantId);
        Assert.Equal("site-a", parts.SiteId);
        Assert.Equal("m2000-001", parts.DeviceId);
    }

    [Fact]
    public void TryParseTelemetryTopic_rejects_legacy_devices_topic()
    {
        var parsed = MqttTopicParser.TryParseTelemetryTopic(
            "devices/m2000-001/telemetry",
            out _);

        Assert.False(parsed);
    }
}
