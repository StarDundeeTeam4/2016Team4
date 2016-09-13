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
        public Guid packetId { get; set; }
        public String typeOfPacket { get; set; }
        public Byte[] cargo { get; set; }
        public Byte[] address { get; set; }
        public Guid previousPacket { get; set; }
        public DateTime dateRecieved { get; set; }
        public Boolean isError { get; set; }
        public String errorType { get; set; }
        public int portNumber { get; set; }

    }
}
