using NetSentinel.Api.Models;

namespace NetSentinel.Api.Tests;

public sealed class FirewallConflictTests
{
    [Fact]
    public async Task AnalyzeAsync_FlagsOverlappingOppositeActions()
    {
        await using var context = TestContextFactory.CreateContext();
        var existing = TestContextFactory.CreateRule(
            443,
            "10.0.0.0/8",
            FirewallAction.Deny);
        existing.Name = "Deny internal HTTPS";
        context.FirewallRules.Add(existing);
        await context.SaveChangesAsync();

        var candidate = TestContextFactory.CreateRule(443, "10.10.0.0/16");
        candidate.Name = "Allow application HTTPS";
        var analyzer = TestContextFactory.CreateAnalyzer(context);

        var findings = await analyzer.AnalyzeAsync(candidate);

        Assert.Contains(findings, finding => finding.Category == "ConflictingRule");
    }

    [Fact]
    public async Task AnalyzeAsync_IgnoresDisjointRules()
    {
        await using var context = TestContextFactory.CreateContext();
        context.FirewallRules.Add(
            TestContextFactory.CreateRule(443, "10.0.0.0/8", FirewallAction.Deny));
        await context.SaveChangesAsync();
        var candidate = TestContextFactory.CreateRule(443, "192.168.0.0/16");
        var analyzer = TestContextFactory.CreateAnalyzer(context);

        var findings = await analyzer.AnalyzeAsync(candidate);

        Assert.DoesNotContain(findings, finding => finding.Category == "ConflictingRule");
    }
}
