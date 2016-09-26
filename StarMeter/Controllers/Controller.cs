using System;
using System.Collections.Generic;
using System.Linq;
using StarMeter.Models;

namespace StarMeter.Controllers
{
    public class Controller
    {
        public readonly List<string> FilePaths = new List<string>();
        public readonly Dictionary<Guid, Packet> Packets = new Dictionary<Guid,Packet>();

        /// <summary>
        /// Try and find the packet from the provided Guid, null if not found
        /// </summary>
        /// <param name="guid">The packetID to locate</param>
        /// <returns>The packet if found, null if not</returns>
        public Packet FindPacket(Guid guid)
        {
            try
            {
                return Packets[guid];
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
            var filesAdded = new List<string>();       

            foreach (var fileName in newFileNames)
            {
                if (!FilePaths.Contains(fileName))
                {
                    FilePaths.Add(fileName);
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
            return FilePaths.Select(filePath => filePath.Split('\\').Last()).ToArray();
        }

        /// <summary>
        /// Remove a file path by fileName from the path list
        /// </summary>
        /// <param name="fileName">The file NAME (not path) to remove</param>
        /// <returns>The index of the removed file in the list</returns>
        public int RemoveFile(string fileName)
        {
            var index = FilePaths.FindIndex(x => x.EndsWith(fileName));
            if (index >= 0)
            {
                FilePaths.RemoveAt(index);
            }
            return index;
        }

        /// <summary>
        /// Sends each file in filePaths to the Parser, and adds all results to packets
        /// </summary>
        /// <returns>An array of all added packets</returns>
        public Packet[] ParsePackets() 
        {
            Packets.Clear();
            var parser = new Parser();
            foreach (var file in FilePaths) 
            {
                var packetDict = (parser.ParseFile(file));
                foreach (var packet in packetDict)
                {
                    Packets.Add(packet.Key, packet.Value);
                }
            }
            return Packets.Values.ToArray();
        }
    }
}
