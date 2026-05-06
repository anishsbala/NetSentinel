namespace NetSentinel.Api.Models;

public enum ScanStatus
{
    Completed,
    Failed
}

public enum FirewallAction
{
    Allow,
    Deny
}

public enum FirewallDirection
{
    Inbound,
    Outbound
}

public enum AlertSeverity
{
    Low,
    Medium,
    High,
    Critical
}
