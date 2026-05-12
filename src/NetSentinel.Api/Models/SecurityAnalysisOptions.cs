namespace NetSentinel.Api.Models;

public sealed class SecurityAnalysisOptions
{
    public const string SectionName = "SecurityAnalysis";

    public string[] TrustedNetworks { get; set; } =
    [
        "127.0.0.0/8",
        "10.0.0.0/8",
        "172.16.0.0/12",
        "192.168.0.0/16"
    ];
}
