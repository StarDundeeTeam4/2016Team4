using System;
using System.Collections.Generic;
using StarMeter.Models;

namespace StarMeter.Controllers
{

    public interface IAnalyser
    {
        int TotalNoOfDataChars(Dictionary<Guid, Packet> packetDictionary);
        int TotalNoOfPackets(Dictionary<Guid, Packet> packetDictionary);
        int TotalNoOfErrorPackets(Dictionary<Guid, Packet> packetDictionary);
        int CalculateDataRate(Dictionary<Guid, Packet> packetDictionary);
        int CalculatePacketRate(Dictionary<Guid, Packet> packetDictionary);
        int CalculateErrorRate(Dictionary<Guid, Packet> packetDictionary);
    }

    public class Analyser : IAnalyser
    {

        public int TotalNoOfDataChars(Dictionary<Guid, Packet> packetDictionary)
        {
            //To Do
            return 0;
        }

        public int TotalNoOfPackets(Dictionary<Guid, Packet> packetDictionary)
        {
            //To Do
            return 0;
        }

        public int TotalNoOfErrorPackets(Dictionary<Guid, Packet> packetDictionary)
        {
            //To Do
            return 0;
        }

        public int CalculateDataRate(Dictionary<Guid, Packet> packetDictionary)
        {
            //To Do
            return 0;
        }

        public int CalculatePacketRate(Dictionary<Guid, Packet> packetDictionary)
        {
            //To Do
            return 0;
        }

        public int CalculateErrorRate(Dictionary<Guid, Packet> packetDictionary)
        {
            //To Do
            return 0;
        }
    }
}
