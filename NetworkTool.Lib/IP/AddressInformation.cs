using System.Net;

namespace NetworkTool.Lib.IP;

public class AddressInformation
{
    private readonly uint _ipAddress;
    private readonly uint _subnetMask;

    private AddressInformation(uint ipAddress, uint subnetMask)
    {
        _ipAddress = ipAddress;
        _subnetMask = subnetMask;
        NetworkAddress = GetNetworkAddress();
        BroadcastAddress = GetBroadcastAddress();
    }

    // ReSharper disable once MemberCanBePrivate.Global
    public AddressInformation(IPAddress ipAddress, IPAddress subnetMask)
        : this(IPMath.IPToBits(ipAddress), IPMath.IPToBits(subnetMask))
    {
    }

    public AddressInformation(string ipAddress, string subnetMask)
        : this(IPAddress.Parse(ipAddress), IPAddress.Parse(subnetMask))
    {
    }

    public IPAddress NetworkAddress { get; }
    public IPAddress BroadcastAddress { get; }


    // This enumerates the range and should not be used.
    public static bool IsAddressInRange(IPAddress address, List<IPAddress> range)
    {
        var addressBits = IPMath.IPToBits(address);
        var firstBits = IPMath.IPToBits(range.First());
        var lastBits = IPMath.IPToBits(range.Last());
        return addressBits <= lastBits && addressBits >= firstBits;
    }

    private static IEnumerable<IPAddress> GetAddressRange(IPAddress startOfRange, IPAddress endOfRange)
    {
        var startOfRangeBits = IPMath.IPToBits(startOfRange);
        var endOfRangeBits = IPMath.IPToBits(endOfRange);
        for (var addressBits = startOfRangeBits; addressBits <= endOfRangeBits; addressBits++)
            yield return IPMath.BitsToIP(addressBits);
    }

    public static IEnumerable<IPAddress> GetAddressRange(string startOfRange, string endOfRange)
    {
        return GetAddressRange(IPAddress.Parse(startOfRange), IPAddress.Parse(endOfRange));
    }

    // ReSharper disable once MemberCanBePrivate.Global
    public static int GetNumberOfAddressesInRange(IPAddress startOfRange, IPAddress endOfRange)
    {
        var startOfRangeBits = IPMath.IPToBits(startOfRange);
        var endOfRangeBits = IPMath.IPToBits(endOfRange);
        return (int)(endOfRangeBits - startOfRangeBits + 1);
    }

    public static int GetNumberOfAddressesInRange(string startOfRange, string endOfRange)
    {
        return GetNumberOfAddressesInRange(IPAddress.Parse(startOfRange), IPAddress.Parse(endOfRange));
    }

    public IEnumerable<IPAddress> GetAddressRangeFromNetwork()
    {
        var startOfRangeBits = GetNetworkAddressAsBits() + 1;
        var endOfRangeBits = GetBroadcastAddressAsBits() - 1;
        for (var addressBits = startOfRangeBits; addressBits <= endOfRangeBits; addressBits++)
            yield return IPMath.BitsToIP(addressBits);
    }

    private uint GetNetworkAddressAsBits()
    {
        return _subnetMask & _ipAddress;
    }

    private IPAddress GetNetworkAddress()
    {
        return IPMath.BitsToIP(GetNetworkAddressAsBits());
    }

    private uint GetBroadcastAddressAsBits()
    {
        return ~_subnetMask | GetNetworkAddressAsBits();
    }

    private IPAddress GetBroadcastAddress()
    {
        return IPMath.BitsToIP(GetBroadcastAddressAsBits());
    }
}