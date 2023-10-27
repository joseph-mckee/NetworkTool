using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NetworkTool.WPF.Models;
using System;
using System.Collections.ObjectModel;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace NetworkTool.WPF.ViewModels;

public partial class PingViewModel : ObservableObject
{
    [ObservableProperty]
    ObservableCollection<PingReplyModel> pingReplies = new();

    private string? addressOrHostname;

    public string? AddressOrHostname
    {
        get => addressOrHostname;
        set
        {
            SetProperty(ref addressOrHostname, value);
            if (value is null || value == string.Empty)
                IsPingable = false;
            else
                IsPingable = true;
        }
    }

    [ObservableProperty]
    int attempts;

    [ObservableProperty]
    bool isAttempts;

    bool isContinuous;

    public bool IsContinuous
    {
        get => isContinuous;
        set
        {
            SetProperty(ref isContinuous, value);
            IsAttempts = !value;
        }
    }

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

    [ObservableProperty]
    bool isPingable;

    [ObservableProperty]
    bool isCancellable;

    private CancellationTokenSource? cancellationTokenSource;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SuccessPercentage))]
    private long successfulPings;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SuccessPercentage))]
    private long failedPings;

    [ObservableProperty]
    private bool isIndeterminate;

    public string SuccessPercentage
    {
        get
        {
            // Check if PingReplies.Count is not zero to avoid division by zero
            if (PingReplies.Count == 0)
            {
                return "N/A"; // Or some other appropriate string
            }

            // Check if SuccessfulPings and PingReplies.Count are numbers
            if (SuccessfulPings > 0 && PingReplies.Count > 0)
            {
                float percentage = (SuccessfulPings / PingReplies.Count) * 100;
                return $"{percentage}%";
            }
            else
            {
                return "0%"; // Or some other appropriate string
            }
        }
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(AverageTime))]
    private long replyTimes;

    public string AverageTime
    {
        get
        {
            if (PingReplies.Count > 0)
            {
                if (ReplyTimes == 0)
                {
                    return "0 ms";
                }
                float average = ReplyTimes / PingReplies.Count;
                return $"{average} ms";
            }
            else
            {
                return "0 ms";
            }
        }
    }

    [ObservableProperty]
    private int progress;


    public PingViewModel()
    {
        AddressOrHostname = "8.8.8.8";
        Attempts = 4;
        IsAttempts = true;
        IsContinuous = false;
        MaxHops = 30;
        Timeout = 4000;
        BufferSize = 32;
        DelayTime = 100;
        IsFragmentable = false;
        IsPingable = true;
        IsCancellable = false;
        SuccessfulPings = 0;
        FailedPings = 0;
        ReplyTimes = 0;
        Progress = 0;
        IsIndeterminate = false;
    }

    [RelayCommand]
    async Task StartPinging()
    {
        IsCancellable = true;
        ClearList();
        cancellationTokenSource = new CancellationTokenSource();
        var token = cancellationTokenSource.Token;
        await Task.Run(() => Ping(token), token);
        IsCancellable = false;
    }

    private async Task Ping(CancellationToken cancellationToken)
    {
        Ping pingSender = new Ping();
        PingOptions pingOptions = new PingOptions
        {
            Ttl = MaxHops,
            DontFragment = !IsFragmentable
        };
        byte[] buffer = new byte[BufferSize];

        if (IsContinuous)
        {
            IsIndeterminate = true;
            int i = 0;
            while (true)
            {
                try
                {
                    await SendPing(i, buffer, pingSender, pingOptions, cancellationToken);
                    i++;
                    Progress++;
                }
                catch (OperationCanceledException ex)
                {
                    Console.WriteLine(ex.Message);
                    break;
                }
            }
        }
        else
        {
            for (int i = 0; i < Attempts; i++)
            {
                try
                {
                    await SendPing(i, buffer, pingSender, pingOptions, cancellationToken);
                    Progress++;
                }
                catch (OperationCanceledException ex)
                {
                    Console.WriteLine(ex.Message);
                    break;
                }
            }
        }
    }

    async Task SendPing(int index, byte[] buffer, Ping pingSender, PingOptions pingOptions, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (AddressOrHostname is not null && AddressOrHostname != string.Empty)
        {
            try
            {
                PingReply reply = await pingSender.SendPingAsync(AddressOrHostname, Timeout, buffer, pingOptions);
                Application.Current.Dispatcher.Invoke(() =>
                {
                    PingReplies.Add(new PingReplyModel(reply, index + 1));
                    ReplyTimes += reply.RoundtripTime;
                    if (reply.Status == IPStatus.Success)
                    {
                        SuccessfulPings++;
                    }
                    else
                    {
                        FailedPings++;
                    }
                });
            }
            catch (PingException ex)
            {
                StopPinging();
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        else
        {
            StopPinging();
            MessageBox.Show("Enter an address or hostname to ping.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
        try
        {
            await Task.Delay(DelayTime, cancellationToken);  // Use cancellation token with delay
        }
        catch (OperationCanceledException ex)
        {
            Console.WriteLine(ex.Message);
        }
    }

    [RelayCommand]
    void StopPinging()
    {
        IsCancellable = false;
        IsIndeterminate = false;
        Progress = Attempts;
        cancellationTokenSource?.Cancel();
    }


    [RelayCommand]
    void ClearList()
    {
        PingReplies.Clear();
        SuccessfulPings = 0;
        FailedPings = 0;
        ReplyTimes = 0;
        Progress = 0;
    }
}
