using System.Net;
using Microsoft.EntityFrameworkCore;
using NetSentinel.Api.Data;
using NetSentinel.Api.DTOs;
using NetSentinel.Api.Models;

namespace NetSentinel.Api.Services;

public interface IScanIngestionService
{
    Task<ScanRecord> IngestAsync(
        ScanReportRequest request,
        CancellationToken cancellationToken = default);
}

public sealed class ScanIngestionService(NetSentinelDbContext dbContext)
    : IScanIngestionService
{
    public async Task<ScanRecord> IngestAsync(
        ScanReportRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request.CompletedAtUtc < request.StartedAtUtc)
        {
            throw new ArgumentException("CompletedAtUtc cannot precede StartedAtUtc.");
        }

        var scan = new ScanRecord
        {
            Target = request.Target.Trim(),
            StartedAtUtc = request.StartedAtUtc,
            CompletedAtUtc = request.CompletedAtUtc,
            Status = request.ErrorMessage is null ? ScanStatus.Completed : ScanStatus.Failed,
            HostsDiscovered = request.Hosts.Count,
            ErrorMessage = request.ErrorMessage
        };
        dbContext.Scans.Add(scan);

        foreach (var host in request.Hosts)
        {
            if (!IPAddress.TryParse(host.IpAddress, out _))
            {
                throw new ArgumentException($"Invalid host IP address: {host.IpAddress}");
            }

            var device = dbContext.Devices.Local.FirstOrDefault(x => x.IpAddress == host.IpAddress)
                ?? await dbContext.Devices.FirstOrDefaultAsync(
                    x => x.IpAddress == host.IpAddress,
                    cancellationToken);
            if (device is null)
            {
                device = new Device
                {
                    Hostname = host.Hostname.Trim(),
                    IpAddress = host.IpAddress,
                    OsType = string.IsNullOrWhiteSpace(host.OsType) ? "Unknown" : host.OsType.Trim(),
                    FirstSeenAtUtc = request.CompletedAtUtc,
                    LastSeenAtUtc = request.CompletedAtUtc
                };
                dbContext.Devices.Add(device);
            }
            else
            {
                device.Hostname = host.Hostname.Trim();
                device.OsType = string.IsNullOrWhiteSpace(host.OsType)
                    ? device.OsType
                    : host.OsType.Trim();
                device.LastSeenAtUtc = request.CompletedAtUtc;
            }

            foreach (var observation in host.OpenPorts)
            {
                var protocol = observation.Protocol.ToUpperInvariant();
                var openPort = dbContext.OpenPorts.Local.FirstOrDefault(x =>
                        x.DeviceId == device.Id
                        && x.PortNumber == observation.PortNumber
                        && x.Protocol == protocol
                        && x.ObservationSource == "Scanner")
                    ?? await dbContext.OpenPorts.FirstOrDefaultAsync(
                        x => x.DeviceId == device.Id
                            && x.PortNumber == observation.PortNumber
                            && x.Protocol == protocol
                            && x.ObservationSource == "Scanner",
                        cancellationToken);

                if (openPort is null)
                {
                    dbContext.OpenPorts.Add(new OpenPort
                    {
                        Device = device,
                        ScanRecord = scan,
                        PortNumber = observation.PortNumber,
                        Protocol = protocol,
                        ServiceName = observation.ServiceName,
                        ObservationSource = "Scanner",
                        ObservedAtUtc = request.CompletedAtUtc
                    });
                }
                else
                {
                    openPort.ScanRecord = scan;
                    openPort.ServiceName = observation.ServiceName;
                    openPort.ObservedAtUtc = request.CompletedAtUtc;
                }
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        return scan;
    }
}
