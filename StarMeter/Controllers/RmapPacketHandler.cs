using System;
using System.Collections;
using System.Collections.Generic;
using StarMeter.Models;

namespace StarMeter.Controllers
{
    public class RmapPacketHandler
    {
        public RmapPacket CreateRmapPacket(Packet packet)
        {
            int addressIndex = PacketHandler.GetLogicalAddressIndex(packet.FullPacket);
            var rmapCommandByte = new BitArray(new[] {packet.FullPacket[addressIndex + 2]});
            int addressLength = GetRmapLogicalAddressLength(packet.FullPacket[addressIndex + 2]);

            string rmapPacketType = GetRmapType(rmapCommandByte);
            byte[] sourceAddress = GetSourceAddressRmap(packet.FullPacket, addressLength);
            byte destinationKey = GetDestinationKey(packet.FullPacket);
            var rmapPacket = new RmapPacket
            {
                CommandByte = rmapCommandByte,
                PacketType = rmapPacketType,
                SourcePathAddress = sourceAddress,
                PacketId = packet.PacketId,
                DateRecieved = packet.DateRecieved,
                PortNumber = packet.PortNumber,
                Cargo = packet.Cargo,
                ProtocolId = packet.ProtocolId,
                FullPacket = packet.FullPacket,
                PrevPacket = packet.PrevPacket,
                DestinationKey = destinationKey
            };

            if (!CheckRmapCrc(rmapPacket))
            {
                rmapPacket.IsError = true;
                rmapPacket.ErrorType = ErrorTypes.DataError;
            }

            return rmapPacket;
        }

        public static byte[] GetSourceAddressRmap(byte[] rmapFullPacket, int addressLength)
        {
            int addressIndex = PacketHandler.GetLogicalAddressIndex(rmapFullPacket);
            var result = new List<byte>();
            var sourceAddressIndex = addressIndex + 4;
            try
            {
                for (var i = sourceAddressIndex; i < sourceAddressIndex + addressLength; i++)
                {
                    result.Add(rmapFullPacket[i]);
                }
            }
            catch (IndexOutOfRangeException e)
            {
                return result.ToArray();
            }

            return result.ToArray();
        }

        private static int GetRmapLogicalAddressLength(byte rmapCommandByte)
        {
            var finalArray = new BitArray(new[] { getBit(rmapCommandByte, 1), getBit(rmapCommandByte, 2), false, false, false, false, false, false });
            var result = new int[1];
            finalArray.CopyTo(result, 0);
            var final = result[0];
            return final * 4;
        }

        private static bool getBit(byte cmdByte, int index)
        {
            var bit = (cmdByte & (1 << index - 1)) != 0;
            return bit;
        }

        public static bool CheckRmapCrc(RmapPacket packet)
        {
            if (packet.PacketType.EndsWith("Reply"))
            {
                //test cargo CRC
                bool cargo = CRC.CheckCrcForPacket(packet.Cargo);

                //test header CRC
                //remove cargo from header and test as if full packet
                int length = packet.FullPacket.Length - packet.Cargo.Length;
                byte[] headerBytes = new byte[length];
                Array.Copy(packet.FullPacket, headerBytes, length);
                bool header = CRC.CheckCrcForPacket(headerBytes);

                return (header && cargo);
            }
            if (!CRC.CheckCrcForPacket(packet.FullPacket)) return false;

            return true;
        }

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

        public static byte GetDestinationKey(byte[] fullPacket)
        {
            int addressIndex = PacketHandler.GetLogicalAddressIndex(fullPacket);
            return fullPacket[addressIndex + 3];
        }

       
    }
}
