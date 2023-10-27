using System.Net.NetworkInformation;

namespace NetworkTool.WPF.Models
{
    public class TraceRouteReplyModel
    {
        public int Index { get; set; }
        public string? IPAddress { get; set; }
        public int BufferSize { get; set; }
        public long RoundtripTime { get; set; }
        public IPStatus Status { get; set; }
    }
}
