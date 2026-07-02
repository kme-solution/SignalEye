using System.Globalization;

namespace SignalEyes.Modbus;

public sealed class CsvModbusMappingProvider : IModbusMappingProvider
{
    private readonly Lazy<IReadOnlyDictionary<string, ModbusRegisterMapping>> _mappings;

    public CsvModbusMappingProvider(string mappingPath)
    {
        MappingPath = ResolveMappingPath(mappingPath);
        _mappings = new Lazy<IReadOnlyDictionary<string, ModbusRegisterMapping>>(LoadMappings);
    }

    public string MappingPath { get; }

    public bool TryGetByNodeName(string nodeName, out ModbusRegisterMapping mapping) =>
        _mappings.Value.TryGetValue(nodeName, out mapping!);

    private IReadOnlyDictionary<string, ModbusRegisterMapping> LoadMappings()
    {
        if (!File.Exists(MappingPath))
        {
            return new Dictionary<string, ModbusRegisterMapping>(StringComparer.OrdinalIgnoreCase);
        }

        using var reader = File.OpenText(MappingPath);
        var headerLine = reader.ReadLine();
        if (string.IsNullOrWhiteSpace(headerLine))
        {
            return new Dictionary<string, ModbusRegisterMapping>(StringComparer.OrdinalIgnoreCase);
        }

        var headers = ParseCsvLine(headerLine);
        var mappings = new Dictionary<string, ModbusRegisterMapping>(StringComparer.OrdinalIgnoreCase);
        string? line;
        while ((line = reader.ReadLine()) is not null)
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            var values = ParseCsvLine(line);
            var row = BuildRow(headers, values);
            var nodeName = GetValue(row, "Node-name");
            if (string.IsNullOrWhiteSpace(nodeName))
            {
                continue;
            }

            var functionCode = ParseInt(GetValue(row, "Function-code"));
            var registerAddress = ParseInt(GetValue(row, "Register-Address"));
            var dataType = GetValue(row, "Data-type");
            if (functionCode is null || registerAddress is null || string.IsNullOrWhiteSpace(dataType))
            {
                continue;
            }

            mappings[nodeName] = new ModbusRegisterMapping(
                NodeName: nodeName,
                FunctionCode: functionCode.Value,
                RegisterAddress: registerAddress.Value,
                DataType: dataType,
                Unit: NullIfBlank(GetValue(row, "Unit")),
                DisplayName: NullIfBlank(GetValue(row, "Display-name")) ?? NullIfBlank(GetValue(row, "Parameter")),
                Formula: NullIfBlank(GetValue(row, "Formula")));
        }

        return mappings;
    }

    private static Dictionary<string, string> BuildRow(IReadOnlyList<string> headers, IReadOnlyList<string> values)
    {
        var row = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        for (var index = 0; index < headers.Count; index++)
        {
            if (string.IsNullOrWhiteSpace(headers[index]))
            {
                continue;
            }

            var key = row.ContainsKey(headers[index])
                ? $"{headers[index]}#{index.ToString(CultureInfo.InvariantCulture)}"
                : headers[index];
            row[key] = index < values.Count ? values[index] : string.Empty;
        }

        return row;
    }

    private static string GetValue(IReadOnlyDictionary<string, string> row, string key) =>
        row.TryGetValue(key, out var value) ? value.Trim() : string.Empty;

    private static string? NullIfBlank(string value) =>
        string.IsNullOrWhiteSpace(value) || value.Equals("N/A", StringComparison.OrdinalIgnoreCase)
            ? null
            : value.Trim();

    private static int? ParseInt(string value) =>
        int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed)
            ? parsed
            : null;

    private static string ResolveMappingPath(string mappingPath)
    {
        if (Path.IsPathRooted(mappingPath) || File.Exists(mappingPath))
        {
            return mappingPath;
        }

        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            var candidate = Path.Combine(directory.FullName, mappingPath);
            if (File.Exists(candidate))
            {
                return candidate;
            }

            directory = directory.Parent;
        }

        return mappingPath;
    }

    private static IReadOnlyList<string> ParseCsvLine(string line)
    {
        var values = new List<string>();
        var current = new System.Text.StringBuilder();
        var inQuotes = false;

        for (var index = 0; index < line.Length; index++)
        {
            var character = line[index];
            if (character == '"')
            {
                if (inQuotes && index + 1 < line.Length && line[index + 1] == '"')
                {
                    current.Append('"');
                    index++;
                    continue;
                }

                inQuotes = !inQuotes;
                continue;
            }

            if (character == ',' && !inQuotes)
            {
                values.Add(current.ToString());
                current.Clear();
                continue;
            }

            current.Append(character);
        }

        values.Add(current.ToString());
        return values;
    }
}
