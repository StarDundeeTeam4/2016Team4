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
        public Guid     PacketID     { get; private set; }
        public string   TypeOfPacket { get; private set; }
        public byte[]   Cargo        { get; private set; }
        public byte[]   Address      { get; private set; }
        public DateTime DateRecieved { get; private set; }
        public bool     IsError      { get; private set; }
        public string   ErrorType    { get; private set; }
        public int      PortNumber   { get; private set; }
        public Guid     PrevPacket   { get; set; }
        public Guid     NextPacket   { get; set; }

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
