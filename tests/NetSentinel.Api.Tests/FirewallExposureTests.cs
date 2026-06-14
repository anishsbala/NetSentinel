using NetSentinel.Api.Models;

namespace NetSentinel.Api.Tests;

public sealed class FirewallExposureTests
{
    [Theory]
    [InlineData(3389, "Critical")]
    [InlineData(22, "High")]
    public async Task AnalyzeAsync_FlagsGlobalRemoteAccess(int port, string severity)
    {
        await using var context = TestContextFactory.CreateContext();
        var analyzer = TestContextFactory.CreateAnalyzer(context);
        var rule = TestContextFactory.CreateRule(port, "0.0.0.0/0");

        var findings = await analyzer.AnalyzeAsync(rule);

        var finding = Assert.Single(findings);
        Assert.Equal("ExposedRemoteAccess", finding.Category);
        Assert.Equal(Enum.Parse<AlertSeverity>(severity), finding.Severity);
    }

    [Fact]
    public async Task AnalyzeAsync_IgnoresDisabledRule()
    {
        await using var context = TestContextFactory.CreateContext();
        var analyzer = TestContextFactory.CreateAnalyzer(context);
        var rule = TestContextFactory.CreateRule(3389, "0.0.0.0/0");
        rule.Enabled = false;

        var findings = await analyzer.AnalyzeAsync(rule);

        Assert.Empty(findings);
    }
}
