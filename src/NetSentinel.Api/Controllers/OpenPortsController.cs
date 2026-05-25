using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NetSentinel.Api.Data;

namespace NetSentinel.Api.Controllers;

[ApiController]
[Route("api/open-ports")]
public sealed class OpenPortsController(NetSentinelDbContext dbContext) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetOpenPorts(CancellationToken cancellationToken)
    {
        var ports = await dbContext.OpenPorts
            .AsNoTracking()
            .OrderByDescending(x => x.ObservedAtUtc)
            .Select(x => new
            {
                x.Id,
                x.DeviceId,
                x.Device.Hostname,
                x.Device.IpAddress,
                x.PortNumber,
                x.Protocol,
                x.ServiceName,
                x.ObservationSource,
                x.ObservedAtUtc
            })
            .ToListAsync(cancellationToken);

        return Ok(ports);
    }
}
