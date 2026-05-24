using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NetSentinel.Api.Data;

namespace NetSentinel.Api.Controllers;

[ApiController]
[Route("api/devices")]
public sealed class DevicesController(NetSentinelDbContext dbContext) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetDevices(CancellationToken cancellationToken)
    {
        var devices = await dbContext.Devices
            .AsNoTracking()
            .OrderByDescending(x => x.LastSeenAtUtc)
            .Select(x => new
            {
                x.Id,
                x.Hostname,
                x.IpAddress,
                x.OsType,
                x.FirstSeenAtUtc,
                x.LastSeenAtUtc,
                OpenPortCount = x.OpenPorts.Count
            })
            .ToListAsync(cancellationToken);

        return Ok(devices);
    }
}
