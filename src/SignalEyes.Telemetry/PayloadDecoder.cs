using System.Text.Json;
using SignalEyes.Contracts;
using SignalEyes.Modbus;

namespace SignalEyes.Telemetry;

public sealed class PayloadDecoder
{
    private readonly IModbusValueMapper _modbusValueMapper;
    private readonly TimeProvider _timeProvider;

    public PayloadDecoder(IModbusValueMapper modbusValueMapper, TimeProvider? timeProvider = null)
    {
        _modbusValueMapper = modbusValueMapper;
        _timeProvider = timeProvider ?? TimeProvider.System;
    }

    public DecodedPayload Decode(RawMqttMessage rawMessage)
    {
        if (!rawMessage.PayloadEncoding.Equals("utf-8", StringComparison.OrdinalIgnoreCase))
        {
            return new DecodedPayload(rawMessage.Payload, [], new Dictionary<string, string>
            {
                ["payloadEncoding"] = rawMessage.PayloadEncoding,
                ["decoder"] = "binary"
            });
        }

        if (string.IsNullOrWhiteSpace(rawMessage.Payload))
        {
            return new DecodedPayload(string.Empty, [], new Dictionary<string, string>
            {
                ["payloadEncoding"] = rawMessage.PayloadEncoding,
                ["decoder"] = "empty"
            });
        }

        return TryDecodeJsonTelemetry(rawMessage.Payload, out var readings, out var metadata)
            ? new DecodedPayload(rawMessage.Payload, readings, metadata)
            : new DecodedPayload(rawMessage.Payload, [], new Dictionary<string, string>
            {
                ["payloadEncoding"] = rawMessage.PayloadEncoding,
                ["decoder"] = "raw-text"
            });
    }

    private bool TryDecodeJsonTelemetry(
        string payloadText,
        out IReadOnlyList<TelemetryReading> readings,
        out IReadOnlyDictionary<string, string> metadata)
    {
        readings = [];
        metadata = new Dictionary<string, string>();

        try
        {
            using var document = JsonDocument.Parse(payloadText);
            if (document.RootElement.ValueKind != JsonValueKind.Object)
            {
                return false;
            }

            var readingList = new List<TelemetryReading>();
            var meta = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            var timestampUtc = _timeProvider.GetUtcNow();

            foreach (var property in document.RootElement.EnumerateObject())
            {
                if (property.NameEquals("m"))
                {
                    ExtractMetrics(property.Value, readingList, timestampUtc, meta);
                    continue;
                }

                if (property.Value.ValueKind is JsonValueKind.String or JsonValueKind.Number or JsonValueKind.True or JsonValueKind.False)
                {
                    meta[property.Name] = property.Value.ToString();
                }
            }

            readings = readingList;
            metadata = meta;
            return readingList.Count > 0 || meta.Count > 0;
        }
        catch (JsonException)
        {
            return false;
        }
    }

    private void ExtractMetrics(
        JsonElement metricsElement,
        ICollection<TelemetryReading> readings,
        DateTimeOffset timestampUtc,
        IReadOnlyDictionary<string, string> payloadMetadata)
    {
        if (metricsElement.ValueKind != JsonValueKind.Object)
        {
            return;
        }

        foreach (var property in metricsElement.EnumerateObject())
        {
            var rawValue = property.Value.ToString();
            var reading = _modbusValueMapper.Map(property.Name, rawValue, timestampUtc, payloadMetadata);
            readings.Add(reading);
        }
    }
}
