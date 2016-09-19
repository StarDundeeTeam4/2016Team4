using System;
using System.Collections.Generic;
using System.Linq;
using StarMeter.Models;

namespace StarMeter.Controllers
{
    public class Controller
    {

        public readonly List<string> filePaths = new List<string>();
        public readonly Dictionary<Guid, Packet> packets = new Dictionary<Guid,Packet>();

        public Packet FindPacket(Guid guid)
        {
            try
            {
                return packets[guid];
            }
            catch
            {
                return null;
            }
        }
        
        public List<string> AddFileNames(string[] newFileNames)
        {
            List<string> filesAdded = new List<string>();       

            foreach (string fileName in newFileNames)
            {
                if (!filePaths.Contains(fileName))
                {
                    filePaths.Add(fileName);
                    filesAdded.Add(fileName.Split('\\').Last());
                }
            }

            return filesAdded;

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

            packets.Clear();

            Parser parser = new Parser();
                        
            foreach (var file in filePaths) 
            {
                var packetDict = (parser.ParseFile(file));
                foreach (var p in packetDict)
                {
                    packets.Add(p.Key, p.Value);
                }
            }

            return packets.Values.ToArray();
        }
            
    }
}
