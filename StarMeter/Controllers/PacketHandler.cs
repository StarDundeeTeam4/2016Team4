﻿using System;
using System.Linq;
using StarMeter.Models;
using System.Globalization;
using System.Collections;

namespace StarMeter.Controllers
{
    public static class PacketHandler
    {
        /// <summary>
        /// Sets the core information of the packet from the data array
        /// </summary>
        /// <param name="packet">The packet for which the information is set</param>
        /// <returns>The updated packet</returns>
        public static Packet SetPacketInformation(Packet packet)
        {
            packet.ErrorType = ErrorType.None;
            packet.Crc = GetCrc(packet); //can't fail unless everything is fucked

            //next four lines must be done in order as if packet ends early, everything before will work and everything after will fail anyway
            packet.DestinationAddress = GetAddressArray(packet);
            packet.ProtocolId = GetProtocolId(packet);
            packet.SequenceNum = GetSequenceNumber(packet);
            packet.Cargo = GetCargoArray(packet);

            return packet;
        }
        
        /// <summary>
        /// Checks whether the packet is a P (packet type) or E (Error type) or other
        /// </summary>
        /// <param name="packetType">The packet type to check</param>
        /// <returns>Whether the packet is a P type</returns>
        public static bool IsPType(string packetType)
        {
            return string.CompareOrdinal(packetType, "P") == 0;
        }

        /// <summary>
        /// Parses the datestring into a DateTime object
        /// </summary>
        /// <param name="stringDateTime">The datestring to parse</param>
        /// <param name="result">The datetime object the result should be written to</param>
        /// <returns>Whether the operation was successful</returns>
        public static bool ParseDateTime(string stringDateTime, out DateTime result)
        {
            return DateTime.TryParseExact(stringDateTime, "dd-MM-yyyy HH:mm:ss.fff", null, DateTimeStyles.None, out result);
        }

        /// <summary>
        /// Calculates the end position of the address in the packet data
        /// </summary>
        /// <param name="packet">The packet's data</param>
        /// <returns>The end index of the address bytes</returns>
        public static int GetLogicalAddressIndex(Packet packet)
        {
            for (var i = 0; i < packet.FullPacket.Length; i++)
            {
                if (packet.FullPacket[i] >= 32) return i;
            }
            return -1;
        }

        /// <summary>
        /// Retrieves the cargo from the full packet data
        /// </summary>
        /// <param name="packet">The packet for which the cargo should be returned</param>
        /// <returns>Packet's cargo</returns>
        public static byte[] GetCargoArray(Packet packet)
        {
            var logicalIndex = GetLogicalAddressIndex(packet);
            try
            {
                var start = logicalIndex + 1; //increment anyway
                if (packet.ProtocolId == 1)
                {
                    var type =
                        RmapPacketHandler.GetRmapType(
                            new BitArray(new[] {packet.FullPacket[GetLogicalAddressIndex(packet) + 2]}));

                    if (type.EndsWith("Reply"))
                    {
                        start += 12 - 1; //skip header, but -1 due to increment above
                    }
                }

                var length = packet.FullPacket.Length - start;
                if (length < 0)
                {
                    start = 0;
                    length = packet.FullPacket.Length; //overflow handling
                }
                var cargo = new byte[length];
                Array.Copy(packet.FullPacket, start, cargo, 0, length);
                return cargo;
            }
            catch (IndexOutOfRangeException)
            {
                return null;
            }
        }

        /// <summary>
        /// Calculates the destination address of the packet
        /// </summary>
        /// <param name="packet">The packet's data</param>
        /// <returns>The byte array of the address</returns>
        public static byte[] GetAddressArray(Packet packet)
        {
            var logicalIndex = GetLogicalAddressIndex(packet);
            var addressArray = new byte[logicalIndex + 1];
            Array.Copy(packet.FullPacket, addressArray, logicalIndex + 1);
            return addressArray;
        }

        /// <summary>
        /// Returns the last byte of the packet which should be the CRC byte
        /// </summary>
        /// <param name="packet">The packet's data</param>
        /// <returns>The CRC byte</returns>
        public static byte GetCrc(Packet packet)
        {
            return packet.FullPacket.Last();
        }

        /// <summary>
        /// Calculates the packet's protocol ID
        /// </summary>
        /// <param name="packet">The packet's data to calculate the protocol ID from</param>
        /// <returns>The protocol ID</returns>
        public static int GetProtocolId(Packet packet)
        {
            try
            {
                var logicalIndex = GetLogicalAddressIndex(packet);
                return packet.FullPacket[logicalIndex + 1];
            }

            catch (IndexOutOfRangeException)
            {
                return -1;
            }
        }

        /// <summary>
        /// Calculates the packet's sequence number
        /// </summary>
        /// <param name="packet">The packet for which the sequence number should be calculated</param>
        /// <returns>The sequence number</returns>
        public static int GetSequenceNumber(Packet packet)
        {
            try
            {
                if (packet.ProtocolId == 1)
                {
                    return RmapPacketHandler.GetRmapSequenceNumber(packet);
                }
                var logicalIndex = GetLogicalAddressIndex(packet);
                return Convert.ToInt32(packet.FullPacket[logicalIndex + 2]);
            }
            catch (Exception)
            {
                return -1;
            }
        }
    }
}
