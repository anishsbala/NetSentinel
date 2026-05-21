using System.Net;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using NetSentinel.Api.Data;
using NetSentinel.Api.DTOs;
using NetSentinel.Api.Models;

namespace NetSentinel.Api.Services;

public interface IAgentHeartbeatService
{
    Task<AgentHeartbeat> RecordAsync(
        AgentHeartbeatRequest request,
        CancellationToken cancellationToken = default);
}

public sealed class AgentHeartbeatService(NetSentinelDbContext dbContext)
    : IAgentHeartbeatService
{
    public async Task<AgentHeartbeat> RecordAsync(
        AgentHeartbeatRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!IPAddress.TryParse(request.IpAddress, out _))
        {
            throw new ArgumentException("IpAddress must be a valid IP address.");
        }

        var receivedAt = DateTimeOffset.UtcNow;
        var device = await dbContext.Devices.FirstOrDefaultAsync(
            x => x.IpAddress == request.IpAddress,
            cancellationToken);
        if (device is null)
        {
            device = new Device
            {
                Hostname = request.Hostname.Trim(),
                IpAddress = request.IpAddress,
                OsType = request.OsType.Trim(),
                FirstSeenAtUtc = request.ReportedAtUtc,
                LastSeenAtUtc = request.ReportedAtUtc
            };
            dbContext.Devices.Add(device);
        }
        else
        {
            device.Hostname = request.Hostname.Trim();
            device.OsType = request.OsType.Trim();
            device.LastSeenAtUtc = request.ReportedAtUtc;
        }

        var heartbeat = new AgentHeartbeat
        {
            Device = device,
            AgentId = request.AgentId.Trim(),
            FirewallEnabled = request.FirewallEnabled,
            ListeningPortsJson = JsonSerializer.Serialize(request.ListeningPorts),
            ServicesJson = JsonSerializer.Serialize(request.Services),
            CpuPercent = request.CpuPercent,
            MemoryPercent = request.MemoryPercent,
            ReportedAtUtc = request.ReportedAtUtc,
            ReceivedAtUtc = receivedAt
        };
        dbContext.AgentHeartbeats.Add(heartbeat);

        foreach (var observation in request.ListeningPorts)
        {
            var protocol = observation.Protocol.ToUpperInvariant();
            var openPort = dbContext.OpenPorts.Local.FirstOrDefault(x =>
                    x.DeviceId == device.Id
                    && x.PortNumber == observation.PortNumber
                    && x.Protocol == protocol
                    && x.ObservationSource == "Agent")
                ?? await dbContext.OpenPorts.FirstOrDefaultAsync(
                    x => x.DeviceId == device.Id
                        && x.PortNumber == observation.PortNumber
                        && x.Protocol == protocol
                        && x.ObservationSource == "Agent",
                    cancellationToken);

            if (openPort is null)
            {
                dbContext.OpenPorts.Add(new OpenPort
                {
                    Device = device,
                    PortNumber = observation.PortNumber,
                    Protocol = protocol,
                    ServiceName = observation.ServiceName,
                    ObservationSource = "Agent",
                    ObservedAtUtc = request.ReportedAtUtc
                });
            }
            else
            {
                openPort.ServiceName = observation.ServiceName;
                openPort.ObservedAtUtc = request.ReportedAtUtc;
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        return heartbeat;
    }
}
