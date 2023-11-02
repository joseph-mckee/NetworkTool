using QuickScan.Lib;
using System.Diagnostics;
using System.Net;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;

namespace NetworkTool.Lib;

public static class ARP
{
    [DllImport("iphlpapi.dll", ExactSpelling = true)]
    private static extern int SendARP(int destIp, int srcIp, [Out] byte[] pMacAddr, ref uint phyAddrLen);

    public static async Task<string> SendARPAsync(string ipAddress)
    {
        return await Task.Run(() =>
        {
            try
            {
                IPAddress addr = IPAddress.Parse(ipAddress);

                var macAddr = new byte[6];
                var macAddrLen = (uint)macAddr.Length;

                if (SendARP(BitConverter.ToInt32(addr.GetAddressBytes(), 0), BitConverter.ToInt32(NetworkInfoManager.GetCurrentNetworkInfo().Address.GetAddressBytes(), 0), macAddr, ref macAddrLen) != 0)
                {
                    return "Unknown";
                }

                var str = new string[(int)macAddrLen];
                for (var i = 0; i < macAddrLen; i++)
                {
                    str[i] = macAddr[i].ToString("x2");
                }

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
        NetworkInterface[] interfaces = NetworkInterface.GetAllNetworkInterfaces();
        foreach (var netInterface in interfaces)
        {
            IPInterfaceProperties ipProps = netInterface.GetIPProperties();
            if (ipProps.GetIPv4Properties() != null)
            {
                uint interfaceIndex = (uint)ipProps.GetIPv4Properties().Index;
                int result = FlushIpNetTable(interfaceIndex);
                Debug.WriteLine($"Flushed ARP for interface index {interfaceIndex}. Result: {result}");
            }
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    struct MIB_IPNETROW
    {
        [MarshalAs(UnmanagedType.U4)]
        public int dwIndex;
        [MarshalAs(UnmanagedType.U4)]
        public int dwPhysAddrLen;
        [MarshalAs(UnmanagedType.U1)]
        public byte mac0;
        [MarshalAs(UnmanagedType.U1)]
        public byte mac1;
        [MarshalAs(UnmanagedType.U1)]
        public byte mac2;
        [MarshalAs(UnmanagedType.U1)]
        public byte mac3;
        [MarshalAs(UnmanagedType.U1)]
        public byte mac4;
        [MarshalAs(UnmanagedType.U1)]
        public byte mac5;
        [MarshalAs(UnmanagedType.U1)]
        public byte mac6;
        [MarshalAs(UnmanagedType.U1)]
        public byte mac7;
        [MarshalAs(UnmanagedType.U4)]
        public int dwAddr;
        [MarshalAs(UnmanagedType.U4)]
        public int dwType;
    }

    [DllImport("IpHlpApi.dll")]
    private static extern long GetIpNetTable(IntPtr pIpNetTable, ref int pdwSize, bool bOrder);

    public static List<ArpEntry> GetARPCache()
    {
        IntPtr buff;
        int requiredLen = 0;
        List<ArpEntry> list = new();
        string ret = "--------------- ARP Table --------------\n";
        long result = GetIpNetTable(IntPtr.Zero, ref requiredLen, true);

        try
        {
            buff = Marshal.AllocCoTaskMem(requiredLen);
            result = GetIpNetTable(buff, ref requiredLen, true);

            int entries = Marshal.ReadInt32(buff);
            IntPtr entryBuffer = new IntPtr(buff.ToInt64() + Marshal.SizeOf(typeof(int)));

            MIB_IPNETROW[] arpTable = new MIB_IPNETROW[entries];

            for (int i = 0; i < entries; i++)
            {
                int currentIndex = i * Marshal.SizeOf(typeof(MIB_IPNETROW));
                IntPtr newStruct = new IntPtr(entryBuffer.ToInt64() + currentIndex);
                arpTable[i] = (MIB_IPNETROW)Marshal.PtrToStructure(newStruct, typeof(MIB_IPNETROW));
            }

            for (int i = 0; i < entries; i++)
            {
                MIB_IPNETROW entry = arpTable[i];
                IPAddress addr = new IPAddress(BitConverter.GetBytes(entry.dwAddr));

                if (entry.mac0 == 0 && entry.mac1 == 0 && entry.mac2 == 0 && entry.mac3 == 0 && entry.mac4 == 0 && entry.mac5 == 0)
                    continue;
                else
                    list.Add(new ArpEntry
                    {
                        IPAddress = addr.ToString(),
                        MACAddress = String.Format("{0}:{1}:{2}:{3}:{4}:{5}", entry.mac0.ToString("X2"), entry.mac1.ToString("X2"), entry.mac2.ToString("X2"), entry.mac3.ToString("X2"), entry.mac4.ToString("X2"), entry.mac5.ToString("X2")),
                        Index = entry.dwIndex
                    });

                ret += String.Format("{0}: {1}:{2}:{3}:{4}:{5}:{6} | {7}\n", addr.ToString(), entry.mac0.ToString("X2"), entry.mac1.ToString("X2"), entry.mac2.ToString("X2"), entry.mac3.ToString("X2"), entry.mac4.ToString("X2"), entry.mac5.ToString("X2"), entry.dwIndex);
            }

        }
        catch (Exception e)
        {

        }
        return list;
    }
}