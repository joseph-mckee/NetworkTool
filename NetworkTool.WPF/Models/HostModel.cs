using CommunityToolkit.Mvvm.ComponentModel;

namespace NetworkTool.WPF.Models
{
    public partial class HostModel : ObservableObject
    {
        [ObservableProperty]
        private string? hostName;

        [ObservableProperty]
        private string? iPAddress;

        [ObservableProperty]
        private long roundTripTime;

        [ObservableProperty]
        private string? mACAddress;

        [ObservableProperty]
        private string? vendor;

        [ObservableProperty]
        private bool arped;

        [ObservableProperty]
        private bool pinged;
    }
}
