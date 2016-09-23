using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StarMeter.Controllers;
using StarMeter.Models;
using Moq;
using StarMeter.Interfaces;


namespace StarMeter.Tests.Controllers
{
    [TestClass]
    public class ParserTests
    {
        private Parser _parser;

        [TestInitialize]
        public void Initialize()
        {
            _parser = new Parser();
        }

        [TestMethod]
        public void TestParsePacketsErrorPacket()
        {
            var readerMock = new Mock<IStreamReader>();

            var stockResponses = new Queue<string>();
            stockResponses.Enqueue("08-09-2016 23:59:27.036");
            stockResponses.Enqueue("4");
            stockResponses.Enqueue("");
            stockResponses.Enqueue("09-09-2016 00:01:11.031");
            stockResponses.Enqueue("P");
            stockResponses.Enqueue("02 fe 01 0d 00 fe 00 09 00 00 00 04 ab 6b d9 40 e5 55");
            stockResponses.Enqueue("EOP");
            stockResponses.Enqueue("");
            stockResponses.Enqueue("09-09-2016 00:01:12.026");
            stockResponses.Enqueue("P");
            stockResponses.Enqueue("02 fe 01 0d 00 fe 00 0a 00 00 00 04 51 3b d3 22 9c 27");
            stockResponses.Enqueue("EOP");
            stockResponses.Enqueue("");
            stockResponses.Enqueue("09-09-2016 00:01:12.030");
            stockResponses.Enqueue("P");
            stockResponses.Enqueue("02 fe 01 0d 00 fe 00");
            stockResponses.Enqueue("None");
            stockResponses.Enqueue("");
            stockResponses.Enqueue("09-09-2016 00:01:12.032");
            stockResponses.Enqueue("E");
            stockResponses.Enqueue("Disconnect");
            stockResponses.Enqueue("");
            stockResponses.Enqueue("09-09-2016 00:51:15.176");
            stockResponses.Enqueue(null);
            readerMock.Setup(t => t.ReadLine()).Returns(stockResponses.Dequeue);

            readerMock.SetupSequence(t => t.Peek()).Returns(1);

            _parser.ParsePackets(readerMock.Object);

            Guid id = _parser._prevPacket.GetValueOrDefault();
            Assert.IsTrue(_parser.PacketDict[id].IsError);
        }

        [TestMethod]
        public void GetRmapPacketFromParserWhenRmapProtocolUsed()
        {
            var readerMock = new Mock<IStreamReader>();

            var stockResponses = new Queue<string>();
            stockResponses.Enqueue("08-09-2016 15:11:04.045");
            stockResponses.Enqueue("1");
            stockResponses.Enqueue("");
            stockResponses.Enqueue("08-09-2016 15:12:50.081");
            stockResponses.Enqueue("P");
            stockResponses.Enqueue(@"2d 01 0c 00 57 ff fb 00 00 00 08 2e f3 e3 58 99 aa ef e5 20 25");
            stockResponses.Enqueue("EOP");
            stockResponses.Enqueue("");
            stockResponses.Enqueue("08-09-2016 15:13:55.193");
            stockResponses.Enqueue("E");
            stockResponses.Enqueue("Disconnect");
            stockResponses.Enqueue("");
            stockResponses.Enqueue("08-09-2016 15:13:56.193");
            stockResponses.Enqueue(null);
            readerMock.Setup(t => t.ReadLine()).Returns(stockResponses.Dequeue);
                
            readerMock.SetupSequence(t => t.Peek()).Returns(5).Returns(4).Returns(-1);

            _parser.ParsePackets(readerMock.Object);

            var expectedValue = typeof(RmapPacket);
            var result = _parser.PacketDict.Values.FirstOrDefault().GetType();
            Assert.AreEqual(expectedValue, result);
        }

        [TestMethod]
        public void GetRmapPacketWithTypeSet()
        {
            var readerMock = new Mock<IStreamReader>();

            var stockResponses = new Queue<string>();
            stockResponses.Enqueue("08-09-2016 18:45:04.045");
            stockResponses.Enqueue("1");
            stockResponses.Enqueue("");
            stockResponses.Enqueue("08-09-2016 15:12:50.081");
            stockResponses.Enqueue("P");
            stockResponses.Enqueue(@"2d 01 0c 00 57 ff fb 00 00 00 08 2e f3 e3 58 99 aa ef e5 20 25");
            stockResponses.Enqueue("EOP");
            stockResponses.Enqueue("");
            stockResponses.Enqueue("08-09-2016 15:13:55.193");
            stockResponses.Enqueue("E");
            stockResponses.Enqueue("Disconnect");
            stockResponses.Enqueue("");
            stockResponses.Enqueue("08-09-2016 15:13:56.193");
            stockResponses.Enqueue(null);
            readerMock.Setup(t => t.ReadLine()).Returns(stockResponses.Dequeue);

            readerMock.SetupSequence(t => t.Peek()).Returns(5).Returns(4).Returns(-1);

            _parser.ParsePackets(readerMock.Object);

            var expectedValue = new RmapPacket()
            {
                PacketId = Guid.NewGuid(),
                PortNumber = 1,
                Address = new byte[]{33},
                PacketType = "Read Reply"
            };
            var result = _parser.PacketDict.Values.FirstOrDefault();
            Assert.AreEqual(expectedValue.PacketType, ((RmapPacket)result).PacketType);
        }

        [TestMethod]
        public void GetSourcePathAddressRmapFromParser()
        {
            var readerMock = new Mock<IStreamReader>();

            var stockResponses = new Queue<string>();
            stockResponses.Enqueue("08-09-2016 18:45:04.045");
            stockResponses.Enqueue("1");
            stockResponses.Enqueue("");
            stockResponses.Enqueue("08-09-2016 15:12:50.081");
            stockResponses.Enqueue("P");
            stockResponses.Enqueue(@"01 01 00 fe 01 4d 20 00 00 03 02 fe 00 00 00 00 00 01 00 00 00 04 dc");
            stockResponses.Enqueue("EOP");
            stockResponses.Enqueue("");
            stockResponses.Enqueue("08-09-2016 15:13:55.193");
            stockResponses.Enqueue("E");
            stockResponses.Enqueue("Disconnect");
            stockResponses.Enqueue("");
            stockResponses.Enqueue("08-09-2016 15:13:56.193");
            stockResponses.Enqueue(null);
            readerMock.Setup(t => t.ReadLine()).Returns(stockResponses.Dequeue);

            readerMock.SetupSequence(t => t.Peek()).Returns(5).Returns(4).Returns(-1);

            _parser.ParsePackets(readerMock.Object);

            var expectedValue = new RmapPacket()
            {
                PacketId = Guid.NewGuid(),
                PortNumber = 1,
                Address = new byte[] { 33 },
                PacketType = "Read",
                SourcePathAddress = new byte[]{ 0x00, 0x00, 0x03, 0x02 }
            };
            var result = _parser.PacketDict.Values.FirstOrDefault();
            Assert.AreEqual(expectedValue.SourcePathAddress.Length, ((RmapPacket)result).SourcePathAddress.Length);
            Assert.AreEqual(expectedValue.SourcePathAddress[0], ((RmapPacket)result).SourcePathAddress[0]);
        }

        [TestMethod]
        public void TestSetPrevPacket()
        {
            Packet p1 = new Packet()
            {
                PacketId = Guid.NewGuid()
            };
            Packet p2 = new Packet()
            {
                PacketId = Guid.NewGuid()
            };
            _parser.PacketDict.Add(p1.PacketId, p1);
            _parser.PacketDict.Add(p2.PacketId, p2);

            _parser._prevPacket = p1.PacketId;

            Assert.AreEqual(_parser._prevPacket, p1.PacketId);
            _parser.SetPrevPacket(p2);
            Assert.AreEqual(_parser._prevPacket, p2.PacketId);
        }

        [TestMethod]
        public void TestGetPrevPacket()
        {
            Packet prev = new Packet();

            _parser.PacketDict.Add(prev.PacketId, prev);

            Packet p = new Packet()
            {
                PrevPacket = prev.PacketId,
            };

            Packet result = _parser.GetPrevPacket(p);
            Assert.AreEqual(prev, result);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _parser = null;
        }
    }
}
