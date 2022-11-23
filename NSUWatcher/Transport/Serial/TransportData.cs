using TransportDataContracts;

namespace NSUWatcher.Transport.Serial
{
    public class TransportData : ITransportData
    {
        public string Destination { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
    }
}
