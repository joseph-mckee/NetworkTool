using System.ComponentModel;
using System.Net;
using System.Net.NetworkInformation;

namespace NetworkTool.Lib;

[Serializable]
public class PingReplyEx
{
    private readonly byte[] _buffer = null;
    private readonly IPAddress _ipAddress = null;
    private readonly uint _nativeCode = 0;
    private readonly uint _roundTripTime = 0;
    private readonly IPStatus _status = IPStatus.Unknown;
    private Win32Exception _exception;


    internal PingReplyEx(uint nativeCode, int replystatus, IPAddress ipAddress, TimeSpan duration)
    {
        _nativeCode = nativeCode;
        _ipAddress = ipAddress;
        if (Enum.IsDefined(typeof(IPStatus), replystatus))
            _status = (IPStatus)replystatus;
    }
    internal PingReplyEx(uint nativeCode, int replystatus, IPAddress ipAddress, int roundTripTime, byte[] buffer)
    {
        _nativeCode = nativeCode;
        _ipAddress = ipAddress;
        _roundTripTime = (uint)roundTripTime;
        _buffer = buffer;
        if (Enum.IsDefined(typeof(IPStatus), replystatus))
            _status = (IPStatus)replystatus;
    }


    /// <summary>Native result from <code>IcmpSendEcho2Ex</code>.</summary>
    public uint NativeCode
    {
        get { return _nativeCode; }
    }
    public IPStatus Status
    {
        get { return _status; }
    }
    /// <summary>The source address of the reply.</summary>
    public IPAddress IpAddress
    {
        get { return _ipAddress; }
    }
    public byte[] Buffer
    {
        get { return _buffer; }
    }
    public uint RoundTripTime
    {
        get { return _roundTripTime; }
    }
    /// <summary>Resolves the <code>Win32Exception</code> from native code</summary>
    public Win32Exception Exception
    {
        get
        {
            if (Status != IPStatus.Success)
                return _exception ?? (_exception = new Win32Exception((int)NativeCode, Status.ToString()));
            else
                return null;
        }
    }

    public override string ToString()
    {
        if (Status == IPStatus.Success)
            return Status + " from " + IpAddress + " in " + RoundTripTime + " ms with " + Buffer.Length + " bytes";
        else if (Status != IPStatus.Unknown)
            return Status + " from " + IpAddress;
        else
            return Exception.Message + " from " + IpAddress;
    }
}