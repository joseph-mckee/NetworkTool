using System.Collections.ObjectModel;
using System.Linq;
using System.Net.NetworkInformation;
using CommunityToolkit.Mvvm.ComponentModel;
using NetworkTool.WPF.Models;

namespace NetworkTool.WPF.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private bool _isNetworkScan;

    [ObservableProperty] private ObservableCollection<InterfaceModel> _networkInterfaces = new();

    private int _selectedIndex;

    private InterfaceModel? _selectedInterface;

    public MainViewModel()
    {
        var networkInterfaces = NetworkInterface.GetAllNetworkInterfaces();
        foreach (var networkInterface in networkInterfaces)
            if (networkInterface.OperationalStatus == OperationalStatus.Up)
                _networkInterfaces.Add(new InterfaceModel
                {
                    Name = networkInterface.Name,
                    Description = networkInterface.Description,
                    IpAddress = networkInterface.GetIPProperties().UnicastAddresses
                        .First(ip => ip.Address.GetAddressBytes().Length == 4).Address.ToString(),
                    Index = networkInterface.GetIPProperties().GetIPv4Properties().Index
                });
        SelectedIndex = 0;
        PingViewModel = new PingViewModel(this);
        TraceRouteViewModel = new TraceRouteViewModel(this);
        ScanViewModel = new ScanViewModel();
        ArpViewModel = new ArpViewModel();
        IsNetworkScan = true;
    }

    public PingViewModel PingViewModel { get; }
    public TraceRouteViewModel TraceRouteViewModel { get; }
    public ScanViewModel ScanViewModel { get; }

    public ArpViewModel ArpViewModel { get; }

    public bool IsNetworkScan
    {
        get => _isNetworkScan;
        set => SetProperty(ref _isNetworkScan, !value);
    }

    public InterfaceModel? SelectedInterface => _selectedInterface;

    public int SelectedIndex
    {
        get => _selectedIndex;
        set
        {
            SetProperty(ref _selectedInterface, NetworkInterfaces[value]);
            SetProperty(ref _selectedIndex, value);
        }
    }
}