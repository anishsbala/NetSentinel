namespace NetSentinel.Api.Tests;

public sealed class SqlExposureTests
{
    [Fact]
    public async Task AnalyzeAsync_FlagsSqlFromUntrustedSource()
    {
        await using var context = TestContextFactory.CreateContext();
        var analyzer = TestContextFactory.CreateAnalyzer(context);
        var rule = TestContextFactory.CreateRule(1433, "8.8.8.0/24");

        var findings = await analyzer.AnalyzeAsync(rule);

        Assert.Contains(findings, finding => finding.Category == "ExposedDatabase");
    }

    [Fact]
    public async Task AnalyzeAsync_AllowsSqlFromTrustedSubnet()
    {
        await using var context = TestContextFactory.CreateContext();
        var analyzer = TestContextFactory.CreateAnalyzer(context);
        var rule = TestContextFactory.CreateRule(1433, "10.20.0.0/16");

        var findings = await analyzer.AnalyzeAsync(rule);

        Assert.DoesNotContain(findings, finding => finding.Category == "ExposedDatabase");
    }
}
