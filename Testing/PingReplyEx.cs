﻿using System.ComponentModel;
using System.Net;
using System.Net.NetworkInformation;

[Serializable]
public class PingReplyEx
{
    private Win32Exception _exception;


    internal PingReplyEx(uint nativeCode, int replystatus, IPAddress ipAddress, TimeSpan duration)
    {
        NativeCode = nativeCode;
        IpAddress = ipAddress;
        if (Enum.IsDefined(typeof(IPStatus), replystatus))
            Status = (IPStatus)replystatus;
    }

    internal PingReplyEx(uint nativeCode, int replystatus, IPAddress ipAddress, int roundTripTime, byte[] buffer)
    {
        NativeCode = nativeCode;
        IpAddress = ipAddress;
        RoundTripTime = (uint)roundTripTime;
        Buffer = buffer;
        if (Enum.IsDefined(typeof(IPStatus), replystatus))
            Status = (IPStatus)replystatus;
    }


    /// <summary>Native result from <code>IcmpSendEcho2Ex</code>.</summary>
    public uint NativeCode { get; }

    public IPStatus Status { get; } = IPStatus.Unknown;

    /// <summary>The source address of the reply.</summary>
    public IPAddress IpAddress { get; }

    public byte[] Buffer { get; }

    public uint RoundTripTime { get; }

    /// <summary>Resolves the <code>Win32Exception</code> from native code</summary>
    public Win32Exception Exception
    {
        get
        {
            if (Status != IPStatus.Success)
                return _exception ?? (_exception = new Win32Exception((int)NativeCode, Status.ToString()));
            return null;
        }
    }

    public override string ToString()
    {
        if (Status == IPStatus.Success)
            return Status + " from " + IpAddress + " in " + RoundTripTime + " ms with " + Buffer.Length + " bytes";
        if (Status != IPStatus.Unknown)
            return Status + " from " + IpAddress;
        return Exception.Message + " from " + IpAddress;
    }
}