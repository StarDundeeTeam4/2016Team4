using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace StarMeter.Models
{
    public class Packet
    {
        private Guid packetId { get; set; }
        private String typeOfPacket { get; set; }
        private Byte[] cargo { get; set; }
        private Byte[] address { get; set; }
        private Guid previousPacket { get; set; }
        private DateTime dateRecieved { get; set; }
        private Boolean isError { get; set; }
        private String errorType { get; set; }
        private int portNumber { get; set; }

    }
}
