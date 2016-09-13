using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StarMeter.Models;

namespace StarMeter.Controllers
{
    public class Parser
    {
        public void ParseFile()
        {
            //set up string buffer/etc.
            foreach (Packet packet in ParsePacket())
            {
                //add packet to data structure
            }
        }

        private static System.Collections.Generic.IEnumerable<Packet> ParsePacket()
        {
            yield return new Packet();
        }

        public DateTime ParseDateTime(string stringDateTime)
        {
            return new DateTime();
        }

        public string GetPacketType(string inputLine)
        {
            var packetType = char.IsDigit(inputLine[0]) 
                ? "port number" 
                : "packet";
            return packetType;
        }

    }
}