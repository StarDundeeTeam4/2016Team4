using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Documents;
using StarMeter.Models;

namespace StarMeter.Controllers
{
    public class Parser
    {
        public Dictionary<Guid, Packet> packetDict = new Dictionary<Guid, Packet>();

        private const string DateTimeRegex = @"^(0[1-9]|1\d|2[0-8]|29(?=-\d\d-(?!1[01345789]00|2[1235679]00)\d\d(?:[02468][048]|[13579][26]))|30(?!-02)|31(?=-0[13578]|-1[02]))-(0[1-9]|1[0-2])-([12]\d{3}) ([01]\d|2[0-3]):([0-5]\d):([0-5]\d).(\d{3})$";

        public void ParseFile()
        {
            String filePath = ""; 
            StreamReader r = new StreamReader(filePath);
            packetDict = ParsePackets(r);
        }

        public Dictionary<Guid, Packet> ParsePackets(StreamReader r)
        {
            var line = "";
            r.ReadLine();
            var portNumber = int.Parse(r.ReadLine());
            while ((line = r.ReadLine()) != null && r.Peek() > -1)
            {
                var packetId = Guid.NewGuid();
                var packet = new Packet {PortNumber = portNumber, PacketID = packetId};
                if (Regex.IsMatch(line, DateTimeRegex))
                {
                    packet.DateRecieved = ParseDateTime(line);
                }


                var packetType = r.ReadLine();
                if (string.Compare(packetType, "P") == 0)
                {
                    packet.Cargo = r.ReadLine().Split(' ');
                    var endingState = r.ReadLine();
                    packet.IsError = string.Compare(endingState, "EOP") != 0;
                }
                else
                {
                    packet.IsError = true;
                    r.ReadLine();
                }
                packetDict.Add(packetId, packet);
                r.ReadLine();
            }
            return packetDict;
        }

        public byte[] ParseCargo(string line)
        {
            return new byte[1];
        }

        public DateTime ParseDateTime(string stringDateTime)
        {
            return DateTime.ParseExact(stringDateTime, "dd-MM-yyyy HH:mm:ss.fff", null);
        }

        public string GetPacketType(string inputLine)
        {
            var packetType = char.IsDigit(inputLine[0]) 
                ? "port number" 
                : "packet";
            return packetType;
        }
    }
}