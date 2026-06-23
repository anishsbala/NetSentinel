using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NetSentinel.Api.Data;
using NetSentinel.Api.DTOs;
using NetSentinel.Api.Models;
using NetSentinel.Api.Services;

namespace NetSentinel.Api.Controllers;

[ApiController]
[Route("api/firewall-rules")]
public sealed class FirewallRulesController(
    NetSentinelDbContext dbContext,
    IFirewallRuleAnalyzer analyzer) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetRules(CancellationToken cancellationToken)
    {
        var rules = await dbContext.FirewallRules
            .AsNoTracking()
            .OrderByDescending(x => x.ObservedAtUtc)
            .Select(x => new
            {
                x.Id,
                x.DeviceId,
                DeviceName = x.Device == null ? null : x.Device.Hostname,
                x.Name,
                x.Direction,
                x.Action,
                x.Protocol,
                x.PortNumber,
                x.SourceCidr,
                x.DestinationCidr,
                x.Enabled,
                x.ObservedAtUtc,
                FindingCount = x.Alerts.Count
            })
            .ToListAsync(cancellationToken);

        return Ok(rules);
    }

    [HttpPost]
    public async Task<IActionResult> CreateRule(
        FirewallRuleRequest request,
        CancellationToken cancellationToken)
    {
        if (!Ipv4Cidr.TryParse(request.SourceCidr, out _)
            || !Ipv4Cidr.TryParse(request.DestinationCidr, out _))
        {
            return BadRequest(new ValidationProblemDetails(
                new Dictionary<string, string[]>
                {
                    ["cidr"] = ["SourceCidr and DestinationCidr must be valid IPv4 CIDR ranges."]
                }));
        }

        if (request.DeviceId.HasValue
            && !await dbContext.Devices.AnyAsync(
                x => x.Id == request.DeviceId.Value,
                cancellationToken))
        {
            return BadRequest(new ValidationProblemDetails(
                new Dictionary<string, string[]>
                {
                    ["deviceId"] = ["The specified device does not exist."]
                }));
        }

        var rule = new FirewallRule
        {
            DeviceId = request.DeviceId,
            Name = request.Name.Trim(),
            Direction = request.Direction,
            Action = request.Action,
            Protocol = request.Protocol.ToUpperInvariant(),
            PortNumber = request.PortNumber,
            SourceCidr = request.SourceCidr,
            DestinationCidr = request.DestinationCidr,
            Enabled = request.Enabled,
            ObservedAtUtc = request.ObservedAtUtc ?? DateTimeOffset.UtcNow
        };
        dbContext.FirewallRules.Add(rule);
        await dbContext.SaveChangesAsync(cancellationToken);

        var findings = await analyzer.AnalyzeAsync(rule, cancellationToken);
        foreach (var finding in findings)
        {
            if (await dbContext.Alerts.AnyAsync(
                    x => x.Fingerprint == finding.Fingerprint,
                    cancellationToken))
            {
                continue;
            }

            dbContext.Alerts.Add(new Alert
            {
                DeviceId = rule.DeviceId,
                FirewallRuleId = rule.Id,
                Category = finding.Category,
                Severity = finding.Severity,
                Title = finding.Title,
                Description = finding.Description,
                Fingerprint = finding.Fingerprint,
                CreatedAtUtc = DateTimeOffset.UtcNow
            });
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        return CreatedAtAction(nameof(GetRules), new { id = rule.Id }, new { rule, findings });
    }
}
