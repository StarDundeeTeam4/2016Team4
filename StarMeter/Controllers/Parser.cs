using System;
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
    }
}