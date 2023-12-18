using System.Net;
using System.Net.Sockets;

namespace NetworkTool.Lib.TCP;

public class Tcp : IDisposable
{
    private readonly TcpClient _tcpClient = new();

    public Tcp()
    {
        _tcpClient.SendTimeout = 1000;
    }

    public void Dispose()
    {
        _tcpClient.Dispose();
    }

    public async Task<bool> ScanHostAsync(IPAddress target, int port = 80)
    {
        try
        {
            await _tcpClient.ConnectAsync(target.ToString(), port);
            return _tcpClient.Connected;
        }
        catch (SocketException e)
        {
            if (e.SocketErrorCode == SocketError.ConnectionRefused) return true;
        }

        return false;
    }
}