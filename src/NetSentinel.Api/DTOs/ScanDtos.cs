using System.ComponentModel.DataAnnotations;

namespace NetSentinel.Api.DTOs;

public sealed record ScanReportRequest(
    [property: Required, StringLength(128)] string Target,
    DateTimeOffset StartedAtUtc,
    DateTimeOffset CompletedAtUtc,
    [property: Required] IReadOnlyList<ScanHostRequest> Hosts,
    string? ErrorMessage = null);

public sealed record ScanHostRequest(
    [property: Required, StringLength(128)] string Hostname,
    [property: Required, StringLength(45)] string IpAddress,
    [property: StringLength(64)] string OsType,
    [property: Required] IReadOnlyList<PortObservationRequest> OpenPorts);

public sealed record PortObservationRequest(
    [property: Range(1, 65535)] int PortNumber,
    [property: StringLength(32)] string Protocol = "TCP",
    [property: StringLength(128)] string? ServiceName = null);

public sealed record ScanResponse(
    Guid Id,
    string Target,
    DateTimeOffset StartedAtUtc,
    DateTimeOffset CompletedAtUtc,
    string Status,
    int HostsDiscovered,
    string? ErrorMessage);
