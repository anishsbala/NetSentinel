namespace NetSentinel.Api.Models;

public sealed class OpenPort
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid DeviceId { get; set; }
    public Guid? ScanRecordId { get; set; }
    public int PortNumber { get; set; }
    public required string Protocol { get; set; }
    public string? ServiceName { get; set; }
    public required string ObservationSource { get; set; }
    public DateTimeOffset ObservedAtUtc { get; set; }

    public Device Device { get; set; } = null!;
    public ScanRecord? ScanRecord { get; set; }
}
