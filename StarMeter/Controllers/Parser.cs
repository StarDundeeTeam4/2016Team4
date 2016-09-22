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
        private readonly PacketHandler _packetHandler = new PacketHandler();
        private readonly RmapPacketHandler _rmapPacketHandler = new RmapPacketHandler();

        public Dictionary<Guid, Packet> ParseFile(string filePath)
        {
            _prevPacket = null;
            var r = new StreamReaderWrapper(filePath);
            PacketDict = ParsePackets(r);
            r.Close();
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
                if (_packetHandler.ParseDateTime(line, out tempDate))
                {
                    packet.DateRecieved = tempDate;
                }

                var packetType = r.ReadLine();
                packet = SetPrevPacket(packet);
                if (_packetHandler.IsPType(packetType))
                {
                    //read cargo line and convert to byte array
                    var packetHexData = r.ReadLine().Split(' ');
                    packet.FullPacket = packetHexData.Select(item => byte.Parse(item, NumberStyles.HexNumber)).ToArray();

                    var endingState = r.ReadLine();
                    packet.IsError = string.CompareOrdinal(endingState, "EOP") != 0;

                    packet.ProtocolId = _packetHandler.GetProtocolId(packet.FullPacket);
                    packet.Cargo = _packetHandler.GetCargoArray(packet);
                    packet.Address = _packetHandler.GetAddressArray(packet.FullPacket);
                    packet.Crc = _packetHandler.GetCrc(packet.FullPacket);
                    packet.SequenceNum = _packetHandler.GetSequenceNumber(packet);
                    if (packet.ProtocolId == 1)
                    {
                        packet = _rmapPacketHandler.CreateRmapPacket(packet);
                    }
                    else
                    {
                        packet.ErrorType = GetErrorType(packet);
                    }
                }
                else
                {
                    packet.IsError = true;
                    var error = r.ReadLine();
                    if (error == "Disconnect")
                    {
                        packet.ErrorType = ErrorTypes.Disconnect;
                    }

                    if (PacketDict.Count > 2) { 
                        ErrorDetector errorDetector = new ErrorDetector();
                        var previousPacket = GetPrevPacket(packet);
                        var previousPreviousPacket = GetPrevPacket(previousPacket);
                        previousPacket.ErrorType = errorDetector.GetErrorType(previousPreviousPacket, previousPacket);
                        previousPacket.IsError = true;
                    }
                }

                

                PacketDict.Add(packetId, packet);
                r.ReadLine();
            }
            PacketDict.Remove(PacketDict.Keys.Last());
            return PacketDict;
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

        public ErrorTypes GetErrorType(Packet packet)
        {
            bool CrcValid;
            if (packet.GetType() == typeof(RmapPacket))
            {
                CrcValid = RmapPacketHandler.CheckRmapCrc((RmapPacket)packet);
            }
            else
            {
                CrcValid = CRC.CheckCrcForPacket(packet.FullPacket);
            }
            return !CrcValid ? ErrorTypes.DataError : ErrorTypes.None;
        }

    }
}