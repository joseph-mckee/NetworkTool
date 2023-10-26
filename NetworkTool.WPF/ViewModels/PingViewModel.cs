using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NetworkTool.WPF.Models;
using System.Collections.ObjectModel;
using System.Net.NetworkInformation;
using System.Threading.Tasks;

namespace NetworkTool.WPF.ViewModels;

public partial class PingViewModel : ObservableObject
{
    [ObservableProperty]
    ObservableCollection<PingReplyModel> pingReplies = new();

    [ObservableProperty]
    string? addressOrHostname;

    [ObservableProperty]
    int attempts;

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

    public PingViewModel()
    {
        AddressOrHostname = "8.8.8.8";
        Attempts = 4;
        MaxHops = 30;
        Timeout = 4000;
        BufferSize = 32;
        DelayTime = 0;
        IsFragmentable = false;
    }

    [RelayCommand]
    async Task StartPinging()
    {
        for (int i = 0; i < Attempts; i++)
        {
            Ping pingSender = new Ping();
            PingOptions pingOptions = new PingOptions();
            pingOptions.Ttl = MaxHops;
            pingOptions.DontFragment = !IsFragmentable;
            byte[] buffer = new byte[BufferSize];
            PingReply reply;
            if (AddressOrHostname is not null)
            {
                reply = await pingSender.SendPingAsync(AddressOrHostname, Timeout, buffer, pingOptions);
                PingReplies.Add(new PingReplyModel(reply, i + 1));
            } 
            else
            {
                // Handle empty field
            }
            await Task.Delay(DelayTime);
        }
    }

    [RelayCommand]
    void ClearList()
    {
        PingReplies.Clear();
    }
}
