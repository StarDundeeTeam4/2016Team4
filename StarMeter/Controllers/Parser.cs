using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
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

                DateTime temp;
                if (ParseDateTime(line, out temp))
                {
                    packet.DateRecieved = temp;
                }

                var packetType = r.ReadLine();
                if (IsPType(packetType))
                {
                    //read cargo line and convert to byte array
                    string[] hex = r.ReadLine().Split(' ');
                    packet.Cargo = hex.Select(item => byte.Parse(item, NumberStyles.HexNumber)).ToArray();

                    var endingState = r.ReadLine();
                    packet.IsError = string.CompareOrdinal(endingState, "EOP") != 0;
                }
                else
                {
                    packet.IsError = true;
                    r.ReadLine();
                }

                packet = setPrevPacket(packet);

                PacketDict.Add(packetId, packet);
                r.ReadLine();
            }
            return PacketDict;
        }

        public Packet setPrevPacket(Packet packet)
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

        public byte[] ParseCargo(string line)
        {
            return new byte[1];
        }

        public static bool ParseDateTime(string stringDateTime, out DateTime result)
        {
            return DateTime.TryParseExact(stringDateTime, "dd-MM-yyyy HH:mm:ss.fff", null, DateTimeStyles.None, out result);
        }

        public int GetLogicalAddressIndex(string[] cargoParam)
        {
            int index = 0, i = 0;
            foreach (var hexString in cargoParam)
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

        public static byte[] GetAddressArray(int logicalIndex, string[] cargoParam)
        {
            var byteList = new List<byte>();
            for (var i = 0; i <= logicalIndex; i++)
            {
                byteList.Add((byte)Convert.ToInt32(cargoParam[i], 16));
            }

            return byteList.ToArray();
        }

        public static byte GetCrc(string[] cargoParam)
        {
            return (byte)Convert.ToInt32(cargoParam[cargoParam.Length - 1], 16);
        }

        public static int GetProtocolId(string[] cargoParam, int logicalIndex)
        {
            return Convert.ToInt32(cargoParam[logicalIndex+1], 16);
        }

        public int GetSequenceNumber(string[] cargoParam, int logicalIndex)
        {
            return Convert.ToInt32(cargoParam[logicalIndex + 2], 16);
        }
    }
}