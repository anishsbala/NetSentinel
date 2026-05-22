namespace NetSentinel.Api.DTOs;

public sealed record AgentHeartbeatResponse(
    Guid Id,
    Guid DeviceId,
    string AgentId,
    string Hostname,
    string IpAddress,
    string OsType,
    bool FirewallEnabled,
    IReadOnlyList<PortObservationRequest> ListeningPorts,
    IReadOnlyList<ServiceInventoryItem> Services,
    double CpuPercent,
    double MemoryPercent,
    DateTimeOffset ReportedAtUtc,
    DateTimeOffset ReceivedAtUtc);
