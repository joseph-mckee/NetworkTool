using System.Net.NetworkInformation;

namespace NetworkTool.WPF.Models
{
    public class PingReplyModel
    {
        public int Index { get; set; }
        public string? IPAddress { get; set; }
        public int BufferSize { get; set; }
        public bool Fragmentable { get; set; }
        public int TTL { get; set; }
        public long RoundtripTime { get; set; }
        public IPStatus Status { get; set; }
        public PingReplyModel(PingReply reply, int index)
        {
            Index = index;
            IPAddress = reply.Address?.ToString() ?? string.Empty;
            BufferSize = reply.Buffer.Length;
            Fragmentable = reply.Options?.DontFragment != true;
            TTL = reply.Options?.Ttl ?? default;
            RoundtripTime = reply.RoundtripTime;
            Status = reply.Status;
        }
    }
}
