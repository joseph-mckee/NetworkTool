using System.Net;
using System.Net.Sockets;

namespace NetworkTool.Lib;

public class Mdns
{
    private const int MdnsPort = 5353;
    private const string MdnsMulticastAddressV4 = "224.0.0.251";
    private static readonly IPAddress MdnsMulticastIpAddressV4 = IPAddress.Parse(MdnsMulticastAddressV4);

    private readonly List<MdnsReply> _replies = new();
    private bool _cancellationRequested;

    public void StartListener()
    {
        _cancellationRequested = false;
        using var mdnsSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        // Allow multiple sockets to use the same port number
        mdnsSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);

        // Bind the socket to the mDNS port
        var localEndPoint = new IPEndPoint(IPAddress.Any, MdnsPort);
        mdnsSocket.Bind(localEndPoint);

        // Join the multicast group
        var multicastOption = new MulticastOption(MdnsMulticastIpAddressV4, IPAddress.Any);
        mdnsSocket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.AddMembership, multicastOption);

        // Listen for multicast traffic
        var buffer = new byte[4096]; // Adjust size as needed
        EndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);

        while (_cancellationRequested != true)
        {
            var bytesRead = mdnsSocket.ReceiveFrom(buffer, ref remoteEndPoint);
            _replies.Add(new MdnsReply
            {
                Message = bytesRead.ToString(),
                EndPoint = remoteEndPoint.ToString()
            });
        }
    }

    public void StopListener()
    {
        _cancellationRequested = true;
    }

    public List<MdnsReply> GetReplies()
    {
        return _replies;
    }
}