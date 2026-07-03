using System.Text.Json;
using SignalEye.Contracts;
using SignalEye.Modbus;

namespace SignalEye.Telemetry;

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

            // Legacy payload: { "m": { ... } }
            if (document.RootElement.TryGetProperty("m", out var legacyMetrics))
            {
                foreach (var property in document.RootElement.EnumerateObject())
                {
                    if (!property.NameEquals("m") && IsScalar(property.Value))
                    {
                        meta[property.Name] = property.Value.ToString();
                    }
                }

                ExtractMetrics(legacyMetrics, readingList, timestampUtc, meta);
                readings = readingList;
                metadata = meta;
                return readingList.Count > 0 || meta.Count > 0;
            }

            foreach (var property in document.RootElement.EnumerateObject())
            {
                if (property.Value.ValueKind == JsonValueKind.Object &&
                    property.Value.TryGetProperty("m", out var deviceMetrics))
                {
                    var deviceMetadata = ExtractDeviceMetadata(property.Name, property.Value);
                    ExtractMetrics(deviceMetrics, readingList, timestampUtc, deviceMetadata);
                    continue;
                }

                if (IsScalar(property.Value))
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

    private static IReadOnlyDictionary<string, string> ExtractDeviceMetadata(
        string deviceKey,
        JsonElement deviceElement)
    {
        var metadata = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["deviceKey"] = deviceKey
        };

        foreach (var property in deviceElement.EnumerateObject())
        {
            if (property.NameEquals("m") || !IsScalar(property.Value))
            {
                continue;
            }

            metadata[property.Name switch
            {
                "d" => "deviceModel",
                "p" => "port",
                "s" => "slaveAddress",
                "fc" => "functionCode",
                _ => property.Name
            }] = property.Value.ToString();
        }

        return metadata;
    }

    private static bool IsScalar(JsonElement element) =>
        element.ValueKind is JsonValueKind.String or JsonValueKind.Number or JsonValueKind.True or JsonValueKind.False;

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
