using StarMeter.Models;
using System;

namespace StarMeter.View.Helpers
{
    public class LogicHelper
    {
        /// <summary>
        /// Check if a packet is received after a certain time
        /// </summary>
        /// <param name="packet">The Packet to check</param>
        /// <param name="dateToCheck">The DateTime to check against</param>
        /// <returns>Whether or not the packet was received after the time</returns>
        public static bool IsAfterTime(Packet packet, DateTime dateToCheck)
        {
            return packet.DateReceived >= dateToCheck;
        }

        /// <summary>
        /// Check if a packet is received before a certain time
        /// </summary>
        /// <param name="packet">The Packet to check</param>
        /// <param name="dateToCheck">The DateTime to check against</param>
        /// <returns>Whether or not the packet was received before the time</returns>
        public static bool IsBeforeTime(Packet packet, DateTime dateToCheck)
        {
            return packet.DateReceived <= dateToCheck;
        }

        /// <summary>
        /// Check if a packet is received between two times
        /// </summary>
        /// <param name="packet">The Packet to check</param>
        /// <param name="startTime">The starting DateTime to check against</param>
        /// <param name="endTime">The ending DateTime to check against</param>
        /// <returns>Whether or not the packet was received between the two times</returns>
        public static bool IsBetweenTimes(Packet packet, DateTime startTime, DateTime endTime)
        {
            return (packet.DateReceived <= endTime) && (packet.DateReceived >= startTime);
        }

        /// <summary>
        /// Check if a packet matches a decimal protocol search string
        /// </summary>
        /// <param name="packet">The packet in question</param>
        /// <param name="protocolToSearch">The search text</param>
        /// <returns></returns>
        public static bool DecimalProtocolSearch(Packet packet, string protocolToSearch)
        {
            return packet.ProtocolId.ToString().Equals(protocolToSearch);
        }


        /// <summary>
        /// Check if a packet matches a hex protocol search string
        /// </summary>
        /// <param name="packet">The packet in question</param>
        /// <param name="protocolToSearch">The search text</param>
        /// <returns></returns>
        public static bool HexProtocolSearch(Packet packet, string protocolToSearch)
        {
            var hexPacketProtocol = packet.ProtocolId.ToString();
            return hexPacketProtocol.Equals(protocolToSearch);
        }

        /// <summary>
        /// Check if a packets address matches the decimal address entered
        /// </summary>
        /// <param name="packet">The packet in question</param>
        /// <param name="addressToSearch">The address to search for</param>
        /// <returns></returns>
        public static bool DecimalAddressSearch(Packet packet, string addressToSearch)
        {
            var address = packet.Address;
            var finalAddressString = PacketLabelCreator.GetAddressLabel(address);
            return finalAddressString.Equals(addressToSearch);
        }

        /// <summary>
        /// Check if a packets address matches the hex address entered
        /// </summary>
        /// <param name="packet">The packet in question</param>
        /// <param name="addressToSearch">The address to search for</param>
        /// <returns></returns>
        public static bool HexAddressSearch(Packet packet, string addressToSearch)
        {
            var hexPacketAddress = BitConverter.ToString(packet.Address).ToLower();
            return hexPacketAddress.Equals(addressToSearch.ToLower());
        }
    }
}
