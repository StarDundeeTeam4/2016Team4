﻿using System;
using System.Collections;
using System.Collections.Generic;
using StarMeter.Models;

namespace StarMeter.Controllers
{
    public static class RmapPacketHandler
    {
        /// <summary>
        /// <param name="packet">The packet to use as a base for the RMAP packet</param>
        /// </summary>
        /// <returns>The new RmapPacket</returns>
        public static RmapPacket CreateRmapPacket(Packet packet)
        {
            var rmapPacket = new RmapPacket();
            //setting vars to be essentially null so packet can be created even if error
            BitArray rmapCommandByte = null;
            byte destinationKey = 0x00;
            var rmapPacketType = "";
            byte[] secondaryAddress = null;
            var addressIndex = PacketHandler.GetLogicalAddressIndex(packet);

            try
            {
                if (addressIndex == -1)
                {
                    throw new IndexOutOfRangeException(); //no logical address so is incomplete or otherwise dataerror
                }
                rmapCommandByte = new BitArray(new[] {packet.FullPacket[addressIndex + 2]});
                destinationKey = GetDestinationKey(packet);
                rmapPacketType = GetRmapType(rmapCommandByte);
                secondaryAddress = GetSecondaryAddressRmap(packet);
            }
            catch (IndexOutOfRangeException)
            {
                rmapPacket.IsError = true;
                rmapPacket.ErrorType = ErrorType.DataError;
            }

            rmapPacket.PacketId     = packet.PacketId;
            rmapPacket.DateReceived = packet.DateReceived;
            rmapPacket.PrevPacket   = packet.PrevPacket;
            rmapPacket.PacketType   = rmapPacketType;
            
            rmapPacket.PortNumber   = packet.PortNumber;
            rmapPacket.ProtocolId   = packet.ProtocolId;
            rmapPacket.SequenceNum  = packet.SequenceNum;
            rmapPacket.Crc          = packet.Crc;
            rmapPacket.DestinationAddress      = packet.DestinationAddress;
            rmapPacket.SecondaryAddress = secondaryAddress;

            rmapPacket.CommandByte       = rmapCommandByte;
            rmapPacket.DestinationKey    = destinationKey;
            rmapPacket.Cargo             = packet.Cargo;
            rmapPacket.FullPacket        = packet.FullPacket;

            return rmapPacket;
        }
        /// <summary>
        /// Calculate transaction identifier
        /// </summary>
        /// <param name="packet">The packet</param>
        /// <param name="addressIndex">the index of the destination logical address</param>
        /// <returns>The Transaction Identifier of this rmap packet</returns>
        public static int GetTransactionIdentifier(Packet packet, int addressIndex)
        {
            var fullPacket = packet.FullPacket;
            //Location of transaction Identifier according to protocol specification
            var transactionBytes = new[] {fullPacket[addressIndex + 6], fullPacket[addressIndex + 5]};
            //Convert back to unsigned 16 bit integer (byte + byte = 16 bits.) 
            var final = BitConverter.ToUInt16(transactionBytes, 0);
            return final;
        }

        /// <summary>
        /// Calculate the source address for the packet
        /// </summary>
        /// <param name="rmapPacket"></param>
        /// <returns>The source address byte array</returns>
        public static byte[] GetSecondaryAddressRmap(Packet rmapPacket)
        {
            var addressArray = new byte[0];
            var addressIndex = PacketHandler.GetLogicalAddressIndex(rmapPacket);
            var destinationKeyIndex = addressIndex + 3;
            try
            {
                var secondaryAddressIndex = -1;
                for (var i = destinationKeyIndex + 1; i < rmapPacket.FullPacket.Length; i++)
                {
                    if (rmapPacket.FullPacket[i] >= 32)
                    {
                        secondaryAddressIndex = i;
                        break;
                    }
                }
                var addressLength = secondaryAddressIndex - destinationKeyIndex;
                addressArray = new byte[addressLength];
                Array.Copy(rmapPacket.FullPacket, destinationKeyIndex + 1, addressArray, 0, addressLength);
            }
            catch (IndexOutOfRangeException e)
            {
                System.Diagnostics.Trace.WriteLine("IndexOutOfRangeException in GetSecondaryAddressRmap: " + e);
            }
            catch (OverflowException e)
            {
                System.Diagnostics.Trace.WriteLine("OverflowException in GetSecondaryAddressRmap: " + e);
            }

            return addressArray;
        }

        /// <summary>
        /// Calculates the length of the packet's source address bytes
        /// </summary>
        /// <param name="rmapCommandByte">The command byte to calculate from</param>
        /// <returns>The length of the source address</returns>
        public static int GetRmapSourcePathAddressLength(byte rmapCommandByte)
        {
            var lastTwoBitsFromCommandByte = new BitArray(new[] { GetBit(rmapCommandByte, 1), GetBit(rmapCommandByte, 2), false, false, false, false, false, false });
            var container = new int[1];
            lastTwoBitsFromCommandByte.CopyTo(container, 0);
            var final = container[0];
            return final * 4;
        }

        /// <summary>
        /// Returns requested bit in byte indicated by index
        /// Explanation of use of bitwise operators - http://stackoverflow.com/questions/4854207/get-a-specific-bit-from-byte
        /// Authors - KeithS, Josh Petrie, PierrOz, Aliostad
        /// </summary>
        /// <param name="">The command byte to calculate from</param>
        /// <param name="myByte">byte whose bit is to be extracted</param>
        /// <param name="index">which bit to return</param>
        /// <returns>the bit requested</returns>
        public static bool GetBit(byte myByte, int index)
        {
            var bit = (myByte & (1 << index - 1)) != 0;
            return bit;
        }

        /// <summary>
        /// Checks that the calculated CRC(s) for an RMAP packet are as provided
        /// </summary>
        /// <param name="packet">The packet to check</param>
        /// <returns>Whether the CRC byte(s) are the same as calculated</returns>
        public static bool CheckRmapCrc(RmapPacket packet)
        {
            if (packet.PacketType.EndsWith("Reply"))
            {
                //test cargo CRC
                var cargo = Crc.CheckCrcForPacket(packet.Cargo);
                //test header CRC
                //remove cargo from header and test as if full packet
                var length = packet.FullPacket.Length - packet.Cargo.Length;
                var headerBytes = new byte[length];
                Array.Copy(packet.FullPacket, headerBytes, length);
                var header = Crc.CheckCrcForPacket(headerBytes);
                return header && cargo;
            }
            return Crc.CheckCrcForPacket(packet.FullPacket);
        }

        /// <summary>
        /// Calculates the packet's type from the command byte
        /// </summary>
        /// <param name="bitArray">The command byte as individual bits</param>
        /// <returns>The packet type as a string</returns>
        public static string GetRmapType(BitArray bitArray)
        {
            var result = "";
            if (bitArray[5])
            {
                result += "Write";
            }
            else if (bitArray[4])
            {
                result += "Read Modify Write";
            }
            else
            {
                result += "Read";
            }
            if (!bitArray[6])
            {
                result += " Reply";
            }
            return result;
        }

        /// <summary>
        /// Calculates the destination key from the packet
        /// </summary>
        /// <param name="packet">The packet's data</param>
        /// <returns>The destination key byte</returns>
        public static byte GetDestinationKey(Packet packet)
        {
            var addressIndex = PacketHandler.GetLogicalAddressIndex(packet);
            return packet.FullPacket[addressIndex + 3];
        }

        /// <summary>
        /// Calculates the RMAP packet's sequence number
        /// </summary>
        /// <param name="packet">The packet for which the sequence number should be calculated</param>
        /// <returns>The sequence number</returns>
        public static int GetRmapSequenceNumber(Packet packet)
        {
            var logicalIndex = PacketHandler.GetLogicalAddressIndex(packet);
            var sequence = new byte[2];

            Array.Copy(packet.FullPacket, logicalIndex + 5, sequence, 0, 2);
            Array.Reverse(sequence); //damn little-endian-ness

            return BitConverter.ToUInt16(sequence, 0);
        }
    }
}
