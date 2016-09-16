using System;
using System.Collections.Generic;
using System.Linq;
using StarMeter.Models;

namespace StarMeter.Controllers
{
    public class Controller
    {

        public readonly List<string> filePaths = new List<string>();
        public List<Packet> packets = new List<Packet>();

        public Packet FindPacket(Guid guid)
        {
            foreach (var p in packets)
            {
                if (guid.Equals(p.PacketId))
                {
                    return p;
                }
            }

            return null;
        }
        
        public void AddFileNames(string[] newFileNames)
        {
            foreach (string fileName in newFileNames)
            {
                if (!filePaths.Contains(fileName))
                {
                    filePaths.Add(fileName);
                }
            }
        }

        public string[] GetFileNames()
        {
            return filePaths.Select(filePath => filePath.Split('\\').Last()).ToArray();
        }

        public int RemoveFile(string fileName)
        {
            int index = filePaths.FindIndex(x => x.EndsWith(fileName));
            if (index >= 0)
            {
                filePaths.RemoveAt(index);
            }
            return index;
        }

        public Packet[] ParsePackets() 
        {
            Parser parser = new Parser();
                        
            foreach (var file in filePaths) 
            {
                var packetDict = (parser.ParseFile(file));
                packets.AddRange(packetDict.Values);
            }

            return packets.ToArray();
        }
            
    }
}
