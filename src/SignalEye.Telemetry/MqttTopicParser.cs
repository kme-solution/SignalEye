namespace SignalEye.Telemetry;

public static class MqttTopicParser
{
    public const string TelemetryTopicPattern = "signaleye/{tenantId}/{siteId}/{deviceId}/telemetry";

    public static bool TryParseTelemetryTopic(string topic, out MqttTopicParts parts)
    {
        parts = new MqttTopicParts(string.Empty, string.Empty, string.Empty);

        var segments = topic.Split('/', StringSplitOptions.TrimEntries);
        if (segments.Length != 5 ||
            !segments[0].Equals("signaleye", StringComparison.OrdinalIgnoreCase) ||
            !segments[4].Equals("telemetry", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(segments[1]) ||
            string.IsNullOrWhiteSpace(segments[2]) ||
            string.IsNullOrWhiteSpace(segments[3]))
        {
            return false;
        }

        parts = new MqttTopicParts(segments[1], segments[2], segments[3]);
        return true;
    }
}
