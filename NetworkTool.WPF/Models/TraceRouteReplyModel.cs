namespace NetworkTool.WPF.Models
{
    public class TraceRouteReplyModel
    {
        public int Index { get; set; }
        public string? IPAddress { get; set; }
        public string? HostName { get; set; }
        public string? RoundTripTime { get; set; }
        public string? Status { get; set; }
    }
}
