using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StarMeter.Controllers;
using StarMeter.Models;

namespace StarMeter.Tests.Controllers
{
    [TestClass]
    public class ParserTests
    {
        [TestMethod]
        public void PassingCorrectStringReturnsDateTimeTest()
        {
            var parser = new Parser();
            const string stringDateTime = "08-09-2016 14:27:53.726";
            var result = parser.ParseDateTime(stringDateTime);

            Assert.IsInstanceOfType(result, typeof(DateTime));
            Assert.AreEqual(result, new DateTime(2016, 09, 08, 14, 27, 53, 726));
        }

        [TestMethod]
        public void GivenAnIntGetPacketTypeShouldReturnPortNumberTest()
        {
            const string inputLine = "1";
            var parser = new Parser();
            const string expectedResult = "port number";

            var actualResult = parser.GetPacketType(inputLine);


            Assert.AreEqual(actualResult, expectedResult);
        }

        [TestMethod]
        public void GivenACharGetPacketTypeShouldReturnPacketTest()
        {
            const string inputLine = "P";
            var parser = new Parser();
            const string expectedResult = "packet";

            var actualResult = parser.GetPacketType(inputLine);

            Assert.AreEqual(actualResult, expectedResult);
        }

        [TestMethod]
        public void NoErrorOnePacketReturnsListWithOneElement()
        {
            var parser = new Parser();
            var r = new StreamReader("testRecOnePacket.rec");
            DateTime stub;
            var result = parser.ParsePackets(r, out stub);
            var expected = new List<Packet>
            {
                new Packet()
                {
                    IsError = false,
                    DateRecieved = DateTime.ParseExact("08-09-2016 15:12:50.081", "dd-MM-yyyy HH:mm:ss.fff", null),
                    Cargo =
                        @"00 fe fa 00 17 50 b8 f6 ca d3 9e 3c 52 74 51 9f ef 80 ba f6 75 92 de c3 aa 62 5f aa f0 de 46 28 24 7c ff 81 c5 ce a5 fa 59 57 81 49 0c 9d cd 4a 9b 7f bd f3 70 c9 c0 8a 0f 06 03 15 b0 95 36 13 2d ff 94 69 1f 88 1d 9f 44 04 26 4c 25 ec 14 cf f5 b1 65 40 bb 50 f0 a7 b4 27 6d 6b f2 07 37 0d 4a 8a 51 15 6d a7 a7 4d 55 83 97 2e e3 8a b0 98 c6 bf ba c6 9e 50 f6 80 61 6e a7 92 fe 5b d0 7e 41 c5 40 6e f7 52 cc 6c 52 7c dc d5 8f 9f 29 0b d5 50 c4 6b 61 f1 5b 7f e0 82 b8 74 1c ba 8a ce db 57 68 5a 04 b2 13 64 04 96 fb 2b 70 52 05 92 ec 0d 8c 18 4b 5a a6 0a f8 0d a8 f8 94 4c ec 65 e0 e9 d1 c2 de ef 04 9e 33 7a fe 17 d0 cc ce 94 d1 9e 19 b6 a5 b4 5f 8b 70 b4 7f 05 ad 38 7e ab 18 22 84 8f cb 30 27 80 a7 d0 ec 80 f5 35 0b 79 4d aa 73 2b b7 26 0e 69 11 21 46 85 b1 a7 c8"
                            .Split(' ')
                }
            };
            Assert.AreEqual(expected[0].Cargo.Length, result[0].Cargo.Length);
            Assert.AreEqual(expected[0].Cargo[254], result[0].Cargo[254]);
            Assert.AreEqual(expected[0].DateRecieved, result[0].DateRecieved);
            Assert.AreEqual(expected[0].IsError, result[0].IsError);
        }

        [TestMethod]
        public void PacketCollectionWithErrorParsed()
        {
            var parser = new Parser();
            var r = new StreamReader("testRecErrorPacket.rec");
            DateTime stub;
            var result = parser.ParsePackets(r, out stub);
            var expected = new List<Packet>
            {
                new Packet()
                {
                    IsError = false,
                    DateRecieved = DateTime.ParseExact("08-09-2016 15:12:50.081", "dd-MM-yyyy HH:mm:ss.fff", null),
                    Cargo =
                        @"00 fe fa 00 17 50 b8 f6 ca d3 9e 3c 52 74 51 9f ef 80 ba f6 75 92 de c3 aa 62 5f aa f0 de 46 28 24 7c ff 81 c5 ce a5 fa 59 57 81 49 0c 9d cd 4a 9b 7f bd f3 70 c9 c0 8a 0f 06 03 15 b0 95 36 13 2d ff 94 69 1f 88 1d 9f 44 04 26 4c 25 ec 14 cf f5 b1 65 40 bb 50 f0 a7 b4 27 6d 6b f2 07 37 0d 4a 8a 51 15 6d a7 a7 4d 55 83 97 2e e3 8a b0 98 c6 bf ba c6 9e 50 f6 80 61 6e a7 92 fe 5b d0 7e 41 c5 40 6e f7 52 cc 6c 52 7c dc d5 8f 9f 29 0b d5 50 c4 6b 61 f1 5b 7f e0 82 b8 74 1c ba 8a ce db 57 68 5a 04 b2 13 64 04 96 fb 2b 70 52 05 92 ec 0d 8c 18 4b 5a a6 0a f8 0d a8 f8 94 4c ec 65 e0 e9 d1 c2 de ef 04 9e 33 7a fe 17 d0 cc ce 94 d1 9e 19 b6 a5 b4 5f 8b 70 b4 7f 05 ad 38 7e ab 18 22 84 8f cb 30 27 80 a7 d0 ec 80 f5 35 0b 79 4d aa 73 2b b7 26 0e 69 11 21 46 85 b1 a7 c8"
                            .Split(' ')
                },
                new Packet()
                {
                     DateRecieved = DateTime.ParseExact("08-09-2016 16:59:60.287", "dd-MM-yyyy HH:mm:ss.fff", null)
                }
            };
            Assert.AreEqual(expected.Count, result.Count);
            Assert.AreEqual(expected[1].Cargo.Length, result[1].Cargo.Length);
            Assert.AreEqual(expected[0].DateRecieved, result[0].DateRecieved);
            Assert.AreEqual(expected[0].IsError, result[0].IsError);
        }

    }
}
