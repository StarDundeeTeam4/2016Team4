using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarMeter.Models
{
    public class Recording
    {
        public DateTime startStamp, endStamp;
        public int portNumber;
        public List<Packet> packetList;
    }
}
