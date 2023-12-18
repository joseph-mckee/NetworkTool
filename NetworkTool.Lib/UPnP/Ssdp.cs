using System.Net;
using System.Net.Sockets;
using System.Text;
using NetworkTool.Lib.MDNS;

namespace NetworkTool.Lib.UPnP;

public delegate void SsdpReplyReceivedHandler(object sender, MdnsReply e);

public class Ssdp
{
    private const int SsdpPort = 1900;
    private const string SsdpMulticastAddressV4 = "239.255.255.250";

    private const string DiscoveryMessage = @"M-SEARCH * HTTP/1.1
HOST: 239.255.255.250:1900
MAN: ""ssdp:discover""
MX: 1
ST: ssdp:all

";

    private static readonly IPAddress SsdpMulticastIPAddressV4 = IPAddress.Parse(SsdpMulticastAddressV4);

    private bool _isListening;

    public event SsdpReplyReceivedHandler? ReplyReceived;

    public async Task StartListener()
    {
        using var ssdpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        ssdpSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);

        var localEndPoint = new IPEndPoint(IPAddress.Any, SsdpPort);
        ssdpSocket.Bind(localEndPoint);

        var multicastOption = new MulticastOption(SsdpMulticastIPAddressV4, IPAddress.Any);
        ssdpSocket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.AddMembership, multicastOption);

        var buffer = new ArraySegment<byte>(new byte[4096]);

        var requestBytes = Encoding.UTF8.GetBytes(DiscoveryMessage);

        var end = new IPEndPoint(SsdpMulticastIPAddressV4, SsdpPort);

        for (var i = 0; i < 3; i++) await ssdpSocket.SendToAsync(requestBytes, SocketFlags.None, end);

        _isListening = true;
        while (_isListening)
        {
            var receiveTask = ssdpSocket.ReceiveFromAsync(buffer, SocketFlags.None, localEndPoint);
            await Task.WhenAny(receiveTask, Task.Delay(2000));
            var bytesRead = receiveTask.Result.ReceivedBytes;
            if (bytesRead <= 0) continue;
            var endPoint = receiveTask.Result.RemoteEndPoint.ToString();
            var s = endPoint?.Split(':');
            endPoint = s?[0];
            var reply = new MdnsReply
            {
                Message = Encoding.UTF8.GetString(buffer),
                EndPoint = endPoint
            };
            ReplyReceived?.Invoke(this, reply);
        }
    }

    public void StopListener()
    {
        _isListening = false;
    }
}