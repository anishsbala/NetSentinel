using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using NetSentinel.Api.Data;
using NetSentinel.Api.DTOs;
using NetSentinel.Api.Models;

namespace NetSentinel.Api.Services;

public interface IFirewallRuleAnalyzer
{
    Task<IReadOnlyList<FirewallFinding>> AnalyzeAsync(
        FirewallRule rule,
        CancellationToken cancellationToken = default);
}

public sealed class FirewallRuleAnalyzer(
    NetSentinelDbContext dbContext,
    IOptions<SecurityAnalysisOptions> options) : IFirewallRuleAnalyzer
{
    public async Task<IReadOnlyList<FirewallFinding>> AnalyzeAsync(
        FirewallRule rule,
        CancellationToken cancellationToken = default)
    {
        var findings = new List<FirewallFinding>();
        if (!rule.Enabled)
        {
            return findings;
        }

        if (!Ipv4Cidr.TryParse(rule.SourceCidr, out var source))
        {
            throw new ArgumentException("SourceCidr must be a valid IPv4 CIDR.", nameof(rule));
        }

        var allowsInboundTcp = rule.Action == FirewallAction.Allow
            && rule.Direction == FirewallDirection.Inbound
            && IsTcp(rule.Protocol);
        var isGlobalSource = source.Network == 0 && source.Broadcast == uint.MaxValue;

        if (allowsInboundTcp && isGlobalSource && rule.PortNumber == 3389)
        {
            findings.Add(CreateFinding(
                rule,
                "ExposedRemoteAccess",
                AlertSeverity.Critical,
                "RDP is open to every IPv4 address",
                "Inbound TCP port 3389 is allowed from 0.0.0.0/0.",
                "rdp-global"));
        }

        if (allowsInboundTcp && isGlobalSource && rule.PortNumber == 22)
        {
            findings.Add(CreateFinding(
                rule,
                "ExposedRemoteAccess",
                AlertSeverity.High,
                "SSH is open to every IPv4 address",
                "Inbound TCP port 22 is allowed from 0.0.0.0/0.",
                "ssh-global"));
        }

        if (allowsInboundTcp && rule.PortNumber == 1433 && !IsTrusted(source))
        {
            findings.Add(CreateFinding(
                rule,
                "ExposedDatabase",
                AlertSeverity.Critical,
                "SQL Server is exposed outside trusted networks",
                $"Inbound TCP port 1433 is allowed from untrusted source {rule.SourceCidr}.",
                "sql-untrusted"));
        }

        var candidates = await dbContext.FirewallRules
            .AsNoTracking()
            .Where(existing =>
                existing.Id != rule.Id
                && existing.Enabled
                && existing.Direction == rule.Direction
                && existing.PortNumber == rule.PortNumber
                && existing.Action != rule.Action)
            .ToListAsync(cancellationToken);

        foreach (var existing in candidates.Where(existing =>
                     ProtocolsOverlap(existing.Protocol, rule.Protocol)
                     && CidrsOverlap(existing.SourceCidr, rule.SourceCidr)))
        {
            var pair = new[] { rule.Id, existing.Id }.Order().ToArray();
            findings.Add(CreateFinding(
                rule,
                "ConflictingRule",
                AlertSeverity.Medium,
                "Conflicting firewall actions detected",
                $"Rule '{rule.Name}' conflicts with '{existing.Name}' for overlapping traffic.",
                $"conflict-{pair[0]:N}-{pair[1]:N}"));
        }

        return findings;
    }

    private bool IsTrusted(Ipv4Cidr source) =>
        options.Value.TrustedNetworks.Any(value =>
            Ipv4Cidr.TryParse(value, out var trusted) && trusted.Contains(source));

    private static bool IsTcp(string protocol) =>
        protocol.Equals("TCP", StringComparison.OrdinalIgnoreCase)
        || protocol.Equals("ANY", StringComparison.OrdinalIgnoreCase);

    private static bool ProtocolsOverlap(string left, string right) =>
        left.Equals("ANY", StringComparison.OrdinalIgnoreCase)
        || right.Equals("ANY", StringComparison.OrdinalIgnoreCase)
        || left.Equals(right, StringComparison.OrdinalIgnoreCase);

    private static bool CidrsOverlap(string left, string right) =>
        Ipv4Cidr.TryParse(left, out var leftCidr)
        && Ipv4Cidr.TryParse(right, out var rightCidr)
        && leftCidr.Overlaps(rightCidr);

    private static FirewallFinding CreateFinding(
        FirewallRule rule,
        string category,
        AlertSeverity severity,
        string title,
        string description,
        string discriminator)
    {
        var fingerprintInput = $"{rule.Id:N}|{category}|{discriminator}";
        var fingerprint = Convert.ToHexString(
            SHA256.HashData(Encoding.UTF8.GetBytes(fingerprintInput)));
        return new FirewallFinding(category, severity, title, description, fingerprint);
    }
}
