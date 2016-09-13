using System;

namespace StarMeter.Models
{
    public class Packet
    {
        public string type;
        public byte[] cargo;
        public byte[] address;
        public Guid prevPacket;
        public Guid nextPacket;
        public DateTime timestamp;
        public string error;
        public int portNumber;
    }
}