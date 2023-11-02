﻿using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.InteropServices;

namespace NetworkTool.Lib;

public static class PingEx
{
    public static PingReplyEx Send(IPAddress srcAddress, IPAddress destAddress, int timeout = 5000, byte[] buffer = null, PingOptions po = null)
    {
        if (destAddress == null || destAddress.AddressFamily != AddressFamily.InterNetwork || destAddress.Equals(IPAddress.Any))
            throw new ArgumentException();

        //Defining pinvoke args
        var source = srcAddress == null ? 0 : BitConverter.ToUInt32(srcAddress.GetAddressBytes(), 0);
        var destination = BitConverter.ToUInt32(destAddress.GetAddressBytes(), 0);
        var sendbuffer = buffer ?? new byte[] { };
        var options = new Interop.Option
        {
            Ttl = (po == null ? (byte)255 : (byte)po.Ttl),
            Flags = (po == null ? (byte)0 : po.DontFragment ? (byte)0x02 : (byte)0) //0x02
        };
        var fullReplyBufferSize = Interop.ReplyMarshalLength + sendbuffer.Length; //Size of Reply struct and the transmitted buffer length.



        var allocSpace = Marshal.AllocHGlobal(fullReplyBufferSize); // unmanaged allocation of reply size. TODO Maybe should be allocated on stack
        try
        {
            DateTime start = DateTime.Now;
            var nativeCode = Interop.IcmpSendEcho2Ex(
                Interop.IcmpHandle, //_In_      HANDLE IcmpHandle,
                default(IntPtr), //_In_opt_  HANDLE Event,
                default(IntPtr), //_In_opt_  PIO_APC_ROUTINE ApcRoutine,
                default(IntPtr), //_In_opt_  PVOID ApcContext
                source, //_In_      IPAddr SourceAddress,
                destination, //_In_      IPAddr DestinationAddress,
                sendbuffer, //_In_      LPVOID RequestData,
                (short)sendbuffer.Length, //_In_      WORD RequestSize,
                ref options, //_In_opt_  PIP_OPTION_INFORMATION RequestOptions,
                allocSpace, //_Out_     LPVOID ReplyBuffer,
                fullReplyBufferSize, //_In_      DWORD ReplySize,
                timeout //_In_      DWORD Timeout
                );
            TimeSpan duration = DateTime.Now - start;
            var reply = (Interop.Reply)Marshal.PtrToStructure(allocSpace, typeof(Interop.Reply))!; // Parse the beginning of reply memory to reply struct

            byte[] replyBuffer = null;
            if (sendbuffer.Length != 0)
            {
                replyBuffer = new byte[sendbuffer.Length];
                Marshal.Copy(allocSpace + Interop.ReplyMarshalLength, replyBuffer, 0, sendbuffer.Length); //copy the rest of the reply memory to managed byte[]
            }

            if (nativeCode == 0) //Means that native method is faulted.
                return new PingReplyEx(nativeCode, reply.Status, new IPAddress(reply.Address), duration);
            else
                return new PingReplyEx(nativeCode, reply.Status, new IPAddress(reply.Address), reply.RoundTripTime, replyBuffer);
        }
        finally
        {
            Marshal.FreeHGlobal(allocSpace); //free allocated space
        }
    }
}
