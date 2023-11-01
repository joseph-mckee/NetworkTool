using System.Runtime.InteropServices;

namespace NetworkTool.Lib;

/// <summary>Interoperability Helper
///     <see cref="http://msdn.microsoft.com/en-us/library/windows/desktop/bb309069(v=vs.85).aspx" />
/// </summary>
public static class Interop
{
    private static IntPtr? icmpHandle;
    private static int? _replyStructLength;

    /// <summary>Returns the application legal icmp handle. Should be close by IcmpCloseHandle
    ///     <see cref="http://msdn.microsoft.com/en-us/library/windows/desktop/aa366045(v=vs.85).aspx" />
    /// </summary>
    public static IntPtr IcmpHandle
    {
        get
        {
            if (icmpHandle == null)
            {
                icmpHandle = IcmpCreateFile();
                //TODO Close Icmp Handle appropiate
            }

            return icmpHandle.GetValueOrDefault();
        }
    }
    /// <summary>Returns the the marshaled size of the reply struct.</summary>
    public static int ReplyMarshalLength
    {
        get
        {
            if (_replyStructLength == null)
            {
                _replyStructLength = Marshal.SizeOf(typeof(Reply));
            }
            return _replyStructLength.GetValueOrDefault();
        }
    }


    [DllImport("Iphlpapi.dll", SetLastError = true)]
    private static extern IntPtr IcmpCreateFile();
    [DllImport("Iphlpapi.dll", SetLastError = true)]
    private static extern bool IcmpCloseHandle(IntPtr handle);
    [DllImport("Iphlpapi.dll", SetLastError = true)]
    public static extern uint IcmpSendEcho2Ex(IntPtr icmpHandle, IntPtr Event, IntPtr apcroutine, IntPtr apccontext, UInt32 sourceAddress, UInt32 destinationAddress, byte[] requestData, short requestSize, ref Option requestOptions, IntPtr replyBuffer, int replySize, int timeout);
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
        public readonly UInt32 Address;
        public readonly int Status;
        public readonly int RoundTripTime;
        public readonly short DataSize;
        public readonly short Reserved;
        public readonly IntPtr DataPtr;
        public readonly Option Options;
    }
}