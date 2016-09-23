﻿using System;
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
            var sortedPackets = from pair in packetDictionary orderby pair.Value.DateRecieved ascending select pair;

            var timeTaken = sortedPackets.Last().Value.DateRecieved - sortedPackets.First().Value.DateRecieved;
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
            var sortedPackets = from pair in packetDictionary orderby pair.Value.DateRecieved ascending select pair;

            var timeTaken = sortedPackets.Last().Value.DateRecieved - sortedPackets.First().Value.DateRecieved;
            var timeTakenInSeconds = TimeSpan.Parse(timeTaken.ToString()).TotalSeconds;

            var totalPackets = CalculateTotalNoOfPackets(packetDictionary);

            var packetsPerSecond = totalPackets / timeTakenInSeconds;
            return packetsPerSecond;
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
        /// <param name="packets"></param>
        /// <returns></returns>
        public List<KeyValuePair<string, int>>[] GetDataForLineChart(Packet[] packets) 
        {
            var returnedData = new List<KeyValuePair<string, int>>();
            var errorData = new List<KeyValuePair<string, int>>();

            if (packets.Length > 0)
            {
                TimeSpan tStart = packets[0].DateRecieved.TimeOfDay;
                TimeSpan tEnd = packets[packets.Length - 1].DateRecieved.TimeOfDay;

                TimeSpan tDiff = tEnd - tStart;

                const int numPoints = 10;

                double interval = (tDiff.TotalMilliseconds / numPoints);

                for (int i = 0; i < numPoints; i++)
                {
                    int count = 0;
                    int errorCount = 0;

                    TimeSpan lowerBound = tStart.Add(new TimeSpan(0, 0, 0, 0, (int)(interval * (i))));
                    TimeSpan upperBound = tStart.Add(new TimeSpan(0, 0, 0, 0, (int)(interval * (i + 1))));

                    foreach (var packet in packets)
                    {
                        if ((packet.DateRecieved.TimeOfDay >= lowerBound) && (packet.DateRecieved.TimeOfDay <= upperBound))
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

            var toReturn = new List<KeyValuePair<string, int>>[2];
            toReturn[0] = returnedData;
            toReturn[1] = errorData;
            return toReturn;
        }
    }
}
