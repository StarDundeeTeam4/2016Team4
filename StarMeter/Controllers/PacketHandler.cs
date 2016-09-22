﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StarMeter.Models;
using System.Globalization;
using System.Collections;

namespace StarMeter.Controllers
{
    public class PacketHandler
    {
        private Guid? _prevPacket;

        public bool IsPType(string packetType)
        {
            return string.CompareOrdinal(packetType, "P") == 0;
        }

        public bool ParseDateTime(string stringDateTime, out DateTime result)
        {
            return DateTime.TryParseExact(stringDateTime, "dd-MM-yyyy HH:mm:ss.fff", null, DateTimeStyles.None, out result);
        }

        public static int GetLogicalAddressIndex(byte[] fullPacket)
        {
            for (int i = 0; i < fullPacket.Length; i++)
            {
                if (fullPacket[i] >= 32) return i;
            }
            return -1;
        }

        public byte[] GetCargoArray(Packet packet)
        {
            int logicalIndex = GetLogicalAddressIndex(packet.FullPacket);
            byte[] cargo;
            if (packet.ProtocolId == 1)
            {
                string type = RmapPacketHandler.GetRmapType(new BitArray(new[] { packet.FullPacket[GetLogicalAddressIndex(packet.FullPacket) + 2] }));
                if (type.EndsWith("Reply"))
                {
                    int start = logicalIndex + 12;
                    int cargoLength = packet.FullPacket.Length - start;
                    cargo = new byte[cargoLength];
                    Array.Copy(packet.FullPacket, start, cargo, 0, cargoLength);
                    return cargo;
                }
            }
            logicalIndex++;
            int length = packet.FullPacket.Length - logicalIndex;
            cargo = new byte[length];
            Array.Copy(packet.FullPacket, logicalIndex, cargo, 0, length);

            return cargo;
        }

        public byte[] GetAddressArray(byte[] fullPacket)
        {
            int logicalIndex = GetLogicalAddressIndex(fullPacket);
            byte[] addressArray = new byte[logicalIndex + 1];
            Array.Copy(fullPacket, addressArray, logicalIndex + 1);
            return addressArray;
        }

        public byte GetCrc(byte[] fullPacket)
        {
            return fullPacket.Last();
            //return (byte)Convert.ToInt32(fullPacket[fullPacket.Length - 1], 16);
        }

        public int GetProtocolId(byte[] fullPacket)
        {
            try
            {
                int logicalIndex = GetLogicalAddressIndex(fullPacket);
                return fullPacket[logicalIndex + 1];
            }

            catch (IndexOutOfRangeException e)
            {
                return -1;
            }
        }

        public int GetSequenceNumber(Packet packet)
        {
            try
            {
                int logicalIndex = GetLogicalAddressIndex(packet.FullPacket);
                if (packet.GetType() != typeof(RmapPacket)) return Convert.ToInt32(packet.FullPacket[logicalIndex + 2]);
                byte[] sequence = new byte[2];

                Array.Copy(packet.FullPacket, logicalIndex + 5, sequence, 0, 2);
                Array.Reverse(sequence); //damn little-endian-ness


                return BitConverter.ToUInt16(sequence, 0);
            }
            catch (Exception e)
            {
                return -1;
            }
        }
    }
}