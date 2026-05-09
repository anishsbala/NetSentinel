namespace NetSentinel.Api.Models;

public sealed class ScanRecord
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string Target { get; set; }
    public DateTimeOffset StartedAtUtc { get; set; }
    public DateTimeOffset CompletedAtUtc { get; set; }
    public ScanStatus Status { get; set; }
    public int HostsDiscovered { get; set; }
    public string? ErrorMessage { get; set; }

    public ICollection<OpenPort> OpenPorts { get; set; } = [];
}
