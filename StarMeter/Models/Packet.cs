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
        public Guid     packetId       { get; }
        public string   typeOfPacket   { get; }
        public byte[]   cargo          { get; }
        public byte[]   address        { get; }
        public DateTime dateRecieved   { get; }
        public bool     isError        { get; }
        public string   errorType      { get; }
        public int      portNumber     { get; }
        public Guid     previousPacket { get; set; }
        public Guid     nextPacket     { get; set; }
    }
}
