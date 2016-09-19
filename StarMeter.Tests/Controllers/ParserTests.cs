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

        [TestInitialize()]
        public void Initialize()
        {
            _parser = new Parser();
        }

        [TestMethod]
        public void PassingCorrectStringReturnsDateTimeTest()
        {
            const string stringDateTime = "08-09-2016 14:27:53.726";
            DateTime result;

            Assert.IsTrue(_parser.ParseDateTime(stringDateTime, out result));
            Assert.IsInstanceOfType(result, typeof(DateTime));
            Assert.AreEqual(result, new DateTime(2016, 09, 08, 14, 27, 53, 726));
        }

        [TestMethod]
        public void NoErrorOnePacketReturnsListWithOneElement()
        {
            var readerMock = new Mock<IStreamReader>();

            var stockResponses = new Queue<string>();
            stockResponses.Enqueue("08-09-2016 18:45:04.045");
            stockResponses.Enqueue("1");
            stockResponses.Enqueue("");
            stockResponses.Enqueue("08-09-2016 15:12:50.081");
            stockResponses.Enqueue("P");
            stockResponses.Enqueue(@"01 00 fe fa 53 2d e5 81 d1 27 41 d5 e5 fe c6 67 05 54 dd 12 75 f0 86 e4 dd 6c 3f 71 49 2d 29 6c 73 99 66 78 45 83 c5 3b 9a ea a1 b4 45 e4 06 cf 54 d5 16 37 96 e4 ab 6c 5a b0 3e");
            stockResponses.Enqueue("EOP");
            stockResponses.Enqueue("");
            stockResponses.Enqueue("08-09-2016 15:13:55.193");
            stockResponses.Enqueue(null);
            readerMock.Setup(t => t.ReadLine()).Returns(stockResponses.Dequeue);

            readerMock.SetupSequence(t => t.Peek()).Returns(5).Returns(-1);

            var result = _parser.ParsePackets(readerMock.Object);
            var expected = new Dictionary<Guid, Packet>();
            var packetId = Guid.NewGuid();
            var packet1 = new Packet
            {
                IsError = false,
                PacketId = packetId,
                DateRecieved = DateTime.ParseExact("08-09-2016 15:12:50.081", "dd-MM-yyyy HH:mm:ss.fff", null),
                Cargo = _exampleCargo
            };
            expected.Add(packetId, packet1);
           
            var comparativePacket = (from a in result.Values
                where a.DateRecieved == packet1.DateRecieved
                select a).First();


            Assert.AreEqual(expected[packetId].Cargo.Length, comparativePacket.Cargo.Length);
            Assert.AreEqual(expected[packetId].IsError, comparativePacket.IsError);
        }

        [TestMethod]
        public void PacketCollectionWithErrorParsed()
        {
            var readerMock = new Mock<IStreamReader>();

            var stockResponses = new Queue<string>();
            stockResponses.Enqueue("08-09-2016 18:45:04.045");
            stockResponses.Enqueue("1");
            stockResponses.Enqueue("");
            stockResponses.Enqueue("08-09-2016 15:12:50.081");
            stockResponses.Enqueue("P");
            stockResponses.Enqueue(@"00 fe fa 00 17 50 b8 f6 ca d3 9e 3c 52 74 51 9f ef 80 ba f6 75 92 de c3 aa 62 5f aa f0 de 46 28 24 7c ff 81 c5 ce a5 fa 59 57 81 49 0c 9d cd 4a 9b 7f bd f3 70 c9 c0 8a 0f 06 03 15 b0 95 36 13 2d ff 94 69 1f 88 1d 9f 44 04 26 4c 25 ec 14 cf f5 b1 65 40 bb 50 f0 a7 b4 27 6d 6b f2 07 37 0d 4a 8a 51 15 6d a7 a7 4d 55 83 97 2e e3 8a b0 98 c6 bf ba c6 9e 50 f6 80 61 6e a7 92 fe 5b d0 7e 41 c5 40 6e f7 52 cc 6c 52 7c dc d5 8f 9f 29 0b d5 50 c4 6b 61 f1 5b 7f e0 82 b8 74 1c ba 8a ce db 57 68 5a 04 b2 13 64 04 96 fb 2b 70 52 05 92 ec 0d 8c 18 4b 5a a6 0a f8 0d a8 f8 94 4c ec 65 e0 e9 d1 c2 de ef 04 9e 33 7a fe 17 d0 cc ce 94 d1 9e 19 b6 a5 b4 5f 8b 70 b4 7f 05 ad 38 7e ab 18 22 84 8f cb 30 27 80 a7 d0 ec 80 f5 35 0b 79 4d aa 73 2b b7 26 0e 69 11 21 46 85 b1 a7 c8");
            stockResponses.Enqueue("EOP");
            stockResponses.Enqueue("");
            stockResponses.Enqueue("08-09-2016 15:53:23.690");
            stockResponses.Enqueue("E");
            stockResponses.Enqueue("Disconnect");
            stockResponses.Enqueue("");
            stockResponses.Enqueue("08-09-2016 15:13:55.193");
            stockResponses.Enqueue(null);
            readerMock.Setup(t => t.ReadLine()).Returns(stockResponses.Dequeue);

            readerMock.SetupSequence(t => t.Peek()).Returns(5).Returns(5).Returns(-1);

            var result = _parser.ParsePackets(readerMock.Object);
            var expected = new Dictionary<Guid, Packet>();

            var packet1 = new Packet
            {
                IsError = false,
                DateRecieved = DateTime.ParseExact("08-09-2016 15:12:50.081", "dd-MM-yyyy HH:mm:ss.fff", null),
                Cargo = _exampleCargo,
            };
            var packet2 = new Packet
            {
                DateRecieved = DateTime.ParseExact("08-09-2016 15:53:23.690", "dd-MM-yyyy HH:mm:ss.fff", null),
                IsError = true
            };
            var packetId = Guid.NewGuid();
            expected.Add(packetId, packet1);
            packetId = Guid.NewGuid();
            expected.Add(packetId, packet2);
            var comparativePacket = (from a in result.Values
                                    where a.DateRecieved == packet2.DateRecieved
                                    select a).First();

            Assert.AreEqual(expected.Count, result.Count);
            Assert.AreEqual(expected[packetId].DateRecieved, comparativePacket.DateRecieved);
            Assert.AreEqual(expected[packetId].IsError, comparativePacket.IsError);
        }
         
        [TestMethod]
        public void NoneErrorParsesSuccesfully()
        {
            var readerMock = new Mock<IStreamReader>();

            var stockResponses = new Queue<string>();
            stockResponses.Enqueue("08-09-2016 18:45:04.045");
            stockResponses.Enqueue("1");
            stockResponses.Enqueue("");
            stockResponses.Enqueue("08-09-2016 15:12:55.051");
            stockResponses.Enqueue("P");
            stockResponses.Enqueue(@"01 00 fe fa 53 2d e5 81 d1 27 41 d5 e5 fe c6 67 05 54 dd 12 75 f0 86 e4 dd 6c 3f 71 49 2d 29 6c 73 99 66 78 45 83 c5 3b 9a ea a1 b4 45 e4 06 cf 54 d5 16 37 96 e4 ab 6c 5a b0 3e");
            stockResponses.Enqueue("None");
            stockResponses.Enqueue("");
            stockResponses.Enqueue("08-09-2016 15:13:55.193");
            stockResponses.Enqueue(null);
            readerMock.Setup(t => t.ReadLine()).Returns(stockResponses.Dequeue);

            readerMock.SetupSequence(t => t.Peek()).Returns(5).Returns(-1);

            var result = _parser.ParsePackets(readerMock.Object);

            var expected = new Dictionary<Guid, Packet>();
            var packet1 = new Packet
            {
                IsError = true,
                DateRecieved = DateTime.ParseExact("08-09-2016 15:12:55.051", "dd-MM-yyyy HH:mm:ss.fff", null),
                Cargo = _exampleCargo,
            };
           
            var packetId = Guid.NewGuid();
            expected.Add(packetId, packet1);

            var comparativePacket = (from a in result.Values
                                     where a.DateRecieved == packet1.DateRecieved
                                     select a).First();

            Assert.AreEqual(expected[packetId].Cargo.Length, comparativePacket.Cargo.Length);
            Assert.AreEqual(expected[packetId].Cargo[2], comparativePacket.Cargo[2]);
            Assert.AreEqual(expected[packetId].DateRecieved, comparativePacket.DateRecieved);
            Assert.AreEqual(expected[packetId].IsError, comparativePacket.IsError);
        }

        [TestMethod]
        public void PassingCargoWithAddressinFirstByteReturnsIndex()
        {
            var cargoParam =
                @"57 01 4c 20 2d ff fa 00 00 02 00 00 00 00 08 12".Split(' ');
            const int expected = 0;

            var actual = _parser.GetLogicalAddressIndex(cargoParam);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void PassingCargoWithAddressInThirdByteReturnsIndex()
        {
            var cargoParam =
                @"01 00 fe fa 4b e7 5d b0 07 bd 57 83 82 ac ac 66 79 2e 35 d7 3d db 80 41 5e 98 2d d1 29 0b 98 85 6f 52 b9 86 88 f4 48 b7 98 17 24 8e 11 f4 f5 23 79 89 10 86 6e 8d 9c 56 e7 a9 f3 8f bc e5 4b a4 69 61 96 ce 0f 45 a7 a3 bb fa 95 ea eb 9b 64 ae 4e a6 e2 91 cc f8 d0 63 c6 f8 42 f3 9f 12 49 25 7b 67 ce 45 46 c2 c1 9b f9 ce 74 40 18 87 41 30 7f ef 73 9f 5f 02 64 91 84 eb e2 a5 d9 78 d7 31 2d cc 29 94 37 54 e0 c8 d8 fc 87 37 a1 a2 d4 d0 95 ca 8b 59 30 94 9f 07 a8 ec 9a 4c 72 01 40 f0 08 f8 6f 63 ea e1 4a 52 e2 ea 97 78 8d d2 64 b7 17 a1 f1 67 46 97 ca ee ba 62 34 90 73 94 ca 89 93 53 68 59 65 52 48 60 9f 01 6b aa e4 01 3b 0c 8d db 6e 70 a9 f1 2d 6d 42 b5 76 1b e3 18 bf 25 56 46 dc 1f b2 90 22 1a 96 a9 cd 76 af 16 9f f8 80 e0 ca 1c 62 8c 10 ad c9 4c 29 92 ca 77 65 eb".Split(' ');
            const int expected = 2;

            var actual = _parser.GetLogicalAddressIndex(cargoParam);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void GetPhysicalPathPortIndexesFromAddress()
        {
            var cargoParam =
                @"00 04 fe fa 4b e7 5d b0 07 bd 57 83 82 ac ac 66 79 2e 35 d7 3d db 80 41 5e 98 2d d1 29 0b 98 85 6f 52 b9 86 88 f4 48 b7 98 17 24 8e 11 f4 f5 23 79 89 10 86 6e 8d 9c 56 e7 a9 f3 8f bc e5 4b a4 69 61 96 ce 0f 45 a7 a3 bb fa 95 ea eb 9b 64 ae 4e a6 e2 91 cc f8 d0 63 c6 f8 42 f3 9f 12 49 25 7b 67 ce 45 46 c2 c1 9b f9 ce 74 40 18 87 41 30 7f ef 73 9f 5f 02 64 91 84 eb e2 a5 d9 78 d7 31 2d cc 29 94 37 54 e0 c8 d8 fc 87 37 a1 a2 d4 d0 95 ca 8b 59 30 94 9f 07 a8 ec 9a 4c 72 01 40 f0 08 f8 6f 63 ea e1 4a 52 e2 ea 97 78 8d d2 64 b7 17 a1 f1 67 46 97 ca ee ba 62 34 90 73 94 ca 89 93 53 68 59 65 52 48 60 9f 01 6b aa e4 01 3b 0c 8d db 6e 70 a9 f1 2d 6d 42 b5 76 1b e3 18 bf 25 56 46 dc 1f b2 90 22 1a 96 a9 cd 76 af 16 9f f8 80 e0 ca 1c 62 8c 10 ad c9 4c 29 92 ca 77 65 eb".Split(' ');

            var logicalIndex = _parser.GetLogicalAddressIndex(cargoParam);
            var addressArray = _parser.GetAddressArray(cargoParam, logicalIndex);
            var expectedPathValues = new[]
            {
                (byte)Convert.ToInt32("00", 16),
                (byte)Convert.ToInt32("04", 16),
                (byte)Convert.ToInt32("fe", 16)
            };

            Assert.AreEqual(expectedPathValues[1], addressArray[1]);
        }

        [TestMethod]
        public void GetLogicalAddress()
        {
            var cargoParam =
                @"57 01 4c 20 2d ff fb 00 00 02 00 00 00 00 08 3e".Split(' ');
            var expectedPathValues = new[]
            {
                (byte)Convert.ToInt32("57", 16)
            };

            var logicalIndex = _parser.GetLogicalAddressIndex(cargoParam);
            var physicalPathValues = _parser.GetAddressArray(cargoParam, logicalIndex);

            Assert.AreEqual(expectedPathValues[0], physicalPathValues[0]);
        }

        [TestMethod]
        public void GetCrcFromCargo()
        {
            var cargoParam =
                @"57 01 4c 20 2d ff fb 00 00 02 00 00 00 00 08 3e".Split(' ');
            var expected = (byte)Convert.ToInt32("3e", 16);

            var actual = _parser.GetCrc(cargoParam);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void GetProtocolIdFromCargo()
        {
            var cargoParam =
                @"57 01 4c 20 2d ff fb 00 00 02 00 00 00 00 08 3e".Split(' ');
            const int expected = 1;

            var logicalIndex = _parser.GetLogicalAddressIndex(cargoParam);
            var actual = _parser.GetProtocolId(cargoParam, logicalIndex);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void GetSequenceNumberFromNonRmap()
        {
            byte[] _nonRmapPacket =
            {
                0x3f, 0xfa, 0x0f, 0x33, 0xd2, 0x6b, 0xef, 0x66, 0x29, 0xdb, 0x84, 0x3f, 0x1a, 0xd7, 0x68, 0x4a, 0x10,
                0xe9, 0x8e, 0x01, 0xb2, 0xf3, 0xe3, 0xed, 0x70, 0x71, 0x81, 0x0e, 0x5c, 0xfe, 0x25, 0x7d, 0x3c, 0x1a,
                0xe3, 0x50, 0xdd, 0x2a, 0x18, 0xbe, 0xaa, 0xc0, 0xa1, 0x84, 0x7c, 0x1d, 0x4a, 0x86, 0xb4, 0xf4, 0x89,
                0xba, 0x88, 0x71, 0x6d, 0x42, 0x9a, 0x7a, 0xd1, 0xd8, 0xcf, 0x35, 0xd5, 0x6f, 0x5e, 0xca, 0x8f, 0x17,
                0x27, 0x6a, 0xfd, 0x22, 0x47, 0x90, 0x3e, 0x85, 0xe9, 0x86, 0x02, 0x81, 0x5e, 0x2c, 0x40, 0x82, 0x71,
                0xf5, 0x27, 0x47, 0xf4, 0x32, 0x56, 0x43, 0x9f, 0x93, 0x4f, 0x43, 0x1b, 0xea, 0x29, 0x52
            };

            var cargoParam =
                @"3f fa 0f 33 d2 6b ef 66 29 db 84 3f 1a d7 68 4a 10 e9 8e 01 b2 f3 e3 ed 70 71 81 0e 5c fe 25 7d 3c 1a e3 50 dd 2a 18 be aa c0 a1 84 7c 1d 4a 86 b4 f4 89 ba 88 71 6d 42 9a 7a d1 d8 cf 35 d5 6f 5e ca 8f 17 27 6a fd 22 47 90 3e 85 e9 86 02 81 5e 2c 40 82 71 f5 27 47 f4 32 56 43 9f 93 4f 43 1b ea 29 52".Split(' ');
            Packet p = new Packet()
            {
                FullPacket = _nonRmapPacket
            };

            const int expectedSequenceNumber = 15;

            int logicalIndex = _parser.GetLogicalAddressIndex(cargoParam);
            int sequenceNumber = _parser.GetSequenceNumber(p, logicalIndex);

            Assert.AreEqual(expectedSequenceNumber, sequenceNumber);
        }

        [TestMethod]
        public void GetSequenceNumberFromNonRmap2()
        {
            byte[] _nonRmapPacket =
            {
                0x55, 0xfa, 0x00, 0x3e, 0x8a, 0xe4, 0x49, 0x2d, 0xf8, 0x22, 0x36, 0xb6, 0xb0, 0x42, 0x37,
                0xcc, 0x66, 0x7d, 0xb5, 0x03, 0xed, 0xa4, 0x4e, 0x51, 0xf1, 0x04, 0x28, 0x16, 0xe8, 0x8c,
                0x85, 0x5b, 0x46, 0x32, 0x98, 0x57, 0xf0, 0x92, 0x98, 0x38, 0xf4, 0xd8, 0x12, 0x20, 0x77,
                0xe1, 0x83, 0x6d, 0xef, 0x9d, 0x26, 0x3d, 0x92, 0xb8, 0x0b, 0x31, 0x0d, 0x79, 0x69, 0x72,
                0x7a, 0xea, 0x35, 0xf2, 0x8c, 0x39, 0xf3, 0xf5, 0x0d, 0x5e, 0xcd, 0xcf, 0x8b, 0xfe, 0xe2,
                0xef, 0x4e, 0x8f, 0xe0, 0x1c, 0x8d, 0xbe, 0xd5, 0xff, 0xe1, 0x0d, 0x42, 0xe2, 0xf9, 0xc1,
                0x72, 0xb7, 0x90, 0xfb, 0x09, 0x5c, 0x9c, 0x25, 0xe0, 0x1d, 0xc8, 0x8f, 0x10, 0x7b, 0x25,
                0x5a, 0x4d, 0x2b, 0x1c, 0x96, 0x76, 0x62, 0xa8, 0xa7, 0x69, 0x50, 0xc2, 0xed, 0x1c, 0xea,
                0x1c, 0xa4, 0xea, 0xed, 0x11, 0x08, 0x2a, 0x20
            };

            var cargoParam =
                @"55 fa 00 3e 8a e4 49 2d f8 22 36 b6 b0 42 37 cc 66 7d b5 03 ed a4 4e 51 f1 04 28 16 e8 8c 85 5b 46 32 98 57 f0 92 98 38 f4 d8 12 20 77 e1 83 6d ef 9d 26 3d 92 b8 0b 31 0d 79 69 72 7a ea 35 f2 8c 39 f3 f5 0d 5e cd cf 8b fe e2 ef 4e 8f e0 1c 8d be d5 ff e1 0d 42 e2 f9 c1 72 b7 90 fb 09 5c 9c 25 e0 1d c8 8f 10 7b 25 5a 4d 2b 1c 96 76 62 a8 a7 69 50 c2 ed 1c ea 1c a4 ea ed 11 08 2a 20".Split(' ');
            Packet p = new Packet()
            {
                FullPacket = _nonRmapPacket
            };
            const int expectedSequenceNumber = 0;

            int logicalIndex = _parser.GetLogicalAddressIndex(cargoParam);
            int sequenceNumber = _parser.GetSequenceNumber(p, logicalIndex);

            Assert.AreEqual(expectedSequenceNumber, sequenceNumber);
        }

        [TestMethod]
        public void GetSequenceNumberFromRmap()
        {
            const int expected = 0;

            string[] hexData = @"4c 01 7c 20 4a 00 00 00 00 01 00 00 00 00 04 1c 00 00 1e b0 94".Split(' ');
            byte[] packetData =
            {
                0x4c, 0x01, 0x7c, 0x20, 0x4a, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00,
                0x00, 0x00, 0x00, 0x04, 0x1c, 0x00, 0x00, 0x1e, 0xb0, 0x94
            };

            Packet p = new RmapPacket()
            {
                FullPacket = packetData,
            };

            int logicalIndex = _parser.GetLogicalAddressIndex(hexData);
            
            Assert.AreEqual(expected, _parser.GetSequenceNumber(p,logicalIndex));
        }

        [TestMethod]
        public void GetSequenceNumberFromRmap2()
        {
            const int expected = 65533;

            string[] hexData = @"57 01 4c 20 2d ff fd 00 00 02 00 00 00 00 08 d6".Split(' ');
            byte[] packetData =
                {0x57, 0x01, 0x4c, 0x20, 0x2d, 0xff, 0xfd, 0x00, 0x00, 0x02, 0x00, 0x00, 0x00, 0x00, 0x08, 0xd6};

            Packet p = new RmapPacket()
            {
                FullPacket = packetData,
            };

            int logicalIndex = _parser.GetLogicalAddressIndex(hexData);

            Assert.AreEqual(expected, _parser.GetSequenceNumber(p, logicalIndex));
        }

        [TestMethod]
        public void GetRmapPacketFromParserWhenRmapProtocolUsed()
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
            stockResponses.Enqueue(null);
            readerMock.Setup(t => t.ReadLine()).Returns(stockResponses.Dequeue);

            readerMock.SetupSequence(t => t.Peek()).Returns(5).Returns(-1);

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
            stockResponses.Enqueue(null);
            readerMock.Setup(t => t.ReadLine()).Returns(stockResponses.Dequeue);

            readerMock.SetupSequence(t => t.Peek()).Returns(5).Returns(-1);

            parser.ParsePackets(readerMock.Object);

            var expectedValue = new RmapPacket()
            {
                PacketId = Guid.NewGuid(),
                PortNumber = 1,
                Address = new byte[]{33},
                PacketType = "Reply Read"
            };
            var result = parser.PacketDict.Values.FirstOrDefault();
            Assert.AreEqual(expectedValue.PacketType, ((RmapPacket)result).PacketType);
        }

        [TestMethod]
        public void GetRmapType()
        {
            var parser = new Parser();
            const string expectedValue = "Read";
            byte byteExpected = 0x4C;
            var actual = parser.GetRmapType(byteExpected);
            Assert.AreEqual(expectedValue, actual);
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
            stockResponses.Enqueue(null);
            readerMock.Setup(t => t.ReadLine()).Returns(stockResponses.Dequeue);

            readerMock.SetupSequence(t => t.Peek()).Returns(5).Returns(-1);

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

        [TestCleanup()]
        public void Cleanup()
        {
            _parser = null;
        }
    }
}
