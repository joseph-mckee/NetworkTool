using NetworkTool.Lib.UPnP;

namespace Testing;

public static class Program
{
    public static async Task Main(string[] args)
    {
        var ssdp = new Ssdp();
        ssdp.ReplyReceived += (sender, reply) =>
        {
            Console.WriteLine(reply.EndPoint);
            Console.WriteLine(reply.Message);
        };
        await ssdp.StartListener();
        await Task.Delay(500000);
        ssdp.StopListener();
    }
}