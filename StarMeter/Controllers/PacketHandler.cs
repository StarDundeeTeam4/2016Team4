using System;
using System.Linq;
using StarMeter.Models;
using System.Globalization;
using System.Collections;

namespace StarMeter.Controllers
{
    public static class PacketHandler
    {
        public static bool IsPType(string packetType)
        {
            return string.CompareOrdinal(packetType, "P") == 0;
        }

        public static bool ParseDateTime(string stringDateTime, out DateTime result)
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

        public static byte[] GetCargoArray(Packet packet)
        {
            int logicalIndex = GetLogicalAddressIndex(packet.FullPacket);
            byte[] cargo;

            try
            {
                if (packet.ProtocolId == 1)
                {
                    string type =
                        RmapPacketHandler.GetRmapType(
                            new BitArray(new[] {packet.FullPacket[GetLogicalAddressIndex(packet.FullPacket) + 2]}));

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
            catch (IndexOutOfRangeException)
            {
                return null;
                //need to set incomplete packet error?
            }
        }

        public static byte[] GetAddressArray(byte[] fullPacket)
        {
            int logicalIndex = GetLogicalAddressIndex(fullPacket);
            byte[] addressArray = new byte[logicalIndex + 1];
            Array.Copy(fullPacket, addressArray, logicalIndex + 1);
            return addressArray;
        }

        public static byte GetCrc(byte[] fullPacket)
        {
            return fullPacket.Last();
        }

        public static int GetProtocolId(byte[] fullPacket)
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

        public static int GetSequenceNumber(Packet packet)
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
