using System.Globalization;
using SignalEyes.Contracts;

namespace SignalEyes.Modbus;

public sealed class ModbusValueMapper : IModbusValueMapper
{
    private readonly IModbusMappingProvider _mappingProvider;

    public ModbusValueMapper(IModbusMappingProvider mappingProvider)
    {
        _mappingProvider = mappingProvider;
    }

    public TelemetryReading Map(
        string nodeName,
        string rawValue,
        DateTimeOffset timestampUtc,
        IReadOnlyDictionary<string, string>? inheritedMetadata = null)
    {
        var metadata = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        if (inheritedMetadata is not null)
        {
            foreach (var item in inheritedMetadata)
            {
                metadata[item.Key] = item.Value;
            }
        }

        if (!_mappingProvider.TryGetByNodeName(nodeName, out var mapping))
        {
            metadata["mappingStatus"] = "unmapped";
            metadata["nodeName"] = nodeName;
            return new TelemetryReading(
                Name: nodeName,
                Value: rawValue,
                Unit: null,
                Quality: "good",
                TimestampUtc: timestampUtc,
                Metadata: metadata);
        }

        metadata["mappingStatus"] = "mapped";
        metadata["protocol"] = "modbus";
        metadata["functionCode"] = mapping.FunctionCode.ToString(CultureInfo.InvariantCulture);
        metadata["registerAddress"] = mapping.RegisterAddress.ToString(CultureInfo.InvariantCulture);
        metadata["nodeName"] = mapping.NodeName;
        metadata["dataType"] = mapping.DataType;

        var mappedValue = TryParseRawValue(rawValue, out var numeric)
            ? FormatValue(ApplyFormula(numeric, mapping.Formula))
            : rawValue;

        return new TelemetryReading(
            Name: string.IsNullOrWhiteSpace(mapping.DisplayName) ? nodeName : mapping.DisplayName,
            Value: mappedValue,
            Unit: mapping.Unit,
            Quality: "good",
            TimestampUtc: timestampUtc,
            Metadata: metadata);
    }

    private static bool TryParseRawValue(string rawValue, out decimal value) =>
        decimal.TryParse(rawValue, NumberStyles.Number, CultureInfo.InvariantCulture, out value);

    private static decimal ApplyFormula(decimal value, string? formula)
    {
        if (string.IsNullOrWhiteSpace(formula))
        {
            return value;
        }

        var normalized = formula.Replace(" ", string.Empty, StringComparison.Ordinal).ToLowerInvariant();
        if (normalized.Equals("/10", StringComparison.OrdinalIgnoreCase) ||
            normalized.Contains("divideby10", StringComparison.OrdinalIgnoreCase))
        {
            return value / 10;
        }

        if (TryParseFactor(normalized, "%s*", out var multiplyFactor) ||
            TryParseFactor(normalized, "x*", out multiplyFactor) ||
            TryParseFactor(normalized, "*", out multiplyFactor))
        {
            return value * multiplyFactor;
        }

        if (TryParseFactor(normalized, "%s/", out var divisor) ||
            TryParseFactor(normalized, "x/", out divisor) ||
            TryParseFactor(normalized, "/", out divisor))
        {
            return divisor == 0 ? value : value / divisor;
        }

        return value;
    }

    private static bool TryParseFactor(string formula, string prefix, out decimal factor)
    {
        factor = 0;
        return formula.StartsWith(prefix, StringComparison.OrdinalIgnoreCase) &&
            decimal.TryParse(
                formula[prefix.Length..],
                NumberStyles.Number,
                CultureInfo.InvariantCulture,
                out factor);
    }

    private static string FormatValue(decimal value) =>
        value == decimal.Truncate(value)
            ? value.ToString("0", CultureInfo.InvariantCulture)
            : value.ToString("0.###", CultureInfo.InvariantCulture);
}
