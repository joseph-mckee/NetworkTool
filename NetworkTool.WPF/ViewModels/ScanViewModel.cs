using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NetworkTool.Lib;
using NetworkTool.WPF.Models;
using QuickScan.Lib;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace NetworkTool.WPF.ViewModels;

public partial class ScanViewModel : ObservableObject
{
    [ObservableProperty]
    private ObservableCollection<HostModel> hostModels = new();

    private readonly SemaphoreSlim semaphore;

    private VendorLookup vendorLookup = new();

    [ObservableProperty]
    private string? scanRange;

    private readonly IEnumerable<string> scanList;

    public ScanViewModel()
    {
        semaphore = new SemaphoreSlim(500);
        ScanRange = "192.168.1.1-192.168.1.254";
        scanList = NetworkInfoManager.GetIpRange(ScanRange);
    }

    [RelayCommand]
    async Task StartScan()
    {
        //StringBuilder display = new StringBuilder();

        //foreach (var iface in NetworkInterface.GetAllNetworkInterfaces())
        //{
        //    try
        //    {
        //        var ipProperties = iface.GetIPProperties();
        //        var ipv4Props = ipProperties.GetIPv4Properties();
        //        if (ipv4Props != null)
        //        {
        //            int index = ipv4Props.Index;
        //            var entries = ARP.GetARPCache().Where(i => i.Index == index);
        //            var ipv4Address = ipProperties.UnicastAddresses
        //                .FirstOrDefault(a => a.Address.GetAddressBytes().Length == 4)?.Address;

        //            if (ipv4Address != null)
        //            {
        //                display.AppendLine($"{iface.Name} | {iface.Description} | {ipv4Address}");
        //                foreach (var entry in entries)
        //                {
        //                    display.AppendLine($"{entry.IPAddress} | {entry.MACAddress}");
        //                }
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        // Handle the exception, log it, or display a message to the user
        //        display.AppendLine($"Error retrieving information for interface {iface.Name}: {ex.Message}");
        //    }
        //}

        //MessageBox.Show(display.ToString());


        //Debug.WriteLine(ARP.GetARPCache());
        await Task.Run(ARPScan);
        await Task.Run(PingScan);
    }

    private async Task ARPScan()
    {
        var tasks = scanList.Select(async address =>
        {
            await semaphore.WaitAsync();
            try
            {
                Debug.WriteLine($"Arping {address}");
                var mac = await ARP.SendARPAsync(address);
                if (mac != "Unknown")
                {
                    var vendor = vendorLookup.GetVendorName(mac);
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        HostModels.Add(new HostModel
                        {
                            IPAddress = address,
                            MACAddress = mac,
                            Vendor = vendor,
                            Arped = true
                        });
                    });
                }
            }
            catch (System.Exception)
            {

                throw;
            }
            finally { semaphore.Release(); }
        }).ToList();
        await Task.WhenAll(tasks);
    }

    private async Task PingScan()
    {
        var tasks = scanList.Select(async address =>
        {
            await semaphore.WaitAsync();
            try
            {
                Debug.WriteLine($"Pinging {address}");
                using Ping pingSender = new();
                var reply = await pingSender.SendPingAsync(address, 500);
                if (reply.Status == IPStatus.Success)
                {
                    try
                    {
                        var host = HostModels.First(x => x.IPAddress == address);
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
                                IPAddress = address,
                                RoundTripTime = reply.RoundtripTime,
                                Pinged = true
                            });
                        });
                    }
                }
                else
                {
                    try
                    {
                        var host = HostModels.First(x => x.IPAddress == address);
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
            }
            catch (System.Exception)
            {

                throw;
            }
            finally { semaphore.Release(); }
        }).ToList();
        await Task.WhenAll(tasks);
    }
}
