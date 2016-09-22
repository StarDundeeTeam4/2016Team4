using System;
using System.Collections;
using System.Collections.Generic;
using StarMeter.Models;

namespace StarMeter.Controllers
{
    public static class RmapPacketHandler
    {
        public static RmapPacket CreateRmapPacket(Packet packet)
        {
            RmapPacket rmapPacket = new RmapPacket();

            //setting vars to be essentially null so packet can be created even if error
            BitArray rmapCommandByte = null;
            byte destinationKey = 0x00;
            int addressLength = -1;
            string rmapPacketType = "";
            byte[] sourceAddress = null;

            int addressIndex = PacketHandler.GetLogicalAddressIndex(packet.FullPacket);

            try
            {
                if (addressIndex == -1)
                {
                    throw new IndexOutOfRangeException(); //no logical address so is incomplete or otherwise dataerror
                }

                rmapCommandByte = new BitArray(new[] {packet.FullPacket[addressIndex + 2]});
                destinationKey = GetDestinationKey(packet.FullPacket);
                addressLength = GetRmapLogicalAddressLength(packet.FullPacket[addressIndex + 2]);

                rmapPacketType = GetRmapType(rmapCommandByte);
                sourceAddress = GetSourceAddressRmap(packet.FullPacket, addressLength);
            }
            catch (IndexOutOfRangeException)
            {
                rmapPacket.IsError = true;
                rmapPacket.ErrorType = ErrorType.DataError;
            }

            rmapPacket.PacketId     = packet.PacketId;
            rmapPacket.DateRecieved = packet.DateRecieved;
            rmapPacket.PrevPacket   = packet.PrevPacket;
            rmapPacket.PacketType   = rmapPacketType;

            rmapPacket.PortNumber = packet.PortNumber;
            rmapPacket.ProtocolId = packet.ProtocolId;

            rmapPacket.CommandByte       = rmapCommandByte;
            rmapPacket.DestinationKey    = destinationKey;
            rmapPacket.SourcePathAddress = sourceAddress;
            rmapPacket.Cargo             = packet.Cargo;
            rmapPacket.FullPacket        = packet.FullPacket;

            if (!CheckRmapCrc(rmapPacket))
            {
                rmapPacket.IsError = true;
                rmapPacket.ErrorType = ErrorType.DataError;
            }

            return rmapPacket;
        }

        public static byte[] GetSourceAddressRmap(byte[] rmapFullPacket, int addressLength)
        {
            int addressIndex = PacketHandler.GetLogicalAddressIndex(rmapFullPacket);
            int sourceAddressIndex = addressIndex + 4;

            var result = new List<byte>();
            try
            {
                for (int i = 0; i < addressLength; i++)
                {
                    result.Add(rmapFullPacket[sourceAddressIndex + i]);
                }
            }
            catch (IndexOutOfRangeException e)
            {
                System.Diagnostics.Trace.WriteLine("IndexOutOfRangeException in GetSourceAddressRmap");
                System.Diagnostics.Trace.WriteLine(e);
            }

            return result.ToArray();
        }

        public static int GetRmapLogicalAddressLength(byte rmapCommandByte)
        {
            //What does this do/how does it work?
            var finalArray = new BitArray(new[] { getBit(rmapCommandByte, 1), getBit(rmapCommandByte, 2), false, false, false, false, false, false });
            var result = new int[1];
            finalArray.CopyTo(result, 0);
            var final = result[0];
            return final * 4;
        }

        public static bool getBit(byte cmdByte, int index)
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
