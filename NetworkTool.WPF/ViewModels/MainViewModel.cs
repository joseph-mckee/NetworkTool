using CommunityToolkit.Mvvm.ComponentModel;
using NetworkTool.WPF.Models;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.NetworkInformation;

namespace NetworkTool.WPF.ViewModels;

public partial class MainViewModel : ObservableObject
{
    public PingViewModel PingViewModel { get; set; }
    public TraceRouteViewModel TraceRouteViewModel { get; set; }
    public ScanViewModel ScanViewModel { get; set; }

    private bool isNetworkScan;

    public bool IsNetworkScan
    {
        get => isNetworkScan;
        set
        {
            SetProperty(ref isNetworkScan, !value);
        }
    }

    [ObservableProperty]
    private ObservableCollection<InterfaceModel> networkInterfaces = new();

    private InterfaceModel selectedInterface;

    public InterfaceModel SelectedInterface
    {
        get
        {
            return selectedInterface;
        }
        set
        {
            SetProperty(ref selectedInterface, value);
        }
    }

    private int selectedIndex;

    public int SelectedIndex
    {
        get
        {
            return selectedIndex;
        }
        set
        {
            SetProperty(ref selectedInterface, NetworkInterfaces[value]);
            SetProperty(ref selectedIndex, value);
        }
    }
    public MainViewModel()
    {
        var nics = NetworkInterface.GetAllNetworkInterfaces();
        foreach (NetworkInterface nic in nics)
        {
            if (nic.OperationalStatus == OperationalStatus.Up)
            {
                networkInterfaces.Add(new InterfaceModel
                {
                    Name = nic.Name,
                    Description = nic.Description,
                    IPAddress = nic.GetIPProperties().UnicastAddresses.First(ip => ip.Address.GetAddressBytes().Length == 4).Address.ToString()
                });
            }
        }
        SelectedIndex = 0;
        PingViewModel = new PingViewModel(this);
        TraceRouteViewModel = new TraceRouteViewModel(this);
        ScanViewModel = new ScanViewModel();
        IsNetworkScan = true;
    }
}

