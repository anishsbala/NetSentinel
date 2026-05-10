namespace NetSentinel.Api.Models;

public sealed class AgentHeartbeat
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid DeviceId { get; set; }
    public required string AgentId { get; set; }
    public bool FirewallEnabled { get; set; }
    public required string ListeningPortsJson { get; set; }
    public required string ServicesJson { get; set; }
    public double CpuPercent { get; set; }
    public double MemoryPercent { get; set; }
    public DateTimeOffset ReportedAtUtc { get; set; }
    public DateTimeOffset ReceivedAtUtc { get; set; }

    public Device Device { get; set; } = null!;
}
