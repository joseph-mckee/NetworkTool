using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NetworkTool.Lib;
using NetworkTool.WPF.Models;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Net;
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

    [ObservableProperty]
    bool isClearable;

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
            if (PingReplies.Count == 0)
            {
                return "N/A";
            }
            if (SuccessfulPings > 0 || FailedPings > 0)
            {
                float percentage = ((float)SuccessfulPings / (SuccessfulPings + FailedPings)) * 100;
                return $"{percentage}%";
            }
            else
            {
                return "0%";
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
                float average = (float)ReplyTimes / (float)SuccessfulPings;
                return $"{Math.Round(average, 2)} ms";
            }
            else
            {
                return "0 ms";
            }
        }
    }

    [ObservableProperty]
    private int progress;

    [ObservableProperty]
    private string? hostName;

    public PingViewModel()
    {
        AddressOrHostname = "8.8.8.8";
        Attempts = 4;
        IsAttempts = true;
        IsContinuous = false;
        MaxHops = 30;
        Timeout = 4000;
        BufferSize = 32;
        DelayTime = 1000;
        IsFragmentable = false;
        IsPingable = true;
        IsCancellable = false;
        SuccessfulPings = 0;
        FailedPings = 0;
        ReplyTimes = 0;
        Progress = 0;
        IsIndeterminate = false;
        IsClearable = true;
    }

    [RelayCommand]
    async Task StartPinging()
    {
        IsClearable = false;
        IsCancellable = true;
        ClearList();
        cancellationTokenSource = new CancellationTokenSource();
        var token = cancellationTokenSource.Token;
        await Task.Run(() => PreparePing(token));
        IsCancellable = false;
        IsClearable = true;
    }

    private async Task PreparePing(CancellationToken cancellationToken)
    {
        Ping pingSender = new();
        PingOptions pingOptions = new()
        {
            Ttl = MaxHops,
            DontFragment = !IsFragmentable
        };
        byte[] buffer = new byte[BufferSize];
        ResolveDnsInBackground(AddressOrHostname!, cancellationToken);
        if (IsContinuous)
        {
            IsIndeterminate = true;
            while (true)
            {
                try
                {
                    await SendPing(Progress, buffer, pingSender, pingOptions, cancellationToken);
                }
                catch (OperationCanceledException ex)
                {
                    Debug.WriteLine(ex.Message);
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
                }
                catch (OperationCanceledException ex)
                {
                    Debug.WriteLine(ex.Message);
                    break;
                }
            }
        }
    }

    async Task SendPing(int index, byte[] buffer, Ping pingSender, PingOptions pingOptions, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (string.IsNullOrEmpty(AddressOrHostname))
        {
            StopPinging();
            MessageBox.Show("Enter an address or hostname to ping.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        try
        {
            PingReplyEx reply = await Task.Run(() => PingEx.Send(IPAddress.Parse("10.0.13.24"), IPAddress.Parse(AddressOrHostname), Timeout, buffer, pingOptions ), cancellationToken);
            Application.Current.Dispatcher.Invoke(() =>
            {
                if (reply.Status == IPStatus.Success)
                {
                    SuccessfulPings++;
                }
                else
                {
                    FailedPings++;
                }
                PingReplies.Add(new PingReplyModel(reply, index + 1));
                ReplyTimes += reply.RoundTripTime;
                Progress++;
            });
        }
        catch (PingException ex)
        {
            StopPinging();
            MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        try
        {
            if (Progress < Attempts || IsContinuous)
            {
                await Task.Delay(DelayTime, cancellationToken);
            }
        }
        catch (OperationCanceledException ex)
        {
            Debug.WriteLine(ex.Message);
        }
    }

    void ResolveDnsInBackground(string address, CancellationToken cancellationToken)
    {
        Task.Run(async () =>
        {
            try
            {
                var entry = await Dns.GetHostEntryAsync(address, cancellationToken);
                Application.Current.Dispatcher.Invoke(() =>
                {
                    HostName = entry.HostName;
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }, cancellationToken);
    }

    [RelayCommand]
    void StopPinging()
    {
        IsCancellable = false;
        IsIndeterminate = false;
        IsClearable = true;
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
        HostName = null;
        IsClearable = false;
    }
}
