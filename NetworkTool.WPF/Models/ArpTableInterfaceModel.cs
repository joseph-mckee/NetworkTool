using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using CommunityToolkit.Mvvm.ComponentModel;
using NetworkTool.Lib.Arp;

namespace NetworkTool.WPF.Models;

public partial class ArpTableInterfaceModel : ObservableObject
{
    [ObservableProperty] private bool _isExpanded;

    public ArpTableInterfaceModel(NetworkInterface networkInterface)
    {
        var name = networkInterface.Name;
        var description = networkInterface.Description;
        var address = networkInterface.GetIPProperties().UnicastAddresses
            .First(ip => ip.Address.GetAddressBytes().Length == 4).Address.ToString();
        var index = networkInterface.GetIPProperties().GetIPv4Properties().Index;
        InterfaceInfo = $"{name} | {description} | {address} | {index}";
        var entries = Arp.GetArpCache();
        var relevantEntries = entries.Where(arpEntry => arpEntry.Index == index);
        var arpEntries = relevantEntries as ArpEntry[] ?? relevantEntries.ToArray();
        if (arpEntries.Length > 4)
            IsExpanded = true;
        foreach (var entry in arpEntries)
            ArpEntries?.Add(new ArpTableEntryModel
            {
                IpAddress = entry.IpAddress,
                MacAddress = entry.MacAddress
            });
    }

    public string? InterfaceInfo { get; }
    public List<ArpTableEntryModel>? ArpEntries { get; set; } = new();
}