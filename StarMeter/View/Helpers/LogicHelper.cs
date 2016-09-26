using StarMeter.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarMeter.View.Helpers
{
    class LogicHelper
    {

        /// <summary>
        /// Check if a packet is received after a certain time
        /// </summary>
        /// <param name="p">The Packet to check</param>
        /// <param name="dt">The DateTime to check against</param>
        /// <returns>Whether or not the packet was received after the time</returns>
        public static bool IsAfterTime(Packet p, DateTime dt)
        {
            return (p.DateReceived >= dt);
        }

        /// <summary>
        /// Check if a packet is received before a certain time
        /// </summary>
        /// <param name="p">The Packet to check</param>
        /// <param name="dt">The DateTime to check against</param>
        /// <returns>Whether or not the packet was received before the time</returns>
        public static bool IsBeforeTime(Packet p, DateTime dt)
        {
            return (p.DateReceived <= dt);
        }

        /// <summary>
        /// Check if a packet is received between two times
        /// </summary>
        /// <param name="p">The Packet to check</param>
        /// <param name="start">The starting DateTime to check against</param>
        /// <param name="end">The ending DateTime to check against</param>
        /// <returns>Whether or not the packet was received between the two times</returns>
        public static bool IsBetweenTimes(Packet p, DateTime start, DateTime end)
        {
            return ((p.DateReceived <= end) && (p.DateReceived >= start));
        }

        /// <summary>
        /// Check if a packet matches a protocol search string
        /// </summary>
        /// <param name="p">The packet in question</param>
        /// <param name="search">The search text</param>
        /// <returns></returns>
        public static bool MatchesProtocolSearch(Packet p, string search)
        {
            if (p.ProtocolId.ToString().Equals(search))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Check if a packets address matches the address entered
        /// </summary>
        /// <param name="p">THe packet in question</param>
        /// <param name="search">The address to search for</param>
        /// <returns></returns>
        public static bool MatchesAddressSearch(Packet p, string search)
        {
            byte[] address = p.Address;
            string finalAddressString = "";

            if (address.Length > 1)
            {
                finalAddressString += "Physical Path: ";
                for (var i = 0; i < address.Length - 1; i++)
                    finalAddressString += Convert.ToInt32(address[i]) + "  ";
            }
            else
                finalAddressString = Convert.ToInt32(address[0]).ToString();


            if (finalAddressString.Equals(search))
            {
                return true;
            }

            return false;
        }

        public static bool HexAddressSearch(Packet packet, string addressToSearch)
        {
            var hexPacketAddress = BitConverter.ToString(packet.Address);
            return hexPacketAddress.Equals(addressToSearch);
        }

    }
}
