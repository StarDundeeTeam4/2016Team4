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
        private readonly byte[] _exampleCargo =
        {
            0xfa, 0x53, 0x2d, 0xe5, 0x81, 0xd1, 0x27, 0x41, 0xd5, 0xe5, 0xfe, 0xc6, 0x67, 0x05, 0x54,
            0xdd, 0x12, 0x75, 0xf0, 0x86, 0xe4, 0xdd, 0x6c, 0x3f, 0x71, 0x49, 0x2d, 0x29, 0x6c, 0x73,
            0x99, 0x66, 0x78, 0x45, 0x83, 0xc5, 0x3b, 0x9a, 0xea, 0xa1, 0xb4, 0x45, 0xe4, 0x06, 0xcf,
            0x54, 0xd5, 0x16, 0x37, 0x96, 0xe4, 0xab, 0x6c, 0x5a, 0xb0, 0x3e
        };

        private Parser _parser;

        [TestInitialize]
        public void Initialize()
        {
            _parser = new Parser();
        }

        [TestMethod]
        public void GetRmapPacketFromParserWhenRmapProtocolUsed()
        {
            var parser = new Parser();
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

            parser.ParsePackets(readerMock.Object);

            var expectedValue = typeof(RmapPacket);
            var result = parser.PacketDict.Values.FirstOrDefault().GetType();
            Assert.AreEqual(expectedValue, result);
        }

        [TestMethod]
        public void GetRmapPacketWithTypeSet()
        {
            var parser = new Parser();
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

            parser.ParsePackets(readerMock.Object);

            var expectedValue = new RmapPacket()
            {
                PacketId = Guid.NewGuid(),
                PortNumber = 1,
                Address = new byte[]{33},
                PacketType = "Read Reply"
            };
            var result = parser.PacketDict.Values.FirstOrDefault();
            Assert.AreEqual(expectedValue.PacketType, ((RmapPacket)result).PacketType);
        }

        [TestMethod]
        public void GetSourcePathAddressRmapFromParser()
        {
            var parser = new Parser();
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

            parser.ParsePackets(readerMock.Object);

            var expectedValue = new RmapPacket()
            {
                PacketId = Guid.NewGuid(),
                PortNumber = 1,
                Address = new byte[] { 33 },
                PacketType = "Read",
                SourcePathAddress = new byte[]{ 0x00, 0x00, 0x03, 0x02 }
            };
            var result = parser.PacketDict.Values.FirstOrDefault();
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
