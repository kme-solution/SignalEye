using System.Text.Json;
using System.Text.Json.Serialization;

namespace SignalEyes.Infrastructure;

public sealed class JsonLineFileWriter
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    private readonly string _directory;
    private readonly string _filePrefix;

    public JsonLineFileWriter(string directory, string filePrefix)
    {
        _directory = directory;
        _filePrefix = filePrefix;
        Directory.CreateDirectory(_directory);
    }

    public async Task WriteAsync<T>(T value, CancellationToken cancellationToken)
    {
        var fileName = $"{_filePrefix}-{DateTime.UtcNow:yyyyMMdd}.log";
        var path = Path.Combine(_directory, fileName);
        var json = JsonSerializer.Serialize(value, SerializerOptions);
        await File.AppendAllTextAsync(path, json + Environment.NewLine, cancellationToken);
    }
}
