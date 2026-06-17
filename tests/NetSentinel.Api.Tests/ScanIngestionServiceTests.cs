using NetSentinel.Api.DTOs;
using NetSentinel.Api.Services;

namespace NetSentinel.Api.Tests;

public sealed class ScanIngestionServiceTests
{
    [Fact]
    public async Task IngestAsync_CreatesDeviceAndOpenPort()
    {
        await using var context = TestContextFactory.CreateContext();
        var service = new ScanIngestionService(context);
        var now = DateTimeOffset.UtcNow;
        var request = new ScanReportRequest(
            "127.0.0.1",
            now.AddSeconds(-1),
            now,
            [
                new ScanHostRequest(
                    "localhost",
                    "127.0.0.1",
                    "Unknown",
                    [new PortObservationRequest(8080, "TCP", "http-alt")])
            ]);

        var scan = await service.IngestAsync(request);

        Assert.Equal(1, scan.HostsDiscovered);
        Assert.Single(context.Devices);
        Assert.Single(context.OpenPorts);
    }

    [Fact]
    public async Task IngestAsync_RejectsReversedTimestamps()
    {
        await using var context = TestContextFactory.CreateContext();
        var service = new ScanIngestionService(context);
        var now = DateTimeOffset.UtcNow;
        var request = new ScanReportRequest("localhost", now, now.AddSeconds(-1), []);

        await Assert.ThrowsAsync<ArgumentException>(() => service.IngestAsync(request));
    }
}
