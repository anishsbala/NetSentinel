using System.ComponentModel.DataAnnotations;
using NetSentinel.Api.Models;

namespace NetSentinel.Api.DTOs;

public sealed record FirewallRuleRequest(
    Guid? DeviceId,
    [property: Required, StringLength(160)] string Name,
    FirewallDirection Direction,
    FirewallAction Action,
    [property: Required, RegularExpression("^(?i:TCP|UDP|ANY)$")] string Protocol,
    [property: Range(1, 65535)] int? PortNumber,
    [property: Required, StringLength(64)] string SourceCidr,
    [property: Required, StringLength(64)] string DestinationCidr,
    bool Enabled,
    DateTimeOffset? ObservedAtUtc = null);

public sealed record FirewallFinding(
    string Category,
    AlertSeverity Severity,
    string Title,
    string Description,
    string Fingerprint);
