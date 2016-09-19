using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarMeter.Models
{
    public class RmapPacket : Packet
    {
        public byte[] SourcePathAddress;
        public int    DestinationKey;
        public ushort HeaderCrc;
        public bool[] AdditionalInfo;
        public string PacketType;
    }
}
