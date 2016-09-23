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

        [TestCleanup]
        public void Cleanup()
        {
            _parser = null;
        }
    }
}
