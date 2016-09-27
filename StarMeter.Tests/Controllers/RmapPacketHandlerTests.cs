using System.Collections;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StarMeter.Controllers;
using StarMeter.Models;

namespace StarMeter.Tests.Controllers
{
    [TestClass]
    public class RmapPacketHandlerTests
    {
        [TestMethod]
        public void CreateRmapPacketRangeException()
        {
            byte[] data = {0x00};
            var packet = new Packet
            {
                FullPacket = data,
            };

            var result = RmapPacketHandler.CreateRmapPacket(packet);

            Assert.IsTrue(result.IsError);
            Assert.AreEqual(result.ErrorType, ErrorType.DataError);
        }

        [TestMethod]
        public void GetDestinationKeyFromRmap()
        {
            byte[] packetData =
            {
                0x2d, 0x01, 0x0c, 0x00, 0x57, 0xff, 0xfb, 0x00, 0x00, 0x00, 0x08, 0x2f, 0xf3, 0xe3, 0x58, 0x99, 0xaa, 0xef, 0xe5, 0x20, 0x24
            };

            var packet = new RmapPacket
            {
                FullPacket = packetData
            };

            var actual = RmapPacketHandler.GetDestinationKey(packet);
            const byte expected = 0x00;

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void GetTransactionIdentifier()
        {
            byte[] packetData =
            {
                0x03, 0x02, 0xfe, 0x01, 0x0d, 0x00, 0xfe, 0x00, 0x05, 0x00, 0x00, 0x00, 0x04, 0xe7, 0x09, 0xb0, 0x1c, 0xe3, 0xb3
            };
            var packet = new RmapPacket
            {
                FullPacket = packetData
                
            };

            const int expected = 5;
            var actual = RmapPacketHandler.GetTransactionIdentifier(packet, 2);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestCheckRmapCrcTwoErrors()
        {
            byte[] packetData =
            {
                0x2d, 0x01, 0x0c, 0x00, 0x57, 0xff, 0xfb, 0x00, 0x00, 0x00, 0x08, 0x2f, 0xf3, 0xe3, 0x58, 0x99, 0xaa, 0xef, 0xe5, 0x20, 0x24
            };

            var packet = new RmapPacket
            {
                PacketType = "Read Reply",
                FullPacket = packetData,
            };

            packet.Cargo = PacketHandler.GetCargoArray(packet);

            Assert.IsFalse(RmapPacketHandler.CheckRmapCrc(packet));
        }

        [TestMethod]
        public void TestCheckRmapCrcCargoError()
        {
            byte[] packetData =
            {
                0x2d, 0x01, 0x0c, 0x00, 0x57, 0xff, 0xfb, 0x00, 0x00, 0x00, 0x08, 0x2e, 0xf3, 0xe3, 0x58, 0x99, 0xaa, 0xef, 0xe5, 0x20, 0x24
            };

            var packet = new RmapPacket
            {
                PacketType = "Read Reply",
                ProtocolId = 1,
                FullPacket = packetData,
            };
            packet.Cargo = PacketHandler.GetCargoArray(packet);

            Assert.IsFalse(RmapPacketHandler.CheckRmapCrc(packet));
        }

        [TestMethod]
        public void TestCheckRmapCrc()
        {
            byte[] packetData =
            {
                0x57, 0x01, 0x4c, 0x20, 0x2d, 0xff, 0xfb, 0x00, 0x00, 0x02, 0x00, 0x00, 0x00, 0x00, 0x08, 0x3e
            };

            var packet = new RmapPacket
            {
                PacketType = "Read",
                FullPacket = packetData,
            };
            packet.Cargo = PacketHandler.GetCargoArray(packet);

            Assert.IsTrue(RmapPacketHandler.CheckRmapCrc(packet));
        }

        [TestMethod]
        public void TestCheckRmapCrcValid()
        {
            byte[] packetData =
            {
                0x2d, 0x01, 0x0c, 0x00, 0x57, 0xff, 0xfb, 0x00, 0x00, 0x00, 0x08, 0x2e, 0xf3, 0xe3, 0x58, 0x99, 0xaa, 0xef, 0xe5, 0x20, 0x25
            };

            var packet = new RmapPacket
            {
                ProtocolId = 1,
                PacketType = "Read Reply",
                FullPacket = packetData,
            };
            packet.Cargo = PacketHandler.GetCargoArray(packet);

            Assert.IsTrue(RmapPacketHandler.CheckRmapCrc(packet));
        }

        [TestMethod]
        public void TestCheckRmapCrcHeaderError()
        {
            byte[] packetData =
            {
                0x2d, 0x01, 0x0c, 0x00, 0x57, 0xff, 0xfb, 0x00, 0x00, 0x00, 0x08, 0x2f, 0xf3, 0xe3, 0x58, 0x99, 0xaa, 0xef, 0xe5, 0x20, 0x25
            };

            var packet = new RmapPacket()
            {
                PacketType = "Read Reply",
                FullPacket = packetData,
            };
            packet.Cargo = PacketHandler.GetCargoArray(packet);

            Assert.IsFalse(RmapPacketHandler.CheckRmapCrc(packet));
        }

        [TestMethod]
        public void GetRmapTypeWrite()
        {
            const string expectedValue = "Write";
            var actual = RmapPacketHandler.GetRmapType(new BitArray(new[] { 0x6c }));
            Assert.AreEqual(expectedValue, actual);
        }

        [TestMethod]
        public void GetRmapTypeWriteReply()
        {
            const string expectedValue = "Write Reply";
            var actual = RmapPacketHandler.GetRmapType(new BitArray(new[] { 0x2c }));
            Assert.AreEqual(expectedValue, actual);
        }

        [TestMethod]
        public void GetRmapTypeRead()
        {
            const string expectedValue = "Read";
            var actual = RmapPacketHandler.GetRmapType(new BitArray(new[] { 0x4c }));
            Assert.AreEqual(expectedValue, actual);
        }

        [TestMethod]
        public void GetRmapTypeReadReply()
        {
            const string expectedValue = "Read Reply";
            var actual = RmapPacketHandler.GetRmapType(new BitArray(new[] { 0x0c }));
            Assert.AreEqual(expectedValue, actual);
        }

        [TestMethod]
        public void GetRmapTypeReadModifyWrite()
        {
            const string expectedValue = "Read Modify Write";
            var actual = RmapPacketHandler.GetRmapType(new BitArray(new[] { 0x5c }));
            Assert.AreEqual(expectedValue, actual);
        }

        [TestMethod]
        public void GetSourceAddressRmapException()
        {
            var data = new byte[0];
            var packet = new Packet
            {
                FullPacket = data
            };
            var result = RmapPacketHandler.GetSecondaryAddressRmap(packet);
            CollectionAssert.AreEqual(data, result);
        }
    }
}
