using NetSentinel.Api.DTOs;
using NetSentinel.Api.Services;

namespace NetSentinel.Api.Tests;

public sealed class AgentHeartbeatServiceTests
{
    [Fact]
    public async Task RecordAsync_PersistsTelemetryAndAgentPort()
    {
        await using var context = TestContextFactory.CreateContext();
        var service = new AgentHeartbeatService(context);
        var request = new AgentHeartbeatRequest(
            "agent-01",
            "lab-host",
            "192.168.56.20",
            "Windows Server 2022",
            true,
            [new PortObservationRequest(3389, "TCP", "rdp")],
            [new ServiceInventoryItem("Windows Defender", "Running", "simulated")],
            15.5,
            42.0,
            DateTimeOffset.UtcNow);

        var heartbeat = await service.RecordAsync(request);

        Assert.NotEqual(Guid.Empty, heartbeat.Id);
        Assert.Single(context.AgentHeartbeats);
        Assert.Single(context.OpenPorts);
        Assert.True(heartbeat.FirewallEnabled);
    }

    [Fact]
    public async Task RecordAsync_RejectsInvalidAddress()
    {
        await using var context = TestContextFactory.CreateContext();
        var service = new AgentHeartbeatService(context);
        var request = new AgentHeartbeatRequest(
            "agent-01",
            "lab-host",
            "invalid",
            "Linux",
            true,
            [],
            [],
            10,
            20,
            DateTimeOffset.UtcNow);

        await Assert.ThrowsAsync<ArgumentException>(() => service.RecordAsync(request));
    }
}
