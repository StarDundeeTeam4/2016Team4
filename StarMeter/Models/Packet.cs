using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace StarMeter.Models
{
    public class Packet
    {
        public Guid     PacketID     { get; }
        public string   TypeOfPacket { get; }
        public byte[]   Cargo        { get; }
        public byte[]   Address      { get; }
        public DateTime DateRecieved { get; }
        public bool     IsError      { get; }
        public string   ErrorType    { get; }
        public int      PortNumber   { get; }
        public Guid     PrevPacket   { get; set; }
        public Guid     NextPacket   { get; set; }

        public Packet()
        {
            
        }

        public Packet(string type, byte[] cargo, byte[] address, DateTime date, int port)
        {
            PacketID = new Guid();
            TypeOfPacket = type;
            Cargo = cargo;
            Address = address;
            DateRecieved = date;
            PortNumber = port;
        }

        public Packet(string type, byte[] cargo, byte[] address, DateTime date, int port, string errorType)
        {
            PacketID = new Guid();
            TypeOfPacket = type;
            Cargo = cargo;
            Address = address;
            DateRecieved = date;
            PortNumber = port;
            IsError = true;
            ErrorType = errorType;
        }
    }
}
