using System.Collections.ObjectModel;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NetworkTool.Lib.Arp;
using NetworkTool.WPF.Models;

namespace NetworkTool.WPF.ViewModel;

public partial class ArpViewModel : ObservableObject
{
    [ObservableProperty] private string? _arpTable;
    [ObservableProperty] private ObservableCollection<ArpTableInterfaceModel> _arpTableInterfaceModels = new();

    public ArpViewModel()
    {
        UpdateArpTable();
    }

    [RelayCommand]
    private void UpdateArpTable()
    {
        ArpTableInterfaceModels.Clear();
        var networkInterfaces = NetworkInterface.GetAllNetworkInterfaces();
        foreach (var networkInterface in networkInterfaces)
            ArpTableInterfaceModels.Add(new ArpTableInterfaceModel(networkInterface));
    }

    [RelayCommand]
    private async Task FlushArpTable()
    {
        await Task.Run(Arp.FlushArpTable);
        UpdateArpTable();
    }
}