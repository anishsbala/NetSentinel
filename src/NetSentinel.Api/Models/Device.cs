namespace NetSentinel.Api.Models;

public sealed class Device
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string Hostname { get; set; }
    public required string IpAddress { get; set; }
    public required string OsType { get; set; }
    public DateTimeOffset FirstSeenAtUtc { get; set; }
    public DateTimeOffset LastSeenAtUtc { get; set; }

    public ICollection<OpenPort> OpenPorts { get; set; } = [];
    public ICollection<AgentHeartbeat> Heartbeats { get; set; } = [];
    public ICollection<FirewallRule> FirewallRules { get; set; } = [];
}
