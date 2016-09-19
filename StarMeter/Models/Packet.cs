using System;

public enum ErrorTypes
{
    None,
    DataError,
    Disconnect,
    SequenceError,
    Timeout,
};

namespace StarMeter.Models
{
    public class Packet
    {
        public Guid       PacketId     { get; set; }
        public byte[]     Cargo        { get; set; }
        public byte[]     Address      { get; set; }
        public DateTime   DateRecieved { get; set; }
        public bool       IsError      { get; set; }
        public ErrorTypes ErrorType    { get; set; }
        public int        SequenceNum  { get; set; }
        public int        PortNumber   { get; set; }
        public Guid?      PrevPacket   { get; set; }
        public Guid?      NextPacket   { get; set; }
        public ushort     Crc          { get; set; }
        public int        ProtocolId   { get; set; }
        public byte[]     FullPacket   { get; set; }

        public Packet()
        {

        }

        public Packet(string type, byte[] cargo, byte[] address, DateTime date, int port)
        {
            PacketId = Guid.NewGuid();
            Cargo = cargo;
            Address = address;
            DateRecieved = date;
            PortNumber = port;
        }
    }
}
