using TransportDataContracts;

namespace serialTransport
{
    internal class TransportData : ITransportData
    {
        public string Destination { get; set; }
        public string Action { get; set; }
        public string Content { get; set; }
    }
}
