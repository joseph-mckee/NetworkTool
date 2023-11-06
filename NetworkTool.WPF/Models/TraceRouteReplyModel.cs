namespace NetworkTool.WPF.Models;

public class TraceRouteReplyModel
{
    public int Index { get; init; }
    public string? IpAddress { get; init; }
    public string? HostName { get; init; }
    public string? RoundTripTime { get; init; }
    public string? Status { get; init; }
}