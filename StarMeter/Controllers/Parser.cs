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
        private int portNumber;

        public void ParseFile()
        {
            //set up string buffer/etc.
            StreamReader r = new StreamReader("");
            foreach (Packet packet in ParsePacket(r))
            {
                //add packet to data structure
            }
        }

        protected IEnumerable<Packet> ParsePacket(TextReader r)
        {
            restart:
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
                    char type = line[0];
                }
            }

            //parse cargo
            line = r.ReadLine();
            ParseCargo(line);

            //is end of packet?
            line = r.ReadLine();
            if (line == "EOP")
            {
                //create & return packet
                yield return new Packet(
                    );
                    //type,
                    //cargo,
                    //address,
                    //date,
                    //portNumber
                    //);
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