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
        /// <summary>
        /// Method to calculate the total number of bytes being transmitted
        /// </summary>
        /// <param name="packetDictionary">The complete packet dictionary with all of the packets in the transmission</param>
        /// <returns>An integer of the total number of data characters in bytes</returns>
        public int CalculateTotalNoOfDataChars(Dictionary<Guid, Packet> packetDictionary)
        {
            var totalNoOfDataChars = 0;
            foreach (var packet in packetDictionary.Values)
            {
                var packetAddressLength = 0;
                if (packet.Address != null)
                {
                    packetAddressLength = packet.Address.Length;
                }
                var packetCargoLength = packet.Cargo.Length;
                var packetDataChars = packetAddressLength + packetCargoLength;
                totalNoOfDataChars += packetDataChars;
            }
            return totalNoOfDataChars;
        }

        /// <summary>
        /// Method to calculate the total number of packets being transmitted
        /// </summary>
        /// <param name="packetDictionary">The complete packet dictionary with all of the packets in the transmission</param>
        /// <returns>An integer of the total number of packets</returns>
        public int CalculateTotalNoOfPackets(Dictionary<Guid, Packet> packetDictionary)
        {
            var totalNoOfPackets = packetDictionary.Count;
            return totalNoOfPackets;
        }

        /// <summary>
        /// Method to calculate the total number of error packets being transmitted
        /// </summary>
        /// <param name="packetDictionary">The complete packet dictionary with all of the packets in the transmission</param>
        /// <returns>An integer of the total number of packets that contain errors</returns>
        public int CalculateTotalNoOfErrorPackets(Dictionary<Guid, Packet> packetDictionary)
        {
            return packetDictionary.Values.Count(packet => packet.IsError);
        }

        /// <summary>
        /// A method to calculate the data rate of the transmission
        /// </summary>
        /// <param name="packetDictionary">The complete packet dictionary with all of the packets in the transmission</param>
        /// <returns>A double of the total data rate in bytes per second</returns>
        public double CalculateDataRateBytePerSecond(Dictionary<Guid, Packet> packetDictionary)
        {
            var sortedPackets = from pair in packetDictionary orderby pair.Value.DateReceived ascending select pair;

            var timeTaken = sortedPackets.Last().Value.DateReceived - sortedPackets.First().Value.DateReceived;
            var timeTakenInSeconds = TimeSpan.Parse(timeTaken.ToString()).TotalSeconds;

            var totalData = CalculateTotalNoOfDataChars(packetDictionary);

            var dataPerSecond = totalData / timeTakenInSeconds;
            return dataPerSecond;
        }

        /// <summary>
        /// A method to calculate the packet rate of the transmission
        /// </summary>
        /// <param name="packetDictionary">The complete packet dictionary with all of the packets in the transmission</param>
        /// <returns>A double of the total packet rate in packets per second</returns>
        public double CalculatePacketRatePerSecond(Dictionary<Guid, Packet> packetDictionary)
        {
            try
            {
                var sortedPackets = from pair in packetDictionary orderby pair.Value.DateReceived ascending select pair;

                var timeTaken = sortedPackets.Last().Value.DateReceived - sortedPackets.First().Value.DateReceived;
                var timeTakenInSeconds = TimeSpan.Parse(timeTaken.ToString()).TotalSeconds;

                var totalPackets = CalculateTotalNoOfPackets(packetDictionary);

                var packetsPerSecond = totalPackets / timeTakenInSeconds;
                return packetsPerSecond;
            }
            catch (Exception) { return 0; }
        }

        /// <summary>
        /// A method to calculate the error rate of the transmission
        /// </summary>
        /// <param name="packetDictionary">The complete packet dictionary with all of the packets in the transmission</param>
        /// <returns>A double of the total error rate measured against the total number of packets</returns>
        public double CalculateErrorRate(Dictionary<Guid, Packet> packetDictionary)
        {
            var noOfErrorPackets = CalculateTotalNoOfErrorPackets(packetDictionary);
            var noOfPackets = CalculateTotalNoOfPackets(packetDictionary);

            var errorRate = noOfErrorPackets / (double)noOfPackets;
            return errorRate;
        }

        /// <summary>
        /// A method to calculate the error rate of the transmission
        /// </summary>
        /// <param name="packets">The complete packet array with all of the packets in the transmission</param>
        /// <returns>A double of the total error rate measured against the total number of packets</returns>
        public double CalculateErrorRateFromArray(Packet[] packets)
        {
            var noOfErrorPackets = packets.Count(p => p.IsError);
            var noOfPackets = packets.Length;

            var errorRate = noOfErrorPackets / (double)noOfPackets;
            return errorRate;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="packets">The complete packet array with all of the packets in the transmission</param>
        /// <returns></returns>
        public List<KeyValuePair<string, int>>[] GetDataForLineChart(Packet[] packets)
        {
            var graphData = new List<KeyValuePair<string, int>>();
            var errorData = new List<KeyValuePair<string, int>>();
            var dataRate = new List<KeyValuePair<string, int>>();

            if (packets.Length > 0)
            {
                var startTime = packets[0].DateReceived.TimeOfDay;
                var endTime = packets[packets.Length - 1].DateReceived.TimeOfDay;
                var timeDifference = endTime - startTime;

                const int numPoints = 10;
                var graphInterval = timeDifference.TotalMilliseconds / numPoints;

                for (var i = 0; i < numPoints; i++)
                {
                    int count = 0;
                    int errorCount = 0;
                    int charCount = 0;

                    var lowerBound = startTime.Add(new TimeSpan(0, 0, 0, 0, (int)(graphInterval * i)));
                    var upperBound = startTime.Add(new TimeSpan(0, 0, 0, 0, (int)(graphInterval * (i + 1))));

                    foreach (var packet in packets)
                    {
                        if ((packet.DateReceived.TimeOfDay >= lowerBound) && (packet.DateReceived.TimeOfDay <= upperBound))
                        {
                            count++;

                            var packetAddressLength = 0;
                            if (packet.Address != null)
                            {
                                packetAddressLength = packet.Address.Length;
                            }
                            var packetCargoLength = packet.Cargo.Length;
                            var packetDataChars = packetAddressLength + packetCargoLength;
                            charCount += packetDataChars;

                            if (packet.IsError)
                            {
                                errorCount++;
                            }
                        }
                    }
                    var kvp = new KeyValuePair<string, int>(lowerBound.ToString(), count);
                    var kvpError = new KeyValuePair<string, int>(lowerBound.ToString(), errorCount);
                    var kvpData = new KeyValuePair<string, int>(lowerBound.ToString(), charCount);
                    graphData.Add(kvp);
                    errorData.Add(kvpError);
                    dataRate.Add(kvpData);
                }
            }
            var toReturn = new List<KeyValuePair<string, int>>[3];

            toReturn[0] = graphData;
            toReturn[1] = errorData;
            toReturn[2] = dataRate;
            return toReturn;
        }
    }
}
