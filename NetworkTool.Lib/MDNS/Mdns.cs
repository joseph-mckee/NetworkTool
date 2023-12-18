using System.Net;
using System.Net.Sockets;

namespace NetworkTool.Lib.MDNS;

public delegate void MdnsReplyReceivedHandler(object sender, MdnsReply e);

public class Mdns
{
    private const int MdnsPort = 5353;
    private const string MdnsMulticastAddressV4 = "224.0.0.251";
    private static readonly IPAddress MdnsMulticastIpAddressV4 = IPAddress.Parse(MdnsMulticastAddressV4);

    private readonly List<MdnsReply> _replies = new();

    public event MdnsReplyReceivedHandler? ReplyReceived;

    public async Task StartListenerAsync(CancellationToken cancellationToken)
    {
        using var mdnsSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        mdnsSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);

        var localEndPoint = new IPEndPoint(IPAddress.Any, MdnsPort);
        mdnsSocket.Bind(localEndPoint);

        var multicastOption = new MulticastOption(MdnsMulticastIpAddressV4, IPAddress.Any);
        mdnsSocket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.AddMembership, multicastOption);

        var buffer = new ArraySegment<byte>(new byte[4096]); // Adjust size as needed

        while (!cancellationToken.IsCancellationRequested)
        {
            var receiveTask = mdnsSocket.ReceiveFromAsync(buffer, SocketFlags.None, localEndPoint);

            // Wait for the ReceiveFromAsync method to complete or the cancellation to be requested
            var completedTask = await Task.WhenAny(receiveTask, Task.Delay(1000, cancellationToken));

            if (cancellationToken.IsCancellationRequested)
                break;

            var bytesRead = receiveTask.Result.ReceivedBytes;
            if (bytesRead <= 0) continue;
            var endPoint = receiveTask.Result.RemoteEndPoint.ToString();
            var s = endPoint?.Split(':');
            endPoint = s?[0];
            var reply = new MdnsReply
            {
                Message = bytesRead.ToString(),
                EndPoint = endPoint
            };

            _replies.Add(reply);
            ReplyReceived?.Invoke(this, reply);
        }

        mdnsSocket.Close();
    }

    public List<MdnsReply> GetReplies()
    {
        return _replies;
    }
}