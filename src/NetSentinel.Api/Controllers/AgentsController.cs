using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NetSentinel.Api.Data;
using NetSentinel.Api.DTOs;
using NetSentinel.Api.Services;

namespace NetSentinel.Api.Controllers;

[ApiController]
[Route("api/agents/heartbeats")]
public sealed class AgentsController(
    NetSentinelDbContext dbContext,
    IAgentHeartbeatService heartbeatService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<AgentHeartbeatResponse>>> GetHeartbeats(
        CancellationToken cancellationToken)
    {
        var heartbeats = await dbContext.AgentHeartbeats
            .AsNoTracking()
            .Include(x => x.Device)
            .OrderByDescending(x => x.ReportedAtUtc)
            .Take(100)
            .ToListAsync(cancellationToken);

        var response = heartbeats.Select(x => new AgentHeartbeatResponse(
            x.Id,
            x.DeviceId,
            x.AgentId,
            x.Device.Hostname,
            x.Device.IpAddress,
            x.Device.OsType,
            x.FirewallEnabled,
            JsonSerializer.Deserialize<List<PortObservationRequest>>(x.ListeningPortsJson) ?? [],
            JsonSerializer.Deserialize<List<ServiceInventoryItem>>(x.ServicesJson) ?? [],
            x.CpuPercent,
            x.MemoryPercent,
            x.ReportedAtUtc,
            x.ReceivedAtUtc));

        return Ok(response);
    }

    [HttpPost]
    public async Task<ActionResult<AgentHeartbeatResponse>> RecordHeartbeat(
        AgentHeartbeatRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var heartbeat = await heartbeatService.RecordAsync(request, cancellationToken);
            var response = new AgentHeartbeatResponse(
                heartbeat.Id,
                heartbeat.DeviceId,
                heartbeat.AgentId,
                request.Hostname,
                request.IpAddress,
                request.OsType,
                heartbeat.FirewallEnabled,
                request.ListeningPorts,
                request.Services,
                heartbeat.CpuPercent,
                heartbeat.MemoryPercent,
                heartbeat.ReportedAtUtc,
                heartbeat.ReceivedAtUtc);
            return CreatedAtAction(nameof(GetHeartbeats), new { id = heartbeat.Id }, response);
        }
        catch (ArgumentException exception)
        {
            return ValidationProblem(new Dictionary<string, string[]>
            {
                ["heartbeat"] = [exception.Message]
            });
        }
    }
}
