﻿using System.Diagnostics;
using System.Net;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using QuickScan.Lib;

namespace NetworkTool.Lib.Arp;

public static class Arp
{
    [DllImport("iphlpapi.dll", ExactSpelling = true)]
    private static extern int SendARP(int destIp, int srcIp, [Out] byte[] pMacAddr, ref uint phyAddrLen);

    public static async Task<string> SendArpAsync(string ipAddress)
    {
        return await Task.Run(() =>
        {
            try
            {
                var addr = IPAddress.Parse(ipAddress);
                var macAddr = new byte[6];
                var macAddrLen = (uint)macAddr.Length;
                var result = SendARP(BitConverter.ToInt32(addr.GetAddressBytes(), 0),
                    BitConverter.ToInt32(
                        NetworkInfoManager.GetCurrentNetworkInfo().Address?.GetAddressBytes() ??
                        throw new InvalidOperationException(), 0), macAddr, ref macAddrLen);
                if (result != 0) return "Unknown";
                var str = new string[(int)macAddrLen];
                for (var i = 0; i < macAddrLen; i++) str[i] = macAddr[i].ToString("x2");
                return string.Join(":", str).ToUpper();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                return "Unknown";
            }
        });
    }

    [DllImport("IpHlpApi.dll")]
    private static extern int FlushIpNetTable(uint dwIfIndex);

    public static void FlushArpTable()
    {
        var interfaces = NetworkInterface.GetAllNetworkInterfaces();
        foreach (var netInterface in interfaces)
        {
            var ipProps = netInterface.GetIPProperties();
            var interfaceIndex = (uint)ipProps.GetIPv4Properties().Index;
            var result = FlushIpNetTable(interfaceIndex);
            Debug.WriteLine($"Flushed ARP for interface index {interfaceIndex}. Result: {result}");
        }
    }

    [DllImport("IpHlpApi.dll")]
    private static extern long GetIpNetTable(IntPtr pIpNetTable, ref int pdwSize, bool bOrder);

    public static List<ArpEntry> GetArpCache()
    {
        var requiredLen = 0;
        List<ArpEntry> list = new();
        GetIpNetTable(IntPtr.Zero, ref requiredLen, false);
        try
        {
            var buff = Marshal.AllocCoTaskMem(requiredLen);
            GetIpNetTable(buff, ref requiredLen, true);
            var entries = Marshal.ReadInt32(buff);
            var entryBuffer = new IntPtr(buff.ToInt64() + Marshal.SizeOf(typeof(int)));
            var arpTable = new MibIpnetrow[entries];
            for (var i = 0; i < entries; i++)
            {
                var currentIndex = i * Marshal.SizeOf(typeof(MibIpnetrow));
                var newStruct = new IntPtr(entryBuffer.ToInt64() + currentIndex);
                arpTable[i] = (MibIpnetrow)Marshal.PtrToStructure(newStruct, typeof(MibIpnetrow))!;
            }

            for (var i = 0; i < entries; i++)
            {
                var entry = arpTable[i];
                var addr = new IPAddress(BitConverter.GetBytes(entry.dwAddr));
                if (entry is { mac0: 0, mac1: 0, mac2: 0, mac3: 0, mac4: 0, mac5: 0 })
                    continue;
                list.Add(new ArpEntry
                {
                    IpAddress = addr.ToString(),
                    MacAddress =
                        $"{entry.mac0:X2}:{entry.mac1:X2}:{entry.mac2:X2}:{entry.mac3:X2}:{entry.mac4:X2}:{entry.mac5:X2}",
                    Index = entry.dwIndex
                });
            }
        }
        catch (Exception e)
        {
            Debug.WriteLine(e.Message);
        }

        return list;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct MibIpnetrow
    {
        [MarshalAs(UnmanagedType.U4)] public readonly int dwIndex;
        [MarshalAs(UnmanagedType.U4)] public readonly int dwPhysAddrLen;
        [MarshalAs(UnmanagedType.U1)] public readonly byte mac0;
        [MarshalAs(UnmanagedType.U1)] public readonly byte mac1;
        [MarshalAs(UnmanagedType.U1)] public readonly byte mac2;
        [MarshalAs(UnmanagedType.U1)] public readonly byte mac3;
        [MarshalAs(UnmanagedType.U1)] public readonly byte mac4;
        [MarshalAs(UnmanagedType.U1)] public readonly byte mac5;
        [MarshalAs(UnmanagedType.U1)] public readonly byte mac6;
        [MarshalAs(UnmanagedType.U1)] public readonly byte mac7;
        [MarshalAs(UnmanagedType.U4)] public readonly int dwAddr;
        [MarshalAs(UnmanagedType.U4)] public readonly int dwType;
    }
}