using System;
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
            PacketDict.Clear();
            PacketDict = ParsePackets(r);
            r.Close();
            return PacketDict;
        }

        public Dictionary<Guid, Packet> ParsePackets(IStreamReader r)
        {
            string line;
            r.ReadLine();

            string strPortNumber = r.ReadLine();
            int portNumber = int.Parse(strPortNumber);

            r.ReadLine();
            while ((line = r.ReadLine()) != null && r.Peek() > -1)
            {
                Guid packetId = Guid.NewGuid();
                Packet packet = new Packet {PortNumber = portNumber, PacketId = packetId};

                DateTime tempDate;
                if (PacketHandler.ParseDateTime(line, out tempDate))
                {
                    packet.DateRecieved = tempDate;
                }

                string packetType = r.ReadLine();

                if (PacketHandler.IsPType(packetType))
                {
                    packet = SetPrevPacket(packet);

                    //read cargo line and convert to byte array
                    string[] packetAsStrings = r.ReadLine().Split(' ');
                    packet.FullPacket = packetAsStrings.Select(item => byte.Parse(item, NumberStyles.HexNumber)).ToArray();

                    packet = ParseHexLine(packet);

                    if (packet.ProtocolId == 1)
                    {
                        packet = RmapPacketHandler.CreateRmapPacket(packet);
                    }

                    string endingState = r.ReadLine();
                    packet.IsError = string.CompareOrdinal(endingState, "EOP") != 0;
                }
                else
                {
                    packet.PrevPacket = _prevPacket;

                    packet.IsError = true;

                    string error = r.ReadLine();
                    if (error == "Disconnect")
                    {
                        packet.ErrorType = ErrorType.Disconnect;
                    }

                    if (PacketDict.Count > 2) {
                        ErrorDetector errorDetector = new ErrorDetector();
                        var previousPacket = GetPrevPacket(packet);
                        var previousPreviousPacket = GetPrevPacket(previousPacket);
                        previousPacket.ErrorType = errorDetector.GetErrorType(previousPreviousPacket, previousPacket);
                        previousPacket.IsError = true;
                    }
                    r.ReadLine();
                    continue;
                }

                PacketDict.Add(packetId, packet);
                r.ReadLine();
            }
            return PacketDict;
        }

        public Packet ParseHexLine(Packet packet)
        {
            try
            {
                packet.Crc = PacketHandler.GetCrc(packet.FullPacket); //can't fail unless everything is fucked

                //next four lines must be done in order as if packet ends early, everything before will work and everything after will fail anyway
                packet.Address = PacketHandler.GetAddressArray(packet.FullPacket);
                packet.ProtocolId = PacketHandler.GetProtocolId(packet.FullPacket);
                packet.SequenceNum = PacketHandler.GetSequenceNumber(packet);
                packet.Cargo = PacketHandler.GetCargoArray(packet);
            }
            catch (IndexOutOfRangeException)
            {
                packet.IsError = true;
                packet.ErrorType = ErrorType.DataError; //Incomplete packet is DataError?
            }

            return packet;
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

        private Packet GetPrevPacket(Packet packet)
        {
            Guid prevPacketId = (Guid)packet.PrevPacket;
            Packet previousPacket;
            PacketDict.TryGetValue(prevPacketId, out previousPacket);
            return previousPacket;
        }
    }
}