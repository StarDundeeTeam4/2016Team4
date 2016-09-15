using System;

namespace StarMeter.Models
{
    public class Packet
    {
        public Guid     PacketId     { get; set; }
        public string   TypeOfPacket { get; private set; }
        public string[]   Cargo        { get; set; }
        public string[]   Address      { get; set; }
        public DateTime DateRecieved { get; set; }
        public bool     IsError      { get; set; }
        public string   ErrorType    { get; private set; }
        public int      PortNumber   { get; set; }
        public Guid     PrevPacket   { get; set; }
        public Guid     NextPacket   { get; set; }

        public Packet()
        {

        }

        public Packet(string type, string[] cargo, string[] address, DateTime date, int port)
        {
            PacketId = new Guid();
            TypeOfPacket = type;
            Cargo = cargo;
            Address = address;
            DateRecieved = date;
            PortNumber = port;
        }

        public Packet(string type, string[] cargo, string[] address, DateTime date, int port, string errorType)
        {
            PacketId = new Guid();
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
