using System.ComponentModel.DataAnnotations;

namespace NetSentinel.Api.DTOs;

public sealed record AgentHeartbeatRequest(
    [property: Required, StringLength(128)] string AgentId,
    [property: Required, StringLength(128)] string Hostname,
    [property: Required, StringLength(45)] string IpAddress,
    [property: Required, StringLength(64)] string OsType,
    bool FirewallEnabled,
    [property: Required] IReadOnlyList<PortObservationRequest> ListeningPorts,
    [property: Required] IReadOnlyList<ServiceInventoryItem> Services,
    [property: Range(0, 100)] double CpuPercent,
    [property: Range(0, 100)] double MemoryPercent,
    DateTimeOffset ReportedAtUtc);

public sealed record ServiceInventoryItem(
    [property: Required, StringLength(128)] string Name,
    [property: Required, StringLength(32)] string Status,
    [property: StringLength(64)] string? Version = null);
