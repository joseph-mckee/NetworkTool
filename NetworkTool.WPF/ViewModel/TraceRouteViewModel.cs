using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NetworkTool.Lib.Ping;
using NetworkTool.WPF.Models;

namespace NetworkTool.WPF.ViewModel;

public partial class TraceRouteViewModel : ObservableObject
{
    private readonly MainViewModel _mainViewModel;
    [ObservableProperty] private string? _addressOrHostname;

    [ObservableProperty] private int _delayTime;

    [ObservableProperty] private bool _doResolveNames;

    [ObservableProperty] private int _maxHops;

    [ObservableProperty] private int _timeout;
    [ObservableProperty] private ObservableCollection<TraceRouteReplyModel> _traceRouteReplies = new();

    public TraceRouteViewModel(MainViewModel context)
    {
        AddressOrHostname = "8.8.8.8";
        MaxHops = 30;
        Timeout = 500;
        DelayTime = 0;
        _mainViewModel = context;
    }

    [RelayCommand]
    private async Task TraceRoute()
    {
        ClearList();
        for (var i = 1; i < MaxHops; i++)
        {
            PingOptions pingOptions = new()
            {
                Ttl = i,
                DontFragment = true
            };
            var buffer = new byte[32];
            if (AddressOrHostname is not null)
            {
                Stopwatch stopWatch = new();
                stopWatch.Start();
                var reply = await Task.Run(() =>
                    PingEx.Send(
                        IPAddress.Parse(_mainViewModel.SelectedInterface?.IpAddress ??
                                        throw new InvalidOperationException()),
                        IPAddress.Parse(AddressOrHostname), Timeout, buffer, pingOptions));
                stopWatch.Stop();
                if (reply.Status is IPStatus.Success or IPStatus.TtlExpired)
                {
                    string hostName;
                    if (DoResolveNames)
                        try
                        {
                            var dnsQuery = await Dns.GetHostEntryAsync(reply.IpAddress);
                            hostName = dnsQuery.HostName;
                        }
                        catch (SocketException)
                        {
                            hostName = "Unknown";
                        }
                    else
                        hostName = "Unknown";

                    TraceRouteReplies.Add(new TraceRouteReplyModel
                    {
                        Index = i,
                        IpAddress = reply.IpAddress.ToString(),
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
                        IpAddress = reply.IpAddress.ToString(),
                        RoundTripTime = "N/A",
                        Status = reply.Status.ToString()
                    });
                }
            }

            // Handle empty field
            await Task.Delay(DelayTime);
        }
    }

    [RelayCommand]
    private void ClearList()
    {
        TraceRouteReplies.Clear();
    }
}