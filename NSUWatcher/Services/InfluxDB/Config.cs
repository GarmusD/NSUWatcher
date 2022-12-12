using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSUWatcher.Services.InfluxDB
{
    internal class Config
    {
        public string Url { get; set; }
        public string Token { get; set; }
        public string Bucket { get; set; }
        public string Org { get; set; }
        public Timing Timing { get; set; }
    }

    internal class Timing
    {
        public int TSensor { get; set; }
    }
}
