using System;
using System.Collections;
using System.Collections.Generic;
using StarMeter.Models;

namespace StarMeter.Controllers
{
    public class RmapPacketHandler
    {
        public RmapPacket CreateRmapPacket(Packet packet, int addressIndex)
        {
            var rmapCommandByte = Reverse(new BitArray(new[] { packet.FullPacket[addressIndex + 2] }));
            var rmapPacketType = GetRmapType(rmapCommandByte);
            var addressLength = GetRmapLogicalAddressLength(packet.FullPacket[addressIndex + 2]);
            var sourceAddress = GetSourceAddressRmap(packet.FullPacket, addressLength, addressIndex);
            var destinationKey = GetDestinationKey(packet.FullPacket, addressIndex);
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
                DestinationKey = destinationKey
            };

            if (!CheckRmapCrc(rmapPacket))
            {
                rmapPacket.IsError = true;
                rmapPacket.ErrorType = ErrorTypes.DataError;
            }

            return rmapPacket;
        }

        public byte[] GetSourceAddressRmap(byte[] rmapFullPacket, int addressLength, int logicalAddressIndex)
        {
            var result = new List<byte>();
            var sourceAddressIndex = logicalAddressIndex + 4;
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

        private int GetRmapLogicalAddressLength(byte rmapCommandByte)
        {
            var finalArray = new BitArray(new[] { getBit(rmapCommandByte, 1), getBit(rmapCommandByte, 2), false, false, false, false, false, false });
            var result = new int[1];
            finalArray.CopyTo(result, 0);
            var final = result[0];
            return final * 4;
        }

        private bool getBit(byte cmdByte, int index)
        {
            var bit = (cmdByte & (1 << index - 1)) != 0;
            return bit;
        }

        public bool CheckRmapCrc(RmapPacket packet)
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

        public string GetRmapType(BitArray bitArray)
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

        public byte GetDestinationKey(byte[] packetData, int logicalAddressIndex)
        {
            return packetData[logicalAddressIndex + 3];
        }

        //Modified from - Tim Lloyd - StackOverFlow
        public BitArray Reverse(BitArray array)
        {
            var result = array;
            var length = result.Length;
            var mid = length / 2;

            for (var i = 0; i < mid; i++)
            {
                var bit = result[i];
                result[i] = result[length - i - 1];
                result[length - i - 1] = bit;
            }
            return result;
        }
    }
}
