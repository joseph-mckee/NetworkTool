using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkTool.WPF.Models
{
    public class HostModel
    {
        public string? HostName { get; set; }
        public string? IPAddress { get; set; }
        public long RoundTripTime { get; set; }
        public string? MACAddress { get; set; }
    }
}
