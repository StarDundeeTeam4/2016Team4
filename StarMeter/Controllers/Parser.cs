using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StarMeter.Models;

namespace StarMeter.Controllers
{
    public class Parser
    {
        public void ParseFile()
        {
            //set up string buffer/etc.
            StreamReader r = new StreamReader("");
            foreach (Packet packet in ParsePacket(r))
            {
                //add packet to data structure
            }
        }

        private IEnumerable<Packet> ParsePacket(TextReader r)
        {
            //Parse DateTime
            string line = r.ReadLine();
            if (line == null) //ensure file exists
            {
                yield break;
            }
            else
            {
                DateTime date = ParseDateTime(line);
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
                char type = line[0];
                if (char.IsDigit(type))
                {
                    //then is port number
                    //set "global" port number variable
                }
                else
                {
                    //packet type is var type
                }
            }

            //parse cargo
            line = r.ReadLine();
            ParseCargo(line);

            //is end of packet?
            line = r.ReadLine();
            if (line == "EOP")
            {
                Packet packet = new Packet
                {
                    //set packet data here
                };
                yield return packet;
            }
            else
            {
                //is an EEP/Disconnect packet
                //deal with that here
            }
        }

        public DateTime ParseDateTime(string stringDateTime)
        {
            return new DateTime(); //placeholder
        }

        public byte[] ParseCargo(string str)
        {
            return new byte[1]; //placeholder
        }
    }
}