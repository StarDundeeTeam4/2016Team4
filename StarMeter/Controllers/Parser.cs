using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using StarMeter.Models;

namespace StarMeter.Controllers
{
    public class Parser
    {
        public void ParseFile()
        {
            //set up string buffer/etc.
            foreach (Packet packet in ParsePacket())
            {
                //add packet to data structure
            }
        }

        private static IEnumerable<Packet> ParsePacket()
        {
            yield return new Packet();
        }

        public DateTime ParseDateTime(string stringDateTime)
        {
            
            var pattern = @"(\d\d)-(\d\d)-(\d+)\s(\d\d):(\d\d):(\d\d).(\d+)";
            var input = stringDateTime;
            var matches = Regex.Matches(input, pattern);
            var year  =  int.Parse(matches[0].Value);
            var month = int.Parse(matches[1].Value);
            var day = int.Parse(matches[2].Value);
            var hour = int.Parse(matches[3].Value);
            var minute = int.Parse(matches[4].Value);
            var second = int.Parse(matches[5].Value);
            var milliSec = int.Parse(matches[6].Value);

            var dateTime = new DateTime(year, month, day, hour, minute, second, milliSec);

            return dateTime;
        }
    }
}