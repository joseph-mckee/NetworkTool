using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NetworkTool.Lib.Ping;
using NetworkTool.WPF.Models;

namespace NetworkTool.WPF.ViewModels;

public partial class PingViewModel : ObservableObject
{
    private readonly MainViewModel _mainViewModel;
    private string? _addressOrHostname;

    [ObservableProperty] private int _attempts;

    [ObservableProperty] private int _bufferSize;

    private CancellationTokenSource? _cancellationTokenSource;

    [ObservableProperty] private int _delayTime;

    [ObservableProperty] [NotifyPropertyChangedFor(nameof(SuccessPercentage))]
    private long _failedPings;

    [ObservableProperty] private string? _hostName;

    [ObservableProperty] private bool _isAttempts;

    [ObservableProperty] private bool _isCancellable;

    [ObservableProperty] private bool _isClearable;

    private bool _isContinuous;

    [ObservableProperty] private bool _isFragmentable;

    [ObservableProperty] private bool _isIndeterminate;

    [ObservableProperty] private bool _isPingable;

    [ObservableProperty] private int _maxHops;
    [ObservableProperty] private ObservableCollection<PingReplyModel> _pingReplies = new();

    [ObservableProperty] private int _progress;

    [ObservableProperty] [NotifyPropertyChangedFor(nameof(AverageTime))]
    private long _replyTimes;

    [ObservableProperty] [NotifyPropertyChangedFor(nameof(SuccessPercentage))]
    private long _successfulPings;

    [ObservableProperty] private int _timeout;

    public PingViewModel(MainViewModel context)
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
        _mainViewModel = context;
    }

    public string? AddressOrHostname
    {
        get => _addressOrHostname;
        set
        {
            SetProperty(ref _addressOrHostname, value);
            IsPingable = !string.IsNullOrEmpty(value);
        }
    }

    public bool IsContinuous
    {
        get => _isContinuous;
        set
        {
            SetProperty(ref _isContinuous, value);
            IsAttempts = !value;
        }
    }

    public string SuccessPercentage
    {
        get
        {
            if (PingReplies.Count == 0) return "N/A";
            if (SuccessfulPings <= 0 && FailedPings <= 0) return "0%";
            var percentage = (float)SuccessfulPings / (SuccessfulPings + FailedPings) * 100;
            return $"{percentage}%";
        }
    }

    public string AverageTime
    {
        get
        {
            if (PingReplies.Count <= 0) return "0 ms";
            if (ReplyTimes == 0) return "0 ms";
            var average = ReplyTimes / (float)SuccessfulPings;
            return $"{Math.Round(average, 2)} ms";
        }
    }

    [RelayCommand]
    private async Task StartPinging()
    {
        IsClearable = false;
        IsCancellable = true;
        ClearList();
        _cancellationTokenSource = new CancellationTokenSource();
        var token = _cancellationTokenSource.Token;
        await Task.Run(() => PreparePing(token), token);
        IsCancellable = false;
        IsClearable = true;
    }

    private async Task PreparePing(CancellationToken cancellationToken)
    {
        PingOptions pingOptions = new()
        {
            Ttl = MaxHops,
            DontFragment = !IsFragmentable
        };
        var buffer = new byte[BufferSize];
        ResolveDnsInBackground(AddressOrHostname!, cancellationToken);
        if (IsContinuous)
        {
            IsIndeterminate = true;
            while (true)
                try
                {
                    await SendPing(Progress, buffer, pingOptions, cancellationToken);
                }
                catch (OperationCanceledException ex)
                {
                    Debug.WriteLine(ex.Message);
                    break;
                }
        }
        else
        {
            for (var i = 0; i < Attempts; i++)
                try
                {
                    await SendPing(i, buffer, pingOptions, cancellationToken);
                }
                catch (OperationCanceledException ex)
                {
                    Debug.WriteLine(ex.Message);
                    break;
                }
        }
    }

    private async Task SendPing(int index, byte[]? buffer, PingOptions? pingOptions,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (string.IsNullOrEmpty(AddressOrHostname))
        {
            StopPinging();
            MessageBox.Show("Enter an address or hostname to ping.", "Warning", MessageBoxButton.OK,
                MessageBoxImage.Warning);
            return;
        }

        try
        {
            var reply = await Task.Run(
                () => PingEx.Send(
                    IPAddress.Parse(_mainViewModel.SelectedInterface?.IpAddress ??
                                    throw new InvalidOperationException()),
                    IPAddress.Parse(AddressOrHostname), Timeout, buffer, pingOptions), cancellationToken);
            Application.Current.Dispatcher.Invoke(() =>
            {
                if (reply.Status == IPStatus.Success)
                    SuccessfulPings++;
                else
                    FailedPings++;
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
            if (Progress < Attempts || IsContinuous) await Task.Delay(DelayTime, cancellationToken);
        }
        catch (OperationCanceledException ex)
        {
            Debug.WriteLine(ex.Message);
        }
    }

    private void ResolveDnsInBackground(string address, CancellationToken cancellationToken)
    {
        Task.Run(async () =>
        {
            try
            {
                var entry = await Dns.GetHostEntryAsync(address, cancellationToken);
                Application.Current.Dispatcher.Invoke(() => { HostName = entry.HostName; });
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }, cancellationToken);
    }

    [RelayCommand]
    private void StopPinging()
    {
        IsCancellable = false;
        IsIndeterminate = false;
        IsClearable = true;
        Progress = Attempts;
        _cancellationTokenSource?.Cancel();
    }

    [RelayCommand]
    private void ClearList()
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