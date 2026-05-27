using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NetSentinel.Api.Data;

namespace NetSentinel.Api.Controllers;

[ApiController]
[Route("api/alerts")]
public sealed class AlertsController(NetSentinelDbContext dbContext) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAlerts(CancellationToken cancellationToken)
    {
        var alerts = await dbContext.Alerts
            .AsNoTracking()
            .OrderByDescending(x => x.CreatedAtUtc)
            .Select(x => new
            {
                x.Id,
                x.DeviceId,
                x.FirewallRuleId,
                x.Category,
                x.Severity,
                x.Title,
                x.Description,
                x.CreatedAtUtc,
                x.IsAcknowledged,
                DeviceName = x.Device == null ? null : x.Device.Hostname,
                RuleName = x.FirewallRule == null ? null : x.FirewallRule.Name
            })
            .ToListAsync(cancellationToken);

        return Ok(alerts);
    }
}
