using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NetworkTool.Lib.Arp;
using NetworkTool.WPF.Models;
using QuickScan.Lib;

namespace NetworkTool.WPF.ViewModels;

public partial class ScanViewModel : ObservableObject
{
    private readonly IEnumerable<string> _scanList;

    private readonly SemaphoreSlim _semaphore;

    private readonly VendorLookup _vendorLookup = new();

    [ObservableProperty] private ObservableCollection<HostModel> _hostModels = new();

    [ObservableProperty] private string? _scanRange;

    public ScanViewModel()
    {
        _semaphore = new SemaphoreSlim(500);
        ScanRange = "192.168.1.1-192.168.1.254";
        _scanList = NetworkInfoManager.GetIpRange(ScanRange);
    }

    [RelayCommand]
    private async Task StartScan()
    {
        await Task.Run(ArpScan);
        await Task.Run(PingScan);
    }

    private async Task ArpScan()
    {
        var tasks = _scanList.Select(async address =>
        {
            await _semaphore.WaitAsync();
            try
            {
                Debug.WriteLine($"Arping {address}");
                var mac = await Arp.SendArpAsync(address);
                if (mac != "Unknown")
                {
                    var vendor = _vendorLookup.GetVendorName(mac);
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        HostModels.Add(new HostModel
                        {
                            IpAddress = address,
                            MacAddress = mac,
                            Vendor = vendor,
                            Arped = true
                        });
                    });
                }
            }
            finally
            {
                _semaphore.Release();
            }
        }).ToList();
        await Task.WhenAll(tasks);
    }

    private async Task PingScan()
    {
        var tasks = _scanList.Select(async address =>
        {
            await _semaphore.WaitAsync();
            try
            {
                Debug.WriteLine($"Pinging {address}");
                using Ping pingSender = new();
                var reply = await pingSender.SendPingAsync(address, 500);
                if (reply.Status == IPStatus.Success)
                    try
                    {
                        var host = HostModels.First(x => x.IpAddress == address);
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            host.RoundTripTime = reply.RoundtripTime;
                            host.Pinged = true;
                        });
                    }
                    catch
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            HostModels.Add(new HostModel
                            {
                                IpAddress = address,
                                RoundTripTime = reply.RoundtripTime,
                                Pinged = true
                            });
                        });
                    }
                else
                    try
                    {
                        var host = HostModels.First(x => x.IpAddress == address);
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            host.RoundTripTime = reply.RoundtripTime;
                            host.Pinged = true;
                        });
                    }
                    catch
                    {
                        Debug.WriteLine("No living host at address.");
                    }
            }
            finally
            {
                _semaphore.Release();
            }
        }).ToList();
        await Task.WhenAll(tasks);
    }
}