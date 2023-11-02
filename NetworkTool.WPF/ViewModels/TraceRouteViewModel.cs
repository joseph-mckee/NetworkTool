using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NetworkTool.Lib;
using NetworkTool.WPF.Models;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace NetworkTool.WPF.ViewModels;

public partial class TraceRouteViewModel : ObservableObject
{
    [ObservableProperty]
    ObservableCollection<TraceRouteReplyModel> traceRouteReplies = new();

    private CancellationTokenSource? cancellationTokenSource;

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

    private MainViewModel MainViewModel;

    public TraceRouteViewModel(MainViewModel context)
    {
        AddressOrHostname = "8.8.8.8";
        MaxHops = 30;
        Timeout = 500;
        DelayTime = 0;
        MainViewModel = context;
    }

    [RelayCommand]
    async Task TraceRoute()
    {
        ClearList();
        for (int i = 1; i < MaxHops; i++)
        {
            PingOptions pingOptions = new PingOptions();
            pingOptions.Ttl = i;
            pingOptions.DontFragment = true;
            byte[] buffer = new byte[32];
            if (AddressOrHostname is not null)
            {
                Stopwatch stopWatch = new();
                stopWatch.Start();
                PingReplyEx reply = await Task.Run(() => PingEx.Send(IPAddress.Parse(MainViewModel.SelectedInterface.IPAddress), IPAddress.Parse(AddressOrHostname), Timeout, buffer, pingOptions));
                stopWatch.Stop();
                if (reply.Status == IPStatus.Success || reply.Status == IPStatus.TtlExpired)
                {
                    string hostName;
                    if (DoResolveNames == true)
                    {
                        try
                        {
                            var dnsQuery = await Dns.GetHostEntryAsync(reply.IpAddress);
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
                        IPAddress = reply.IpAddress.ToString(),
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
                        IPAddress = reply.IpAddress.ToString(),
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
