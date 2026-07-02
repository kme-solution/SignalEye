using System.Text.Json;
using SignalEyes.Contracts;

namespace SignalEyes.Infrastructure;

public sealed class FileTelemetryLogger
{
    private readonly string _logDirectory;

    public FileTelemetryLogger(string logDirectory)
    {
        _logDirectory = logDirectory;
        Directory.CreateDirectory(_logDirectory);
    }

    public async Task WriteAsync(TelemetryMessage message, CancellationToken cancellationToken)
    {
        var file = Path.Combine(_logDirectory, $"telemetry-{DateTime.UtcNow:yyyyMMdd}.log");
        var json = JsonSerializer.Serialize(message);
        await File.AppendAllTextAsync(file, json + Environment.NewLine, cancellationToken);
    }
}
