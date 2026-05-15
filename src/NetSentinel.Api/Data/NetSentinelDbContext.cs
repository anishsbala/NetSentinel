using Microsoft.EntityFrameworkCore;
using NetSentinel.Api.Models;

namespace NetSentinel.Api.Data;

public sealed class NetSentinelDbContext(DbContextOptions<NetSentinelDbContext> options)
    : DbContext(options)
{
    public DbSet<Device> Devices => Set<Device>();
    public DbSet<ScanRecord> Scans => Set<ScanRecord>();
    public DbSet<OpenPort> OpenPorts => Set<OpenPort>();
    public DbSet<FirewallRule> FirewallRules => Set<FirewallRule>();
    public DbSet<Alert> Alerts => Set<Alert>();
    public DbSet<AgentHeartbeat> AgentHeartbeats => Set<AgentHeartbeat>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Device>(entity =>
        {
            entity.HasIndex(x => x.IpAddress).IsUnique();
            entity.Property(x => x.Hostname).HasMaxLength(128);
            entity.Property(x => x.IpAddress).HasMaxLength(45);
            entity.Property(x => x.OsType).HasMaxLength(64);
        });

        modelBuilder.Entity<ScanRecord>(entity =>
        {
            entity.Property(x => x.Target).HasMaxLength(128);
            entity.Property(x => x.Status).HasConversion<string>().HasMaxLength(32);
            entity.Property(x => x.ErrorMessage).HasMaxLength(512);
        });

        modelBuilder.Entity<OpenPort>(entity =>
        {
            entity.HasIndex(x => new
            {
                x.DeviceId,
                x.PortNumber,
                x.Protocol,
                x.ObservationSource
            }).IsUnique();
            entity.Property(x => x.Protocol).HasMaxLength(16);
            entity.Property(x => x.ServiceName).HasMaxLength(128);
            entity.Property(x => x.ObservationSource).HasMaxLength(32);
            entity.HasOne(x => x.Device)
                .WithMany(x => x.OpenPorts)
                .HasForeignKey(x => x.DeviceId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.ScanRecord)
                .WithMany(x => x.OpenPorts)
                .HasForeignKey(x => x.ScanRecordId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<FirewallRule>(entity =>
        {
            entity.Property(x => x.Name).HasMaxLength(160);
            entity.Property(x => x.Direction).HasConversion<string>().HasMaxLength(16);
            entity.Property(x => x.Action).HasConversion<string>().HasMaxLength(16);
            entity.Property(x => x.Protocol).HasMaxLength(16);
            entity.Property(x => x.SourceCidr).HasMaxLength(64);
            entity.Property(x => x.DestinationCidr).HasMaxLength(64);
            entity.HasOne(x => x.Device)
                .WithMany(x => x.FirewallRules)
                .HasForeignKey(x => x.DeviceId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<Alert>(entity =>
        {
            entity.HasIndex(x => x.Fingerprint).IsUnique();
            entity.Property(x => x.Category).HasMaxLength(64);
            entity.Property(x => x.Severity).HasConversion<string>().HasMaxLength(16);
            entity.Property(x => x.Title).HasMaxLength(160);
            entity.Property(x => x.Description).HasMaxLength(1000);
            entity.Property(x => x.Fingerprint).HasMaxLength(128);
            entity.HasOne(x => x.Device)
                .WithMany()
                .HasForeignKey(x => x.DeviceId)
                .OnDelete(DeleteBehavior.NoAction);
            entity.HasOne(x => x.FirewallRule)
                .WithMany(x => x.Alerts)
                .HasForeignKey(x => x.FirewallRuleId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<AgentHeartbeat>(entity =>
        {
            entity.HasIndex(x => x.AgentId);
            entity.Property(x => x.AgentId).HasMaxLength(128);
            entity.Property(x => x.ListeningPortsJson).HasColumnType("nvarchar(max)");
            entity.Property(x => x.ServicesJson).HasColumnType("nvarchar(max)");
            entity.HasOne(x => x.Device)
                .WithMany(x => x.Heartbeats)
                .HasForeignKey(x => x.DeviceId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
