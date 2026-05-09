namespace NetSentinel.Api.Models;

public sealed class FirewallRule
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid? DeviceId { get; set; }
    public required string Name { get; set; }
    public FirewallDirection Direction { get; set; }
    public FirewallAction Action { get; set; }
    public required string Protocol { get; set; }
    public int? PortNumber { get; set; }
    public required string SourceCidr { get; set; }
    public required string DestinationCidr { get; set; }
    public bool Enabled { get; set; }
    public DateTimeOffset ObservedAtUtc { get; set; }

    public Device? Device { get; set; }
    public ICollection<Alert> Alerts { get; set; } = [];
}
