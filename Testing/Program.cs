using QuickScan.Lib;

namespace Testing;
public static class Program
{
    public static void Main(string[] args)
    {
        var a = Snmp.GetSnmp("192.168.1.1");
    }
}