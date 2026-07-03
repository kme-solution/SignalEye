using SignalEye.Infrastructure;
using Xunit;

namespace SignalEye.Tests;

public sealed class JsonLineFileWriterTests
{
    [Fact]
    public async Task WriteAsync_rotates_and_enforces_directory_size()
    {
        var directory = CreateTemporaryDirectory();
        try
        {
            var writer = new JsonLineFileWriter(directory, "telemetry", 180, TimeSpan.FromDays(7));
            var value = new { payload = new string('x', 100) };

            await writer.WriteAsync(value, CancellationToken.None);
            await writer.WriteAsync(value, CancellationToken.None);

            var files = Directory.GetFiles(directory, "*.log");
            Assert.Single(files);
            Assert.EndsWith("-001.log", files[0]);
            Assert.True(new FileInfo(files[0]).Length <= 180);
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    [Fact]
    public async Task WriteAsync_deletes_files_older_than_retention_age()
    {
        var directory = CreateTemporaryDirectory();
        try
        {
            var expiredPath = Path.Combine(directory, "telemetry-expired.log");
            await File.WriteAllTextAsync(expiredPath, "expired");
            File.SetLastWriteTimeUtc(expiredPath, DateTime.UtcNow.AddDays(-8));
            var writer = new JsonLineFileWriter(directory, "telemetry", 1024, TimeSpan.FromDays(7));

            await writer.WriteAsync(new { value = 1 }, CancellationToken.None);

            Assert.False(File.Exists(expiredPath));
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    private static string CreateTemporaryDirectory()
    {
        var path = Path.Combine(Path.GetTempPath(), $"signaleye-tests-{Guid.NewGuid():N}");
        Directory.CreateDirectory(path);
        return path;
    }
}
