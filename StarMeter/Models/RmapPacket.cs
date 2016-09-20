using System.Collections;

namespace StarMeter.Models
{
    public class RmapPacket : Packet
    {
        public byte      DestinationKey;
        public byte[]   SourcePathAddress;
        public ushort   HeaderCrc;
        public string   PacketType;
        public BitArray CommandByte;
    }
}