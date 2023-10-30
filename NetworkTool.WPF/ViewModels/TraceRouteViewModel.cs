using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NetworkTool.WPF.Models;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace NetworkTool.WPF.ViewModels;

public partial class TraceRouteViewModel : ObservableObject
{
    [ObservableProperty]
    ObservableCollection<TraceRouteReplyModel> traceRouteReplies = new();

    [ObservableProperty]
    string? addressOrHostname;

    [ObservableProperty]
    int maxHops;

    [ObservableProperty]
    int timeout;

    [ObservableProperty]
    int delayTime;

    [ObservableProperty]
    bool doResolveNames;

    public TraceRouteViewModel()
    {
        AddressOrHostname = "8.8.8.8";
        MaxHops = 30;
        Timeout = 500;
        DelayTime = 0;
    }

    [RelayCommand]
    async Task TraceRoute()
    {
        ClearList();
        for (int i = 1; i < MaxHops; i++)
        {
            Ping pingSender = new Ping();
            PingOptions pingOptions = new PingOptions();
            pingOptions.Ttl = i;
            pingOptions.DontFragment = true;
            byte[] buffer = new byte[32];
            if (AddressOrHostname is not null)
            {
                Stopwatch stopWatch = new();
                stopWatch.Start();
                PingReply reply = await pingSender.SendPingAsync(AddressOrHostname, Timeout, buffer, pingOptions);
                stopWatch.Stop();
                if (reply.Status == IPStatus.Success || reply.Status == IPStatus.TtlExpired)
                {
                    string hostName;
                    if (DoResolveNames == true)
                    {
                        try
                        {
                            var dnsQuery = await Dns.GetHostEntryAsync(reply.Address);
                            hostName = dnsQuery.HostName;
                        }
                        catch (SocketException)
                        {
                            hostName = "Unknown";
                        } 
                    }
                    else
                    {
                        hostName = "Unknown";
                    }
                    TraceRouteReplies.Add(new TraceRouteReplyModel
                    {
                        Index = i,
                        IPAddress = reply.Address.ToString(),
                        HostName = hostName,
                        RoundTripTime = stopWatch.ElapsedMilliseconds.ToString(),
                        Status = reply.Status.ToString()
                    });
                    if (reply.Status == IPStatus.Success)
                        break;
                }
                else
                {
                    TraceRouteReplies.Add(new TraceRouteReplyModel
                    {
                        Index = i,
                        IPAddress = reply.Address.ToString(),
                        RoundTripTime = "N/A",
                        Status = reply.Status.ToString()
                    });
                }
            }
            else
            {
                // Handle empty field
            }
            await Task.Delay(DelayTime);
        }
    }

    [RelayCommand]
    void ClearList()
    {
        TraceRouteReplies.Clear();
    }
}
