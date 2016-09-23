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

        /// <summary>
        /// Try and find the packet from the provided Guid, null if not found
        /// </summary>
        /// <param name="guid">The packetID to locate</param>
        /// <returns>The packet if found, null if not</returns>
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
        
        /// <summary>
        /// Add a list of file names to the current file name list, removing duplicates
        /// </summary>
        /// <param name="newFileNames">The new files to add</param>
        /// <returns>The new list of filenames</returns>
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

        /// <summary>
        /// Returns a list of file names (not file paths)
        /// Example: filePaths = C:\file1.txt => returns file1.txt
        /// </summary>
        /// <returns>An array of the filenames</returns>
        public string[] GetFileNames()
        {
            return filePaths.Select(filePath => filePath.Split('\\').Last()).ToArray();
        }

        /// <summary>
        /// Remove a file path by fileName from the path list
        /// </summary>
        /// <param name="fileName">The file NAME (not path) to remove</param>
        /// <returns>The index of the removed file in the list</returns>
        public int RemoveFile(string fileName)
        {
            int index = filePaths.FindIndex(x => x.EndsWith(fileName));
            if (index >= 0)
            {
                filePaths.RemoveAt(index);
            }
            return index;
        }

        /// <summary>
        /// Sends each file in filePaths to the Parser, and adds all results to packets
        /// </summary>
        /// <returns>An array of all added packets</returns>
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
