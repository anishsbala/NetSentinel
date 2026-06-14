using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using NetSentinel.Api.Data;
using NetSentinel.Api.Models;
using NetSentinel.Api.Services;

namespace NetSentinel.Api.Tests;

internal static class TestContextFactory
{
    public static NetSentinelDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<NetSentinelDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new NetSentinelDbContext(options);
    }

    public static FirewallRuleAnalyzer CreateAnalyzer(NetSentinelDbContext context)
    {
        var options = Options.Create(new SecurityAnalysisOptions());
        return new FirewallRuleAnalyzer(context, options);
    }

    public static FirewallRule CreateRule(
        int port,
        string source,
        FirewallAction action = FirewallAction.Allow) =>
        new()
        {
            Name = $"Test rule {port}",
            Direction = FirewallDirection.Inbound,
            Action = action,
            Protocol = "TCP",
            PortNumber = port,
            SourceCidr = source,
            DestinationCidr = "0.0.0.0/0",
            Enabled = true,
            ObservedAtUtc = DateTimeOffset.UtcNow
        };
}
