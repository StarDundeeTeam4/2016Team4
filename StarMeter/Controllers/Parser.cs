using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using StarMeter.Interfaces;
using StarMeter.Models;

namespace StarMeter.Controllers
{
    public class Parser
    {
        public Dictionary<Guid, Packet> PacketDict = new Dictionary<Guid, Packet>();

        private const string DateTimeRegex = @"^(0[1-9]|1\d|2[0-8]|29(?=-\d\d-(?!1[01345789]00|2[1235679]00)\d\d(?:[02468][048]|[13579][26]))|30(?!-02)|31(?=-0[13578]|-1[02]))-(0[1-9]|1[0-2])-([12]\d{3}) ([01]\d|2[0-3]):([0-5]\d):([0-5]\d).(\d{3})$";

        public void ParseFile()
        {
            const string filePath = ""; 
            var r = new StreamReaderWrapper(filePath);
            PacketDict = ParsePackets(r);
        }

        public Dictionary<Guid, Packet> ParsePackets(IStreamReader r)
        {
            var line = "";
            r.ReadLine();
            var strPortNumber = r.ReadLine();
            var portNumber = int.Parse(strPortNumber);
            r.ReadLine();
            while ((line = r.ReadLine()) != null && r.Peek() > -1)
            {
                var packetId = Guid.NewGuid();
                var packet = new Packet {PortNumber = portNumber, PacketId = packetId};
                if (Regex.IsMatch(line, DateTimeRegex))
                {
                    packet.DateRecieved = ParseDateTime(line);
                }

                var packetType = r.ReadLine();
                if (IsPType(packetType))
                {
                    packet.Cargo = r.ReadLine().Split(' ');
                    var endingState = r.ReadLine();
                    packet.IsError = string.CompareOrdinal(endingState, "EOP") != 0;
                }
                else
                {
                    packet.IsError = true;
                    r.ReadLine();
                }
                PacketDict.Add(packetId, packet);
                r.ReadLine();
            }
            return PacketDict;
        }

        protected static bool IsPType(string packetType)
        {
            return string.CompareOrdinal(packetType, "P") == 0;
        }

        public byte[] ParseCargo(string line)
        {
            return new byte[1];
        }

        public DateTime ParseDateTime(string stringDateTime)
        {
            return DateTime.ParseExact(stringDateTime, "dd-MM-yyyy HH:mm:ss.fff", null);
        }

    }
}