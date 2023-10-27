using CommunityToolkit.Mvvm.ComponentModel;

namespace NetworkTool.WPF.ViewModels;

public partial class MainViewModel : ObservableObject
{
    public PingViewModel PingViewModel { get; set; }
    public TraceRouteViewModel TraceRouteViewModel { get; set; }

    [ObservableProperty]
    public string? currentLog;

    public MainViewModel()
    {
        PingViewModel = new PingViewModel();
        TraceRouteViewModel = new TraceRouteViewModel();

    }
}

