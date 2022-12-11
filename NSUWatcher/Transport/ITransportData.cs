using System;
using System.Collections.Generic;
using System.Text;

namespace TransportDataContracts
{
    public interface ITransportData
    {
        string Destination { get; set; }
        string Action { get; set; }
        string Content { get; set; }
    }
}
