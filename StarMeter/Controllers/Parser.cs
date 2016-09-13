using System;
using System.IO;
using System.Collections.Generic;
using StarMeter.Models;

namespace StarMeter.Controllers
{
    public class Parser
    {
        private int portNumber;

        public void ParseFile()
        {
            //set up string buffer/etc.
            var r = new StreamReader("");
            foreach (Packet packet in ParsePacket(r))
            {
                //add packet to data structure
            }
        }

        protected IEnumerable<Packet> ParsePacket(TextReader r)
        {
            DateTime date;
            byte[] address;
            byte[] cargo;
            string type;

            restart:
            //Parse DateTime
            string line = r.ReadLine();
            if (line == null) //ensure file exists
            {
                yield break;
            }
            else
            {
                date = ParseDateTime(line);
            }

            //Check if current section is end section. i.e. is only datetime then EOF
            line = r.ReadLine();
            if (line == null)
            {
                yield break;
            }
            else
            {
                //identify packet type
                if (char.IsDigit(line[0]))
                {
                    //then is port number
                    portNumber = int.Parse(line);
                    r.ReadLine();
                    goto restart;
                }
                else
                {
                    //packet type is var type
                    type = line;
                }
            }

            if (type == "P")
            {
                //parse cargo
                line = r.ReadLine();
                ParseCargo(line);
            }
            else
            {
                //no cargo in error packets
                goto endOfPacket;
            }

            endOfPacket:
            //PLACEHOLDER
            cargo = new byte[1];
            address = new byte[1];
            //PLACEHOLDER

            //is end of packet?
            line = r.ReadLine();
            if (line == "EOP") //EOP, EEP, None, Disconnect?
            {
                //create & return packet
                yield return new Packet(
                    type,
                    cargo,
                    address,
                    date,
                    portNumber
                    );
            }
            else
            {
                //is an EEP/None/Disconnect packet
                //a None->Disconnect on link A will be followed by an EEP->Disconnect on link B
                //deal with that here
            }
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