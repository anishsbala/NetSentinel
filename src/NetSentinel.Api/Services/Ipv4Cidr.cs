using System.Net;
using System.Net.Sockets;

namespace NetSentinel.Api.Services;

public readonly record struct Ipv4Cidr(uint Network, uint Broadcast)
{
    public bool Contains(Ipv4Cidr other) =>
        Network <= other.Network && Broadcast >= other.Broadcast;

    public bool Overlaps(Ipv4Cidr other) =>
        Network <= other.Broadcast && other.Network <= Broadcast;

    public static bool TryParse(string? value, out Ipv4Cidr cidr)
    {
        cidr = default;
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        var parts = value.Split('/', StringSplitOptions.TrimEntries);
        if (parts.Length != 2
            || !IPAddress.TryParse(parts[0], out var address)
            || address.AddressFamily != AddressFamily.InterNetwork
            || !int.TryParse(parts[1], out var prefixLength)
            || prefixLength is < 0 or > 32)
        {
            return false;
        }

        var addressValue = ToUInt32(address);
        var mask = prefixLength == 0 ? 0U : uint.MaxValue << (32 - prefixLength);
        var network = addressValue & mask;
        var broadcast = network | ~mask;
        cidr = new Ipv4Cidr(network, broadcast);
        return true;
    }

    private static uint ToUInt32(IPAddress address)
    {
        var bytes = address.GetAddressBytes();
        return ((uint)bytes[0] << 24)
            | ((uint)bytes[1] << 16)
            | ((uint)bytes[2] << 8)
            | bytes[3];
    }
}
