using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NetworkTool.WPF.Models;
using QuickScan.Lib;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace NetworkTool.WPF.ViewModels;

public partial class ScanViewModel : ObservableObject
{
    [ObservableProperty]
    private ObservableCollection<HostModel> hostModels = new();

    [ObservableProperty]
    private string? scanRange;

    public ScanViewModel()
    {
        ScanRange = "192.168.1.1 - 192.168.1.255";
    }

    [RelayCommand]
    async Task StartScan()
    {
        var addressesToScan = NetworkInfoManager.GetIpRange(ScanRange);
        List<Task> tasks = new();
        foreach (var address in addressesToScan)
        {
            tasks.Add(Scan(address));
        }
        await Task.WhenAll(tasks);
    }

    private async Task Scan(string address)
    {
        Ping pingSender = new();
        var reply = await pingSender.SendPingAsync(address);
        var mac = await MacFinder.GetMacAddressAsync(address);
        if (reply.Status == IPStatus.Success || mac != "Unknown")
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                HostModels.Add(new HostModel
                {
                    HostName = reply.Status.ToString(),
                    IPAddress = address,
                    RoundTripTime = reply.RoundtripTime,
                    MACAddress = mac
                });
            });
        }
    }
}
