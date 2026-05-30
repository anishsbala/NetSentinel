using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace NetSentinel.Api.Data.Migrations;

[DbContext(typeof(NetSentinelDbContext))]
[Migration("202607030001_InitialCreate")]
public sealed class InitialCreate : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Devices",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Hostname = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                IpAddress = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: false),
                OsType = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                FirstSeenAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                LastSeenAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_Devices", x => x.Id));

        migrationBuilder.CreateTable(
            name: "Scans",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Target = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                StartedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                CompletedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                Status = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                HostsDiscovered = table.Column<int>(type: "int", nullable: false),
                ErrorMessage = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true)
            },
            constraints: table => table.PrimaryKey("PK_Scans", x => x.Id));

        migrationBuilder.CreateTable(
            name: "AgentHeartbeats",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                DeviceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                AgentId = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                FirewallEnabled = table.Column<bool>(type: "bit", nullable: false),
                ListeningPortsJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                ServicesJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                CpuPercent = table.Column<double>(type: "float", nullable: false),
                MemoryPercent = table.Column<double>(type: "float", nullable: false),
                ReportedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                ReceivedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AgentHeartbeats", x => x.Id);
                table.ForeignKey(
                    name: "FK_AgentHeartbeats_Devices_DeviceId",
                    column: x => x.DeviceId,
                    principalTable: "Devices",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "FirewallRules",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                DeviceId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                Name = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: false),
                Direction = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                Action = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                Protocol = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                PortNumber = table.Column<int>(type: "int", nullable: true),
                SourceCidr = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                DestinationCidr = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                Enabled = table.Column<bool>(type: "bit", nullable: false),
                ObservedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_FirewallRules", x => x.Id);
                table.ForeignKey(
                    name: "FK_FirewallRules_Devices_DeviceId",
                    column: x => x.DeviceId,
                    principalTable: "Devices",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.SetNull);
            });

        migrationBuilder.CreateTable(
            name: "OpenPorts",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                DeviceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                ScanRecordId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                PortNumber = table.Column<int>(type: "int", nullable: false),
                Protocol = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                ServiceName = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                ObservationSource = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                ObservedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_OpenPorts", x => x.Id);
                table.ForeignKey(
                    name: "FK_OpenPorts_Devices_DeviceId",
                    column: x => x.DeviceId,
                    principalTable: "Devices",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_OpenPorts_Scans_ScanRecordId",
                    column: x => x.ScanRecordId,
                    principalTable: "Scans",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.SetNull);
            });

        migrationBuilder.CreateTable(
            name: "Alerts",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                DeviceId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                FirewallRuleId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                Category = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                Severity = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                Title = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: false),
                Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                Fingerprint = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                CreatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                IsAcknowledged = table.Column<bool>(type: "bit", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Alerts", x => x.Id);
                table.ForeignKey(
                    name: "FK_Alerts_Devices_DeviceId",
                    column: x => x.DeviceId,
                    principalTable: "Devices",
                    principalColumn: "Id");
                table.ForeignKey(
                    name: "FK_Alerts_FirewallRules_FirewallRuleId",
                    column: x => x.FirewallRuleId,
                    principalTable: "FirewallRules",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.SetNull);
            });

        migrationBuilder.CreateIndex(
            name: "IX_AgentHeartbeats_AgentId",
            table: "AgentHeartbeats",
            column: "AgentId");
        migrationBuilder.CreateIndex(
            name: "IX_AgentHeartbeats_DeviceId",
            table: "AgentHeartbeats",
            column: "DeviceId");
        migrationBuilder.CreateIndex(
            name: "IX_Alerts_DeviceId",
            table: "Alerts",
            column: "DeviceId");
        migrationBuilder.CreateIndex(
            name: "IX_Alerts_Fingerprint",
            table: "Alerts",
            column: "Fingerprint",
            unique: true);
        migrationBuilder.CreateIndex(
            name: "IX_Alerts_FirewallRuleId",
            table: "Alerts",
            column: "FirewallRuleId");
        migrationBuilder.CreateIndex(
            name: "IX_Devices_IpAddress",
            table: "Devices",
            column: "IpAddress",
            unique: true);
        migrationBuilder.CreateIndex(
            name: "IX_FirewallRules_DeviceId",
            table: "FirewallRules",
            column: "DeviceId");
        migrationBuilder.CreateIndex(
            name: "IX_OpenPorts_DeviceId_PortNumber_Protocol_ObservationSource",
            table: "OpenPorts",
            columns: ["DeviceId", "PortNumber", "Protocol", "ObservationSource"],
            unique: true);
        migrationBuilder.CreateIndex(
            name: "IX_OpenPorts_ScanRecordId",
            table: "OpenPorts",
            column: "ScanRecordId");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "AgentHeartbeats");
        migrationBuilder.DropTable(name: "Alerts");
        migrationBuilder.DropTable(name: "OpenPorts");
        migrationBuilder.DropTable(name: "FirewallRules");
        migrationBuilder.DropTable(name: "Scans");
        migrationBuilder.DropTable(name: "Devices");
    }
}
