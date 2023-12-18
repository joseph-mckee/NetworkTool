using System.Net;

namespace NetworkTool.Lib.Text;

public class Parser
{
    public static void Parse(string input, out IPAddress startRange, out IPAddress endRange)
    {
        var inputSplit = input.Split('-');
        startRange = IPAddress.Parse(inputSplit[0].Trim());
        endRange = IPAddress.Parse(inputSplit[1].Trim());
    }
}