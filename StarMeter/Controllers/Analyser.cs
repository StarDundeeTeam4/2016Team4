using System;
using System.Collections.Generic;
using System.Linq;
using StarMeter.Models;

namespace StarMeter.Controllers
{

    public interface IAnalyser
    {
        int CalculateTotalNoOfDataChars(Dictionary<Guid, Packet> packetDictionary);
        int CalculateTotalNoOfPackets(Dictionary<Guid, Packet> packetDictionary);
        int CalculateTotalNoOfErrorPackets(Dictionary<Guid, Packet> packetDictionary);
        int CalculateDataRate(Dictionary<Guid, Packet> packetDictionary);
        int CalculatePacketRate(Dictionary<Guid, Packet> packetDictionary);
        double CalculateErrorRate(Dictionary<Guid, Packet> packetDictionary);
    }

    public class Analyser : IAnalyser
    {

        public int CalculateTotalNoOfDataChars(Dictionary<Guid, Packet> packetDictionary)
        {
            //To Do
            return 0;
        }

        public int CalculateTotalNoOfPackets(Dictionary<Guid, Packet> packetDictionary)
        {
            var totalNoOfPackets = packetDictionary.Count;
            return totalNoOfPackets;
        }

        public int CalculateTotalNoOfErrorPackets(Dictionary<Guid, Packet> packetDictionary)
        {
            return packetDictionary.Values.Count(packet => packet.IsError);
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

        public double CalculateErrorRate(Dictionary<Guid, Packet> packetDictionary)
        {
            var noOfErrorPackets = CalculateTotalNoOfErrorPackets(packetDictionary);
            var noOfPackets = CalculateTotalNoOfPackets(packetDictionary);

            var errorRate = noOfErrorPackets / (double)noOfPackets;
            return errorRate;
        }
    }
}
