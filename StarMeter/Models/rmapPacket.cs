using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarMeter.Models
{
    public class RmapPacket : Packet
    {
        private static readonly Dictionary<int, string> PacketTypeDictionary = new Dictionary<int, string>()
        {
            {0, "Read Reply"},
            {1, "Read Modify Write Reply"},
            {2, "Write Reply"},
            {4, "Read"},
            {5, "Read Modify Write"},
            {6, "Write"}
        };

        public byte[] SourceAddress;
        public int    DestinationKey;
        public ushort HeaderCrc;
        public bool[] AdditionalInfo;
        public string PacketType;
    }
}
