using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NetworkTool.Lib.Arp;
using NetworkTool.Lib.IP;
using NetworkTool.Lib.TCP;
using NetworkTool.WPF.Models;
using QuickScan.Lib;

namespace NetworkTool.WPF.ViewModel;

public partial class ScanViewModel : ObservableObject
{
    private readonly VendorLookup _vendorLookup = new();

    [ObservableProperty] private int _goal;

    [ObservableProperty] private ObservableCollection<HostModel> _hostModels = new();

    [ObservableProperty] private int _progress;

    [ObservableProperty] private string? _rangeInput;

    public ScanViewModel()
    {
        _rangeInput = "192.168.1.1-192.168.1.254";
        _progress = 0;
        _goal = 1;
    }

    private void Reset()
    {
        HostModels.Clear();
        Progress = 0;
    }

    [RelayCommand]
    private async Task StartScan()
    {
        Reset();
        var range = RangeInput.Split('-');
        var scanRange = AddressInformation.GetAddressRange(range[0], range[1]);
        // var cts = new CancellationTokenSource();
        // var token = cts.Token;
        // var mdns = new Mdns();
        // mdns.StartListenerAsync(token);
        // mdns.ReplyReceived += async (sender, reply) =>
        // {
        //     var info = new AddressInformation(reply.EndPoint, "255.255.255.0");
        //     var info1 = new AddressInformation(range[0], "255.255.255.0");
        //     if (Equals(info.NetworkAddress, info1.NetworkAddress)) await AddHost(IPAddress.Parse(reply.EndPoint));
        // };
        Goal = AddressInformation.GetNumberOfAddressesInRange(range[0], range[1]);
        await Parallel.ForEachAsync(scanRange, new ParallelOptions { MaxDegreeOfParallelism = 300 },
            async (address, _) =>
            {
                using var tcp = new Tcp();
                var result = await tcp.ScanHostAsync(address);
                if (result)
                {
                    await AddHost(address);
                }
                else
                {
                    using var ping = new Ping();
                    var pingOptions = new PingOptions
                    {
                        Ttl = 32,
                        DontFragment = true
                    };
                    var reply = await ping.SendPingAsync(address, 1000, new byte[8], pingOptions);
                    if (reply.Status == IPStatus.Success) await AddHost(address);
                }

                Application.Current.Dispatcher.Invoke(() => { Progress++; });
            });
        // cts.Cancel();
    }


    private async Task AddHost(IPAddress address)
    {
        // If the host already exists do nothing
        if (HostModels.FirstOrDefault(x => x.IpAddress == address.ToString()) is not null) return;

        var macAddress = Arp.GetArpEntry(address)?.MacAddress ?? "Unknown";
        var vendor = _vendorLookup.GetVendorName(macAddress);
        var name = await NameResolution.ResolveHostName(address);
        Application.Current.Dispatcher.Invoke(() =>
        {
            HostModels.Add(new HostModel
            {
                IpAddress = address.ToString(),
                MacAddress = macAddress,
                Vendor = vendor,
                HostName = name
            });
        });
    }

    // private async Task ArpScan(IEnumerable<IPAddress> ipAddresses)
    // {
    //     var tasks = ipAddresses.Select(async address =>
    //     {
    //         await _semaphore.WaitAsync();
    //         try
    //         {
    //             Debug.WriteLine($"Arping {address}");
    //             var mac = await Arp.SendArpAsync(address, NetworkInfoManager.GetCurrentNetworkInfo().Address);
    //             if (mac != "Unknown")
    //             {
    //                 var vendor = _vendorLookup.GetVendorName(mac);
    //                 var host = new HostModel
    //                 {
    //                     IpAddress = address.ToString(),
    //                     MacAddress = mac,
    //                     Vendor = vendor,
    //                     Arped = true
    //                 };
    //                 AddHost(host, ScanType.Arp);
    //             }
    //         }
    //         finally
    //         {
    //             _semaphore.Release();
    //         }
    //     }).ToList();
    //     await Task.WhenAll(tasks);
    // }

    // private async Task PingScan(IEnumerable<IPAddress> ipAddresses)
    // {
    //     var tasks = ipAddresses.Select(async address =>
    //     {
    //         await _semaphore.WaitAsync();
    //         try
    //         {
    //             Debug.WriteLine($"Pinging {address}");
    //             using Ping pingSender = new();
    //             var reply = await pingSender.SendPingAsync(address, 500);
    //             if (reply.Status != IPStatus.TimedOut)
    //             {
    //                 var host = new HostModel
    //                 {
    //                     IpAddress = address.ToString(),
    //                     RoundTripTime = reply.RoundtripTime,
    //                     Pinged = true
    //                 };
    //                 AddHost(host, ScanType.Ping);
    //             }
    //             else
    //             {
    //                 
    //             }
    //         }
    //         finally
    //         {
    //             _semaphore.Release();
    //         }
    //     }).ToList();
    //     await Task.WhenAll(tasks);
    // }
}