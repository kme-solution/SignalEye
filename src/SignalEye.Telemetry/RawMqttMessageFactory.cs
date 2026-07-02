using System.Text;
using SignalEye.Contracts;

namespace SignalEye.Telemetry;

public sealed class RawMqttMessageFactory
{
    private static readonly UTF8Encoding StrictUtf8 = new(false, true);

    public RawMqttMessage Create(
        string brokerHost,
        int brokerPort,
        string clientId,
        string topic,
        int qos,
        bool retained,
        byte[] payloadBytes,
        DateTimeOffset receivedAtUtc,
        IReadOnlyDictionary<string, string>? metadata = null)
    {
        if (!MqttTopicParser.TryParseTelemetryTopic(topic, out var topicParts))
        {
            throw new ArgumentException(
                $"MQTT topic must match {MqttTopicParser.TelemetryTopicPattern}.",
                nameof(topic));
        }

        var (payloadEncoding, payload) = DecodePayload(payloadBytes);
        var messageMetadata = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["brokerHost"] = brokerHost,
            ["brokerPort"] = brokerPort.ToString(System.Globalization.CultureInfo.InvariantCulture),
            ["clientId"] = clientId
        };

        if (metadata is not null)
        {
            foreach (var item in metadata)
            {
                messageMetadata[item.Key] = item.Value;
            }
        }

        return new RawMqttMessage(
            MessageId: Guid.NewGuid().ToString("N"),
            TenantId: topicParts.TenantId,
            SiteId: topicParts.SiteId,
            DeviceId: topicParts.DeviceId,
            BrokerHost: brokerHost,
            BrokerPort: brokerPort,
            ClientId: clientId,
            Topic: topic,
            QoS: qos,
            Retained: retained,
            ReceivedAtUtc: receivedAtUtc,
            PayloadEncoding: payloadEncoding,
            Payload: payload,
            Metadata: messageMetadata);
    }

    private static (string PayloadEncoding, string Payload) DecodePayload(byte[] payloadBytes)
    {
        if (payloadBytes.Length == 0)
        {
            return ("utf-8", string.Empty);
        }

        try
        {
            return ("utf-8", StrictUtf8.GetString(payloadBytes));
        }
        catch (DecoderFallbackException)
        {
            return ("base64", Convert.ToBase64String(payloadBytes));
        }
    }
}
