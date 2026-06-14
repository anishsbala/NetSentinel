using NetSentinel.Api.Services;

namespace NetSentinel.Api.Tests;

public sealed class Ipv4CidrTests
{
    [Theory]
    [InlineData("10.0.0.0/8")]
    [InlineData("192.168.1.25/32")]
    [InlineData("0.0.0.0/0")]
    public void TryParse_AcceptsValidIpv4Cidr(string value)
    {
        Assert.True(Ipv4Cidr.TryParse(value, out _));
    }

    [Theory]
    [InlineData("8.8.8.8")]
    [InlineData("10.0.0.0/33")]
    [InlineData("::1/128")]
    [InlineData("invalid")]
    public void TryParse_RejectsInvalidOrUnsupportedCidr(string value)
    {
        Assert.False(Ipv4Cidr.TryParse(value, out _));
    }

    [Fact]
    public void Contains_DetectsSubnetMembership()
    {
        Assert.True(Ipv4Cidr.TryParse("10.0.0.0/8", out var trusted));
        Assert.True(Ipv4Cidr.TryParse("10.4.0.0/16", out var candidate));

        Assert.True(trusted.Contains(candidate));
    }
}
