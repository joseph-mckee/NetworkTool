using CommunityToolkit.Mvvm.ComponentModel;

namespace NetworkTool.WPF.Models;

public partial class HostModel : ObservableObject
{
    [ObservableProperty] private string? _hostName;
    [ObservableProperty] private int _index;

    [ObservableProperty] private string? _ipAddress;

    [ObservableProperty] private string? _macAddress;

    [ObservableProperty] private string? _vendor;
}