﻿using System.Runtime.InteropServices;

namespace NetworkTool.Lib.Ping;

/// <summary>
///     Interoperability Helper
///     <see cref="http://msdn.microsoft.com/en-us/library/windows/desktop/bb309069(v=vs.85).aspx" />
/// </summary>
public static class Interop
{
    private static IntPtr? _icmpHandle;
    private static int? _replyStructLength;

    /// <summary>
    ///     Returns the application legal icmp handle. Should be close by IcmpCloseHandle
    ///     <see cref="http://msdn.microsoft.com/en-us/library/windows/desktop/aa366045(v=vs.85).aspx" />
    /// </summary>
    public static IntPtr IcmpHandle
    {
        get
        {
            _icmpHandle ??= IcmpCreateFile();
            //TODO Close Icmp Handle appropriate
            return _icmpHandle.GetValueOrDefault();
        }
    }

    /// <summary>Returns the the marshaled size of the reply struct.</summary>
    public static int ReplyMarshalLength
    {
        get
        {
            _replyStructLength ??= Marshal.SizeOf(typeof(Reply));
            return _replyStructLength.GetValueOrDefault();
        }
    }


    [DllImport("Iphlpapi.dll", SetLastError = true)]
    private static extern IntPtr IcmpCreateFile();

    [DllImport("Iphlpapi.dll", SetLastError = true)]
    private static extern bool IcmpCloseHandle(IntPtr handle);

    [DllImport("Iphlpapi.dll", SetLastError = true)]
    public static extern uint IcmpSendEcho2Ex(IntPtr icmpHandle, IntPtr @event, IntPtr apcroutine, IntPtr apccontext,
        uint sourceAddress, uint destinationAddress, byte[]? requestData, short requestSize, ref Option requestOptions,
        IntPtr replyBuffer, int replySize, int timeout);

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct Option
    {
        public byte Ttl;
        public readonly byte Tos;
        public byte Flags;
        public readonly byte OptionsSize;
        public readonly IntPtr OptionsData;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct Reply
    {
        public readonly uint Address;
        public readonly int Status;
        public readonly int RoundTripTime;
        public readonly short DataSize;
        public readonly short Reserved;
        public readonly IntPtr DataPtr;
        public readonly Option Options;
    }
}