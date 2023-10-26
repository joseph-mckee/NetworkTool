using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NetworkTool.WPF.Models;
using System.Collections.ObjectModel;
using System.Net.NetworkInformation;
using System.Threading.Tasks;

namespace NetworkTool.WPF.ViewModels;

public partial class TraceRouteViewModel : ObservableObject
{
    [ObservableProperty]
    ObservableCollection<PingReplyModel> pingReplies = new();

    [ObservableProperty]
    string? addressOrHostname;

    [ObservableProperty]
    int maxHops;

    [ObservableProperty]
    int timeout;

    [ObservableProperty]
    int bufferSize;

    [ObservableProperty]
    int delayTime;

    [ObservableProperty]
    bool isFragmentable;

    public TraceRouteViewModel()
    {
        AddressOrHostname = "8.8.8.8";
        MaxHops = 30;
        Timeout = 4000;
        BufferSize = 32;
        DelayTime = 500;
        IsFragmentable = false;
    }

    [RelayCommand]
    async Task TraceRoute()
    {
        for (int i = 1; i < MaxHops; i++)
        {
            Ping pingSender = new Ping();
            PingOptions pingOptions = new PingOptions();
            pingOptions.Ttl = i;
            pingOptions.DontFragment = !IsFragmentable;
            byte[] buffer = new byte[BufferSize];
            PingReply reply;
            if (AddressOrHostname is not null)
            {
                reply = await pingSender.SendPingAsync(AddressOrHostname, Timeout, buffer, pingOptions);
            }
            else
            {
                // Handle empty field
            }
            await Task.Delay(DelayTime);
        }
    }
}
