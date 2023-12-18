using System.Diagnostics;
using System.Net;
using QuickScan.Lib;

namespace NetworkTool.Lib.IP;

public static class NameResolution
{
    public static async Task<string> ResolveHostName(IPAddress address)
    {
        string name;
        try
        {
            var dnsTask = Dns.GetHostEntryAsync(address.ToString());
            if (await Task.WhenAny(dnsTask, Task.Delay(3000)) == dnsTask)
            {
                name = dnsTask.Result.HostName;
            }
            else
            {
                var cts = new CancellationTokenSource(5000);
                var cancellationToken = cts.Token;
                name = await Snmp.GetSnmpAsync(address.ToString(), cancellationToken) ?? "Unknown";
            }
        }
        catch (Exception e)
        {
            Debug.WriteLine($"Unhandled Exception: {e.Message}");
            return "Unknown";
        }

        return name;
    }
}