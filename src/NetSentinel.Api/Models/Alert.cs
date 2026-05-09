namespace NetSentinel.Api.Models;

public sealed class Alert
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid? DeviceId { get; set; }
    public Guid? FirewallRuleId { get; set; }
    public required string Category { get; set; }
    public AlertSeverity Severity { get; set; }
    public required string Title { get; set; }
    public required string Description { get; set; }
    public required string Fingerprint { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
    public bool IsAcknowledged { get; set; }

    public Device? Device { get; set; }
    public FirewallRule? FirewallRule { get; set; }
}
