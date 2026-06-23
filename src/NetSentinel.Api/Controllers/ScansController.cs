using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NetSentinel.Api.Data;
using NetSentinel.Api.DTOs;
using NetSentinel.Api.Services;

namespace NetSentinel.Api.Controllers;

[ApiController]
[Route("api/scans")]
public sealed class ScansController(
    NetSentinelDbContext dbContext,
    IScanIngestionService ingestionService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ScanResponse>>> GetScans(
        CancellationToken cancellationToken)
    {
        var scans = await dbContext.Scans
            .AsNoTracking()
            .OrderByDescending(x => x.CompletedAtUtc)
            .Take(100)
            .Select(x => new ScanResponse(
                x.Id,
                x.Target,
                x.StartedAtUtc,
                x.CompletedAtUtc,
                x.Status.ToString(),
                x.HostsDiscovered,
                x.ErrorMessage))
            .ToListAsync(cancellationToken);

        return Ok(scans);
    }

    [HttpPost("results")]
    public async Task<ActionResult<ScanResponse>> IngestResult(
        ScanReportRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var scan = await ingestionService.IngestAsync(request, cancellationToken);
            var response = new ScanResponse(
                scan.Id,
                scan.Target,
                scan.StartedAtUtc,
                scan.CompletedAtUtc,
                scan.Status.ToString(),
                scan.HostsDiscovered,
                scan.ErrorMessage);
            return CreatedAtAction(nameof(GetScans), new { id = scan.Id }, response);
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new ValidationProblemDetails(
                new Dictionary<string, string[]>
                {
                    ["scan"] = [exception.Message]
                }));
        }
    }
}
