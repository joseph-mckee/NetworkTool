using CommunityToolkit.Mvvm.ComponentModel;
using NetworkTool.Lib;
using System.Net.NetworkInformation;

namespace NetworkTool.WPF.Models
{
    public partial class PingReplyModel
    {
        public int Index { get; set; }
        public string? IPAddress { get; set; }
        public long RoundtripTime { get; set; }
        public IPStatus Status { get; set; }
        public PingReplyModel(PingReplyEx reply, int index)
        {
            Index = index;
            IPAddress = reply.IpAddress?.ToString() ?? string.Empty;
            RoundtripTime = reply.RoundTripTime;
            Status = reply.Status;
        }
    }
}
