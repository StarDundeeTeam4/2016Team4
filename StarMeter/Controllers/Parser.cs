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
        public Guid? PrevPacket;

        /// <summary>
        /// Send a file specified by filePath to be parsed
        /// </summary>
        /// <param name="filePath">The path of the file to parse</param>
        /// <returns>A Dictionary of Packets paired with their IDs of the file's contents</returns>
        public Dictionary<Guid, Packet> ParseFile(string filePath)
        {
            PrevPacket = null;
            var streamReader = new StreamReaderWrapper(filePath);
            PacketDict.Clear();
            PacketDict = ParsePackets(streamReader);
            streamReader.Close();
            PrevPacket = null;
            return PacketDict;
        }

        /// <summary>
        /// Parse input from an IStreamReader
        /// </summary>
        /// <param name="r">The reader to read from</param>
        /// <returns>A Dictionary of Packets paired with their IDs of the file's contents</returns>
        public Dictionary<Guid, Packet> ParsePackets(IStreamReader r)
        {
            string line;
            r.ReadLine();

            var strPortNumber = r.ReadLine();
            var portNumber = int.Parse(strPortNumber);

            r.ReadLine();
            while ((line = r.ReadLine()) != null && r.Peek() > -1)
            {
                var packetId = Guid.NewGuid();
                var packet = new Packet
                {
                    PortNumber = portNumber,
                    PacketId = packetId
                };
                DateTime tempDate;

                if (PacketHandler.ParseDateTime(line, out tempDate))
                {
                    packet.DateReceived = tempDate;
                }

                var packetType = r.ReadLine();

                if (PacketHandler.IsPType(packetType))
                {
                    packet = SetPrevPacket(packet);
                    //read cargo line and convert to byte array
                    var packetAsStrings = r.ReadLine().Split(' ');
                    packet.FullPacket =
                        packetAsStrings.Select(item => byte.Parse(item, NumberStyles.HexNumber)).ToArray();
                    packet = PacketHandler.SetPacketInformation(packet);

                    if (packet.ProtocolId == 1)
                    {
                        packet = RmapPacketHandler.CreateRmapPacket(packet);
                    }

                    var endingState = r.ReadLine();
                    packet.IsError = string.CompareOrdinal(endingState, "EOP") != 0;
                }
                else if (packetType == null)
                {
                    break;
                }
                else
                {
                    packet.PrevPacket = PrevPacket;
                    var error = r.ReadLine();

                    if (error == "Disconnect")
                    {
                        packet.ErrorType = ErrorType.Disconnect;
                    }

                    if (PacketDict.Count >= 2)
                    {
                        var errorDetector = new ErrorDetector();
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

        /// <summary>
        /// Sets the packet's previous guid and the previous packet's next packet
        /// </summary>
        /// <param name="packet">The packet for which the previous should be set</param>
        /// <returns>The updated packet</returns>
        public Packet SetPrevPacket(Packet packet)
        {
            //set previous packet's next packet as this packet
            if (PrevPacket != null)
            {
                var prev = PrevPacket.GetValueOrDefault(); //converting from nullable to non-nullable
                PacketDict[prev].NextPacket = packet.PacketId;
            }
            //set current packet's previous packet
            packet.PrevPacket = PrevPacket;
            //store this id as the previous packet
            PrevPacket = packet.PacketId;
            return packet;
        }

        /// <summary>
        /// Gets the non-nullable previous packet from the nullable variable
        /// </summary>
        /// <param name="packet">The packet for which the previous should be returned</param>
        /// <returns>The previous packet</returns>
        public Packet GetPrevPacket(Packet packet)
        {
            var prevPacketId = (Guid)packet.PrevPacket;
            Packet previousPacket;
            PacketDict.TryGetValue(prevPacketId, out previousPacket);
            return previousPacket;
        }
    }
}