using Newtonsoft.Json;
using NSU.Shared;

namespace NSUWatcher.Transport.TestTransport.Commands
{
    internal class TSensorTemplate
    {
        [JsonProperty(JKeys.Generic.Target)]
        public string Target => "tsensor";

        [JsonProperty(JKeys.Generic.Action)]
        public string Action => "info";

        [JsonProperty(JKeys.TempSensor.SensorID)]
        public string Address { get; }

        [JsonProperty(JKeys.Generic.Value)]
        public double Temperature { get; set; }

        [JsonProperty(JKeys.TempSensor.ReadErrors)]
        public int ReadErrors { get; set; }

        public TSensorTemplate(string address, double temperature)
        {
            Address = address;
            Temperature = temperature;
        }
}
}
