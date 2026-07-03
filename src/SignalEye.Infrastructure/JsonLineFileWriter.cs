using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Encodings.Web;
using System.Collections.Concurrent;

namespace SignalEye.Infrastructure;

public sealed class JsonLineFileWriter
{
    public const long DefaultMaxDirectorySizeBytes = 1024L * 1024L * 1024L;
    public static readonly TimeSpan DefaultRetentionAge = TimeSpan.FromDays(7);

    private static readonly ConcurrentDictionary<string, SemaphoreSlim> DirectoryLocks =
        new(StringComparer.OrdinalIgnoreCase);

    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        WriteIndented = false
    };

    private readonly string _directory;
    private readonly string _filePrefix;
    private readonly long _maxDirectorySizeBytes;
    private readonly TimeSpan _retentionAge;
    private readonly SemaphoreSlim _directoryLock;

    public JsonLineFileWriter(
        string directory,
        string filePrefix,
        long maxDirectorySizeBytes = DefaultMaxDirectorySizeBytes,
        TimeSpan? retentionAge = null)
    {
        _directory = directory;
        _filePrefix = filePrefix;
        _maxDirectorySizeBytes = maxDirectorySizeBytes > 0
            ? maxDirectorySizeBytes
            : throw new ArgumentOutOfRangeException(nameof(maxDirectorySizeBytes));
        _retentionAge = retentionAge ?? DefaultRetentionAge;
        if (_retentionAge <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(retentionAge));
        }

        Directory.CreateDirectory(_directory);
        _directoryLock = DirectoryLocks.GetOrAdd(Path.GetFullPath(_directory), _ => new SemaphoreSlim(1, 1));
    }

    public async Task WriteAsync<T>(T value, CancellationToken cancellationToken)
    {
        var json = JsonSerializer.Serialize(value, SerializerOptions);
        var line = json + Environment.NewLine;
        var lineSize = System.Text.Encoding.UTF8.GetByteCount(line);

        await _directoryLock.WaitAsync(cancellationToken);
        try
        {
            DeleteExpiredFiles();
            var path = SelectWritablePath(lineSize);
            await File.AppendAllTextAsync(path, line, cancellationToken);
            EnforceDirectorySize(path);
        }
        finally
        {
            _directoryLock.Release();
        }
    }

    private string SelectWritablePath(int nextLineSize)
    {
        var datePrefix = $"{_filePrefix}-{DateTime.UtcNow:yyyyMMdd}";
        for (var sequence = 0; ; sequence++)
        {
            var suffix = sequence == 0 ? string.Empty : $"-{sequence:000}";
            var path = Path.Combine(_directory, $"{datePrefix}{suffix}.log");
            var currentSize = File.Exists(path) ? new FileInfo(path).Length : 0;
            if (currentSize == 0 || currentSize + nextLineSize <= _maxDirectorySizeBytes)
            {
                return path;
            }
        }
    }

    private void DeleteExpiredFiles()
    {
        var cutoffUtc = DateTime.UtcNow - _retentionAge;
        foreach (var file in EnumerateLogFiles().Where(file => file.LastWriteTimeUtc < cutoffUtc))
        {
            file.Delete();
        }
    }

    private void EnforceDirectorySize(string activePath)
    {
        var files = EnumerateLogFiles()
            .OrderBy(file => file.LastWriteTimeUtc)
            .ToList();
        var totalSize = files.Sum(file => file.Length);

        foreach (var file in files)
        {
            if (totalSize <= _maxDirectorySizeBytes)
            {
                break;
            }

            if (Path.GetFullPath(file.FullName).Equals(Path.GetFullPath(activePath), StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            totalSize -= file.Length;
            file.Delete();
        }
    }

    private IEnumerable<FileInfo> EnumerateLogFiles() =>
        new DirectoryInfo(_directory).EnumerateFiles("*.log", SearchOption.TopDirectoryOnly);
}
