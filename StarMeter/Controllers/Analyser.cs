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
        double CalculateDataRateBytePerSecond(Dictionary<Guid, Packet> packetDictionary);
        double CalculatePacketRatePerSecond(Dictionary<Guid, Packet> packetDictionary);
        double CalculateErrorRate(Dictionary<Guid, Packet> packetDictionary);
    }

    public class Analyser : IAnalyser
    {

        public int CalculateTotalNoOfDataChars(Dictionary<Guid, Packet> packetDictionary)
        {
            var totalNoOfDataChars = 0;
            foreach (var packet in packetDictionary.Values)
            {
                if (packet.Address != null)
                {
                    var packetAddressLength = packet.Address.Length;
                    var packetCargoLength = packet.Cargo.Length;
                    var packetDataChars = packetAddressLength + packetCargoLength;
                    totalNoOfDataChars += packetDataChars;
                }
            }
            return totalNoOfDataChars;
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

        public double CalculateDataRateBytePerSecond(Dictionary<Guid, Packet> packetDictionary)
        {
            var sortedPackets = from pair in packetDictionary orderby pair.Value.DateRecieved ascending select pair;

            var timeTaken = sortedPackets.Last().Value.DateRecieved - sortedPackets.First().Value.DateRecieved;
            var timeTakenInSeconds = TimeSpan.Parse(timeTaken.ToString()).TotalSeconds;

            var totalData = CalculateTotalNoOfDataChars(packetDictionary);

            var dataPerSecond = totalData / timeTakenInSeconds;
            return dataPerSecond;
        }

        public double CalculatePacketRatePerSecond(Dictionary<Guid, Packet> packetDictionary)
        {
            var sortedPackets = from pair in packetDictionary orderby pair.Value.DateRecieved ascending select pair;

            var timeTaken = sortedPackets.Last().Value.DateRecieved - sortedPackets.First().Value.DateRecieved;
            var timeTakenInSeconds = TimeSpan.Parse(timeTaken.ToString()).TotalSeconds;

            var totalPackets = CalculateTotalNoOfPackets(packetDictionary);

            var packetsPerSecond = totalPackets / timeTakenInSeconds;
            return packetsPerSecond;
        }

        public double CalculateErrorRate(Dictionary<Guid, Packet> packetDictionary)
        {
            var noOfErrorPackets = CalculateTotalNoOfErrorPackets(packetDictionary);
            var noOfPackets = CalculateTotalNoOfPackets(packetDictionary);

            var errorRate = noOfErrorPackets / (double)noOfPackets;
            return errorRate;
        }
        public double CalculateErrorRateFromArray(Packet[] packets)
        {
            var count = 0;

            foreach (var p in packets) 
            {
                if (p.IsError) { count++; }
            }

            var noOfPackets = packets.Length;

            var errorRate = count / (double)noOfPackets;
            return errorRate;
        }

        public List<KeyValuePair<string, int>>[] GetDataForLineChart(Packet[] packets) 
        {

            var sortedPackets = from pair in packets orderby pair.DateRecieved ascending select pair;

            List<KeyValuePair<string, int>> returnedData = new List<KeyValuePair<string, int>>();
            List<KeyValuePair<string, int>> errorData = new List<KeyValuePair<string, int>>();

            if (packets.Length > 0)
            {
                TimeSpan tStart = packets[0].DateRecieved.TimeOfDay;
                TimeSpan tEnd = packets[packets.Length - 1].DateRecieved.TimeOfDay;

                TimeSpan tDiff = tEnd - tStart;

                const int NUM_POINTS = 10;

                double interval = (tDiff.TotalMilliseconds / NUM_POINTS);

                for (int i = 0; i < NUM_POINTS; i++)
                {
                    int count = 0;
                    int errorCount = 0;

                    TimeSpan lowerBound = tStart.Add(new TimeSpan(0, 0, 0, 0, (int)(interval * (i))));
                    TimeSpan upperBound = tStart.Add(new TimeSpan(0, 0, 0, 0, (int)(interval * (i + 1))));

                    foreach (var packet in packets)
                    {
                        if ((packet.DateRecieved.TimeOfDay > lowerBound) && (packet.DateRecieved.TimeOfDay < upperBound))
                        {
                            count++;

                            if (packet.IsError)
                            {
                                errorCount++;
                            }
                        }
                    }

                    var kvp = new KeyValuePair<string, int>(lowerBound.ToString(), count);
                    var kvpError = new KeyValuePair<string, int>(lowerBound.ToString(), errorCount);
                    returnedData.Add(kvp);
                    errorData.Add(kvpError);

                }

            }

            List<KeyValuePair<string, int>>[] toReturn = new List<KeyValuePair<string, int>>[2];
            toReturn[0] = returnedData;
            toReturn[1] = errorData;
            return toReturn;
        }
    }
}
