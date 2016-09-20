using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using StarMeter.Interfaces;
using StarMeter.Models;

namespace StarMeter.Controllers
{
    public class Parser
    {
        public Dictionary<Guid, Packet> PacketDict = new Dictionary<Guid, Packet>();
        private Guid? _prevPacket;

        public Dictionary<Guid, Packet> ParseFile(string filePath)
        {
            _prevPacket = null;
            var r = new StreamReaderWrapper(filePath);
            PacketDict = ParsePackets(r);
            return PacketDict;
        }

        public Dictionary<Guid, Packet> ParsePackets(IStreamReader r)
        {
            PacketDict.Clear();
            string line;
            r.ReadLine();
            var strPortNumber = r.ReadLine();
            var portNumber = int.Parse(strPortNumber);
            r.ReadLine();
            while ((line = r.ReadLine()) != null && r.Peek() > -1)
            {
                var packetId = Guid.NewGuid();
                var packet = new Packet {PortNumber = portNumber, PacketId = packetId};

                DateTime tempDate;
                if (ParseDateTime(line, out tempDate))
                {
                    packet.DateRecieved = tempDate;
                }

                var packetType = r.ReadLine();
                if (IsPType(packetType))
                {
                    //read cargo line and convert to byte array
                    var packetHexData = r.ReadLine().Split(' ');
                    packet.FullPacket = packetHexData.Select(item => byte.Parse(item, NumberStyles.HexNumber)).ToArray();

                    var logicalAddressIndex = GetLogicalAddressIndex(packet.FullPacket);

                    packet.Cargo = GetCargoArray(packet, logicalAddressIndex);
                    packet.ProtocolId = GetProtocolId(packet.FullPacket, logicalAddressIndex);
                    if (packet.ProtocolId == 1)
                    {
                        packet = CreateRmapPacket(packet, logicalAddressIndex);
                    }
                    packet.Address = GetAddressArray(packet.FullPacket, logicalAddressIndex);
                    packet.Crc = GetCrc(packet.FullPacket);
                    packet.SequenceNum = GetSequenceNumber(packet, logicalAddressIndex);
                    packet.ErrorType = GetErrorType(packet);

                    var endingState = r.ReadLine();
                    packet.IsError = string.CompareOrdinal(endingState, "EOP") != 0;

                }
                else
                {
                    packet.IsError = true;

                    r.ReadLine();
                }


                packet = SetPrevPacket(packet);

                PacketDict.Add(packetId, packet);
                r.ReadLine();
            }
            return PacketDict;
        }

        private RmapPacket CreateRmapPacket(Packet packet, int addressIndex)
        {
            var rmapCommandByte = new BitArray(new[] {packet.FullPacket[addressIndex + 2]});
            var rmapPacketType = GetRmapType(rmapCommandByte);
            var addressLength = GetRmapLogicalAddressLength(packet.FullPacket[addressIndex + 2]);
            var sourceAddress = GetSourceAddressRmap(packet.FullPacket, addressLength, addressIndex);

            var p = new RmapPacket()
            {
                AdditionalInfo    = rmapCommandByte,
                PacketType        = rmapPacketType,
                SourcePathAddress = sourceAddress,

                PacketId          = packet.PacketId,
                DateRecieved      = packet.DateRecieved,
                PortNumber        = packet.PortNumber,
                Cargo             = packet.Cargo,
                ProtocolId        = packet.ProtocolId,
                FullPacket        = packet.FullPacket,
            };

            if (!CheckRmapCRC(p))
            {
                p.IsError = true;
                p.ErrorType = ErrorTypes.DataError;
            }

            return p;
        }

        public byte[] GetSourceAddressRmap(byte[] rmapFullPacket, int addressLength, int logicalAddressIndex)
        {
            var result = new List<byte>();
            int sourceAddressIndex = logicalAddressIndex+5;
            for (var i = sourceAddressIndex; i < sourceAddressIndex + addressLength; i++)
            {
                result.Add(rmapFullPacket[i]);
            }
            return result.ToArray();
        }

        private int GetRmapLogicalAddressLength(byte rmapCommandByte)
        {
            var finalArray = new BitArray(new[] { getBit(rmapCommandByte, 1), getBit(rmapCommandByte, 2), false, false, false, false, false, false });
            var result = new int[1];
            finalArray.CopyTo(result, 0);
            var final = result[0];
            return final*4;
        }

        private bool getBit(byte cmdByte, int index)
        {
            var bit = (cmdByte & (1 << index - 1)) != 0;
            return bit;
        }

        public string GetRmapType(BitArray bitArray)
        {
            /**
             * bit 7 = reserved
             * bit 6 = read/write
             * bit 5 = reply?
             * bit 4 = verify
             */

            string result = bitArray[5] ? "Write" : "Read";
            
            if (!bitArray[6]) //reply?
            {
                result += " Reply";
            }

            return result;

        }

        public Packet SetPrevPacket(Packet packet)
        {
            //set previous packet's next packet as this packet
            if (_prevPacket != null)
            {
                Guid prev = _prevPacket.GetValueOrDefault(); //converting from nullable to non-nullable
                PacketDict[prev].NextPacket = packet.PacketId;
            }
            //set current packet's previous packet
            packet.PrevPacket = _prevPacket;
            //store this id as the previous packet
            _prevPacket = packet.PacketId;
            return packet;
        }

        private static bool IsPType(string packetType)
        {
            return string.CompareOrdinal(packetType, "P") == 0;
        }

        public bool ParseDateTime(string stringDateTime, out DateTime result)
        {
            return DateTime.TryParseExact(stringDateTime, "dd-MM-yyyy HH:mm:ss.fff", null, DateTimeStyles.None, out result);
        }

        public int GetLogicalAddressIndex(byte[] fullPacket)
        {
            for (int i = 0; i < fullPacket.Length; i++)
            {
                if (fullPacket[i] >= 32) return i;
            }
            return -1;
        }

        public byte[] GetCargoArray(Packet packet, int logicalIndex)
        {
            byte[] cargo;
            if (packet.ProtocolId == 1)
            {
                string type = GetRmapType(new BitArray(new[] { packet.FullPacket[GetLogicalAddressIndex(packet.FullPacket) + 2] }));
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

        public byte[] GetAddressArray(byte[] fullPacket, int logicalIndex)
        {
            byte[] addressArray = new byte[logicalIndex+1];
            Array.Copy(fullPacket, addressArray, logicalIndex+1);
            return addressArray;
        }

        public byte GetCrc(byte[] fullPacket)
        {
            return fullPacket.Last();
            //return (byte)Convert.ToInt32(fullPacket[fullPacket.Length - 1], 16);
        }

        public int GetProtocolId(byte[] fullPacket, int logicalIndex)
        {
            return fullPacket[logicalIndex + 1];
        }

        public int GetSequenceNumber(Packet packet, int logicalIndex)
        {
            if (packet.GetType() == typeof(RmapPacket))
            {
                byte[] sequence = new byte[2];
                Array.Copy(packet.FullPacket, logicalIndex + 5, sequence, 0, 2);
                Array.Reverse(sequence); //damn little-endian-ness
                return BitConverter.ToUInt16(sequence, 0);
            }
            else
            {
                return Convert.ToInt32(packet.FullPacket[logicalIndex + 2]);
            }
        }

        public bool CheckRmapCRC(RmapPacket packet)
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
            else
            {
                if (!CRC.CheckCrcForPacket(packet.FullPacket)) return false;
            }

            return true;
        }

        public ErrorTypes GetErrorType(Packet packet)
        {
            var calculatedCrc = CRC.CheckCrcForPacket(packet.FullPacket);
            if (!calculatedCrc)
            {
                return ErrorTypes.DataError;
            }
            return ErrorTypes.None;
        }

    }
}