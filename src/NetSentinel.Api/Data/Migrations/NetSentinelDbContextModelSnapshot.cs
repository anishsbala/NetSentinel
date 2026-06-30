using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace NetSentinel.Api.Data.Migrations;

[DbContext(typeof(NetSentinelDbContext))]
public sealed class NetSentinelDbContextModelSnapshot : ModelSnapshot
{
    protected override void BuildModel(ModelBuilder modelBuilder)
    {
#pragma warning disable 612, 618
        modelBuilder
            .HasAnnotation("ProductVersion", "10.0.0")
            .HasAnnotation("Relational:MaxIdentifierLength", 128);

        SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

        modelBuilder.Entity("NetSentinel.Api.Models.AgentHeartbeat", b =>
        {
            b.Property<Guid>("Id").ValueGeneratedOnAdd().HasColumnType("uniqueidentifier");
            b.Property<string>("AgentId").IsRequired().HasMaxLength(128).HasColumnType("nvarchar(128)");
            b.Property<double>("CpuPercent").HasColumnType("float");
            b.Property<Guid>("DeviceId").HasColumnType("uniqueidentifier");
            b.Property<bool>("FirewallEnabled").HasColumnType("bit");
            b.Property<string>("ListeningPortsJson").IsRequired().HasColumnType("nvarchar(max)");
            b.Property<double>("MemoryPercent").HasColumnType("float");
            b.Property<DateTimeOffset>("ReceivedAtUtc").HasColumnType("datetimeoffset");
            b.Property<DateTimeOffset>("ReportedAtUtc").HasColumnType("datetimeoffset");
            b.Property<string>("ServicesJson").IsRequired().HasColumnType("nvarchar(max)");
            b.HasKey("Id");
            b.HasIndex("AgentId");
            b.HasIndex("DeviceId");
            b.ToTable("AgentHeartbeats");
        });

        modelBuilder.Entity("NetSentinel.Api.Models.Alert", b =>
        {
            b.Property<Guid>("Id").ValueGeneratedOnAdd().HasColumnType("uniqueidentifier");
            b.Property<string>("Category").IsRequired().HasMaxLength(64).HasColumnType("nvarchar(64)");
            b.Property<DateTimeOffset>("CreatedAtUtc").HasColumnType("datetimeoffset");
            b.Property<string>("Description").IsRequired().HasMaxLength(1000).HasColumnType("nvarchar(1000)");
            b.Property<Guid?>("DeviceId").HasColumnType("uniqueidentifier");
            b.Property<string>("Fingerprint").IsRequired().HasMaxLength(128).HasColumnType("nvarchar(128)");
            b.Property<Guid?>("FirewallRuleId").HasColumnType("uniqueidentifier");
            b.Property<bool>("IsAcknowledged").HasColumnType("bit");
            b.Property<string>("Severity").IsRequired().HasMaxLength(16).HasColumnType("nvarchar(16)");
            b.Property<string>("Title").IsRequired().HasMaxLength(160).HasColumnType("nvarchar(160)");
            b.HasKey("Id");
            b.HasIndex("DeviceId");
            b.HasIndex("Fingerprint").IsUnique();
            b.HasIndex("FirewallRuleId");
            b.ToTable("Alerts");
        });

        modelBuilder.Entity("NetSentinel.Api.Models.Device", b =>
        {
            b.Property<Guid>("Id").ValueGeneratedOnAdd().HasColumnType("uniqueidentifier");
            b.Property<DateTimeOffset>("FirstSeenAtUtc").HasColumnType("datetimeoffset");
            b.Property<string>("Hostname").IsRequired().HasMaxLength(128).HasColumnType("nvarchar(128)");
            b.Property<string>("IpAddress").IsRequired().HasMaxLength(45).HasColumnType("nvarchar(45)");
            b.Property<DateTimeOffset>("LastSeenAtUtc").HasColumnType("datetimeoffset");
            b.Property<string>("OsType").IsRequired().HasMaxLength(64).HasColumnType("nvarchar(64)");
            b.HasKey("Id");
            b.HasIndex("IpAddress").IsUnique();
            b.ToTable("Devices");
        });

        modelBuilder.Entity("NetSentinel.Api.Models.FirewallRule", b =>
        {
            b.Property<Guid>("Id").ValueGeneratedOnAdd().HasColumnType("uniqueidentifier");
            b.Property<string>("Action").IsRequired().HasMaxLength(16).HasColumnType("nvarchar(16)");
            b.Property<Guid?>("DeviceId").HasColumnType("uniqueidentifier");
            b.Property<string>("DestinationCidr").IsRequired().HasMaxLength(64).HasColumnType("nvarchar(64)");
            b.Property<string>("Direction").IsRequired().HasMaxLength(16).HasColumnType("nvarchar(16)");
            b.Property<bool>("Enabled").HasColumnType("bit");
            b.Property<string>("Name").IsRequired().HasMaxLength(160).HasColumnType("nvarchar(160)");
            b.Property<DateTimeOffset>("ObservedAtUtc").HasColumnType("datetimeoffset");
            b.Property<int?>("PortNumber").HasColumnType("int");
            b.Property<string>("Protocol").IsRequired().HasMaxLength(16).HasColumnType("nvarchar(16)");
            b.Property<string>("SourceCidr").IsRequired().HasMaxLength(64).HasColumnType("nvarchar(64)");
            b.HasKey("Id");
            b.HasIndex("DeviceId");
            b.ToTable("FirewallRules");
        });

        modelBuilder.Entity("NetSentinel.Api.Models.OpenPort", b =>
        {
            b.Property<Guid>("Id").ValueGeneratedOnAdd().HasColumnType("uniqueidentifier");
            b.Property<Guid>("DeviceId").HasColumnType("uniqueidentifier");
            b.Property<string>("ObservationSource").IsRequired().HasMaxLength(32).HasColumnType("nvarchar(32)");
            b.Property<DateTimeOffset>("ObservedAtUtc").HasColumnType("datetimeoffset");
            b.Property<int>("PortNumber").HasColumnType("int");
            b.Property<string>("Protocol").IsRequired().HasMaxLength(16).HasColumnType("nvarchar(16)");
            b.Property<Guid?>("ScanRecordId").HasColumnType("uniqueidentifier");
            b.Property<string>("ServiceName").HasMaxLength(128).HasColumnType("nvarchar(128)");
            b.HasKey("Id");
            b.HasIndex("ScanRecordId");
            b.HasIndex("DeviceId", "PortNumber", "Protocol", "ObservationSource").IsUnique();
            b.ToTable("OpenPorts");
        });

        modelBuilder.Entity("NetSentinel.Api.Models.ScanRecord", b =>
        {
            b.Property<Guid>("Id").ValueGeneratedOnAdd().HasColumnType("uniqueidentifier");
            b.Property<DateTimeOffset>("CompletedAtUtc").HasColumnType("datetimeoffset");
            b.Property<string>("ErrorMessage").HasMaxLength(512).HasColumnType("nvarchar(512)");
            b.Property<int>("HostsDiscovered").HasColumnType("int");
            b.Property<DateTimeOffset>("StartedAtUtc").HasColumnType("datetimeoffset");
            b.Property<string>("Status").IsRequired().HasMaxLength(32).HasColumnType("nvarchar(32)");
            b.Property<string>("Target").IsRequired().HasMaxLength(128).HasColumnType("nvarchar(128)");
            b.HasKey("Id");
            b.ToTable("Scans");
        });

        modelBuilder.Entity("NetSentinel.Api.Models.AgentHeartbeat", b =>
        {
            b.HasOne("NetSentinel.Api.Models.Device", "Device")
                .WithMany("Heartbeats")
                .HasForeignKey("DeviceId")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();
            b.Navigation("Device");
        });

        modelBuilder.Entity("NetSentinel.Api.Models.Alert", b =>
        {
            b.HasOne("NetSentinel.Api.Models.Device", "Device")
                .WithMany()
                .HasForeignKey("DeviceId")
                .OnDelete(DeleteBehavior.NoAction);
            b.HasOne("NetSentinel.Api.Models.FirewallRule", "FirewallRule")
                .WithMany("Alerts")
                .HasForeignKey("FirewallRuleId")
                .OnDelete(DeleteBehavior.SetNull);
            b.Navigation("Device");
            b.Navigation("FirewallRule");
        });

        modelBuilder.Entity("NetSentinel.Api.Models.FirewallRule", b =>
        {
            b.HasOne("NetSentinel.Api.Models.Device", "Device")
                .WithMany("FirewallRules")
                .HasForeignKey("DeviceId")
                .OnDelete(DeleteBehavior.SetNull);
            b.Navigation("Device");
        });

        modelBuilder.Entity("NetSentinel.Api.Models.OpenPort", b =>
        {
            b.HasOne("NetSentinel.Api.Models.Device", "Device")
                .WithMany("OpenPorts")
                .HasForeignKey("DeviceId")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();
            b.HasOne("NetSentinel.Api.Models.ScanRecord", "ScanRecord")
                .WithMany("OpenPorts")
                .HasForeignKey("ScanRecordId")
                .OnDelete(DeleteBehavior.SetNull);
            b.Navigation("Device");
            b.Navigation("ScanRecord");
        });

        modelBuilder.Entity("NetSentinel.Api.Models.Device", b =>
        {
            b.Navigation("FirewallRules");
            b.Navigation("Heartbeats");
            b.Navigation("OpenPorts");
        });

        modelBuilder.Entity("NetSentinel.Api.Models.FirewallRule", b =>
        {
            b.Navigation("Alerts");
        });

        modelBuilder.Entity("NetSentinel.Api.Models.ScanRecord", b =>
        {
            b.Navigation("OpenPorts");
        });
#pragma warning restore 612, 618
    }
}
