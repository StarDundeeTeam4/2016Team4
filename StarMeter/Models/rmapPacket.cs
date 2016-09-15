using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarMeter.Models
{
    class rmapPacket : Packet
    {
        public byte[] sourceAddress;
        public int    destinationKey;
        public ushort headerCrc;
        public bool[] additionalInfo;
    }
}
