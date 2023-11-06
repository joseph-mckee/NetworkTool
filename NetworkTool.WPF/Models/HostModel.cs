using CommunityToolkit.Mvvm.ComponentModel;

namespace NetworkTool.WPF.Models;

public partial class HostModel : ObservableObject
{
    [ObservableProperty] private bool _arped;

    [ObservableProperty] private string? _hostName;

    [ObservableProperty] private string? _ipAddress;

    [ObservableProperty] private string? _macAddress;

    [ObservableProperty] private bool _pinged;

    [ObservableProperty] private long _roundTripTime;

    [ObservableProperty] private string? _vendor;
}