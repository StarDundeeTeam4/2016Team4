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
        private Guid? _prevPacket = null;

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

                    var logicalAddressIndex = GetLogicalAddressIndex(packetHexData);

                    packet.Cargo = GetCargoArray(packetHexData, logicalAddressIndex);
                    packet.ProtocolId = GetProtocolId(packetHexData, logicalAddressIndex);
                    if (packet.ProtocolId == 1)
                    {
                        var tmpPacket = packet;
                        var rmapPacketType = GetRmapType(packet.Cargo[logicalAddressIndex + 1]);
                        packet = new RmapPacket()
                        {
                            DateRecieved = tempDate,
                            PortNumber = tmpPacket.PortNumber,
                            PacketId = tmpPacket.PacketId,
                            PacketType = rmapPacketType,
                            Cargo = tmpPacket.Cargo,
                            ProtocolId = tmpPacket.ProtocolId,
                            FullPacket = tmpPacket.FullPacket
                        };
                    }
                    packet.Address = GetAddressArray(packetHexData, logicalAddressIndex);
                    packet.Crc = GetCrc(packetHexData);
                    packet.SequenceNum = GetSequenceNumber(packetHexData, logicalAddressIndex);
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

        public string GetRmapType(byte rmapCommandByte)
        {
            var result = "";
            var bitArray = new BitArray(new[] {rmapCommandByte});

            if (!bitArray[6])
            {
                result += "Reply ";
            }
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

        public int GetLogicalAddressIndex(string[] fullPacket)
        {
            int index = 0, i = 0;
            foreach (var hexString in fullPacket)
            {
                var hexValue = Convert.ToInt32(hexString, 16);
                if (hexValue >= 32)
                {
                    index = i;
                    break;
                }
                i++;
            }
            return index;
        }

        public byte[] GetCargoArray(string[] fullPacket, int logicalIndex)
        {
            var byteList = new List<byte>();
            for (var i = logicalIndex + 1 ; i <= fullPacket.Length - 1; i++)
            {
                byteList.Add((byte)Convert.ToInt32(fullPacket[i], 16));
            }

            return byteList.ToArray();
        }

        public byte[] GetAddressArray(string[] fullPacket, int logicalIndex)
        {
            var byteList = new List<byte>();
            for (var i = 0; i <= logicalIndex; i++)
            {
                byteList.Add((byte)Convert.ToInt32(fullPacket[i], 16));
            }

            return byteList.ToArray();
        }

        public byte GetCrc(string[] fullPacket)
        {
            return (byte)Convert.ToInt32(fullPacket[fullPacket.Length - 1], 16);
        }

        public int GetProtocolId(string[] fullPacket, int logicalIndex)
        {
            return Convert.ToInt32(fullPacket[logicalIndex + 1], 16);
        }

        public int GetSequenceNumber(string[] fullPacket, int logicalIndex)
        {
            return Convert.ToInt32(fullPacket[logicalIndex + 2], 16);
        }

        public ErrorTypes GetErrorType(Packet packet)
        {
            var calculatedCrc = Crc.CheckCrcForPacket(packet.FullPacket);
            if (!calculatedCrc)
            {
                return ErrorTypes.DataError;
            }
            return ErrorTypes.None;
        }

    }
}