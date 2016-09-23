﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StarMeter.Controllers;
using StarMeter.Models;

namespace StarMeter.Tests.Controllers
{
    [TestClass]
    public class PacketHandlerTests
    {
        [TestInitialize]
        public void Initialize()
        {
            
        }

        [TestMethod]
        public void SplitCargoFromNonRmapPacket()
        {
            byte[] data = { 0x2d, 0x01, 0x0c, 0x00, 0x57, 0xff, 0xfb, 0x00, 0x00, 0x00, 0x08, 0x2e, 0xf3, 0xe3, 0x58, 0x99, 0xaa, 0xef, 0xe5, 0x20, 0x25 };
            var packet = new Packet
            {
                ProtocolId = 42,
                FullPacket = data,
            };
            byte[] expectedCargo = { 0x01, 0x0c, 0x00, 0x57, 0xff, 0xfb, 0x00, 0x00, 0x00, 0x08, 0x2e, 0xf3, 0xe3, 0x58, 0x99, 0xaa, 0xef, 0xe5, 0x20, 0x25 };
            var cargo = PacketHandler.GetCargoArray(packet);

            CollectionAssert.AreEqual(cargo, expectedCargo);
        }

        [TestMethod]
        public void SplitCargoFromRmapPacket()
        {
            byte[] data = { 0x2d, 0x01, 0x0c, 0x00, 0x57, 0xff, 0xfb, 0x00, 0x00, 0x00, 0x08, 0x2e, 0xf3, 0xe3, 0x58, 0x99, 0xaa, 0xef, 0xe5, 0x20, 0x25 };
            byte[] expectedCargo = { 0xf3, 0xe3, 0x58, 0x99, 0xaa, 0xef, 0xe5, 0x20, 0x25 };
            var packet = new Packet
            {
                ProtocolId = 01,
                FullPacket = data,
            };

            var cargo = PacketHandler.GetCargoArray(packet);

            CollectionAssert.AreEqual(cargo, expectedCargo);
        }

        [TestMethod]
        public void GetCargoArrayFail()
        {
            byte[] data = new byte[0];
            Packet p = new Packet()
            {
                FullPacket = data,
                ProtocolId = 1,
            };

            byte[] result = PacketHandler.GetCargoArray(p);
            Assert.IsNull(result);
        }

        [TestMethod]
        public void GetCargoArrayOverflow()
        {
            byte[] data = {0x02, 0xfe, 0x01, 0x0d, 0x00, 0xfe, 0x00};

            Packet p = new Packet()
            {
                FullPacket = data,
                ProtocolId = 1,
            };

            byte[] result = PacketHandler.GetCargoArray(p);
            CollectionAssert.AreEqual(data, result);
        }

        [TestMethod]
        public void PassingCorrectStringReturnsDateTimeTest()
        {
            const string stringDateTime = "08-09-2016 14:27:53.726";
            DateTime result;

            Assert.IsTrue(PacketHandler.ParseDateTime(stringDateTime, out result));
            Assert.IsInstanceOfType(result, typeof(DateTime));
            Assert.AreEqual(result, new DateTime(2016, 09, 08, 14, 27, 53, 726));
        }

        [TestMethod]
        public void PassingCargoWithAddressinFirstByteReturnsIndex()
        {
            byte[] cargo =
                {0x57, 0x01, 0x4c, 0x20, 0x2d, 0xff, 0xfb, 0x00, 0x00, 0x02, 0x00, 0x00, 0x00, 0x00, 0x08, 0x3e};
            var packet = new Packet
            {
                FullPacket = cargo
            };
            const int expected = 0;
            var actual = PacketHandler.GetLogicalAddressIndex(packet);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void PassingCargoWithAddressInThirdByteReturnsIndex()
        {
            byte[] cargo =
            {
                0x01, 0x00, 0xfe, 0xfa, 0x4b, 0xe7, 0x5d, 0xb0, 0x07, 0xbd, 0x57, 0x83, 0x82, 0xac, 0xac, 0x66, 0x79,
                0x2e, 0x35, 0xd7, 0x3d, 0xdb, 0x80, 0x41, 0x5e, 0x98, 0x2d, 0xd1, 0x29, 0x0b, 0x98, 0x85, 0x6f, 0x52,
                0xb9, 0x86, 0x88, 0xf4, 0x48, 0xb7, 0x98, 0x17, 0x24, 0x8e, 0x11, 0xf4, 0xf5, 0x23, 0x79, 0x89, 0x10,
                0x86, 0x6e, 0x8d, 0x9c, 0x56, 0xe7, 0xa9, 0xf3, 0x8f, 0xbc, 0xe5, 0x4b, 0xa4, 0x69, 0x61, 0x96, 0xce,
                0x0f, 0x45, 0xa7, 0xa3, 0xbb, 0xfa, 0x95, 0xea, 0xeb, 0x9b, 0x64, 0xae, 0x4e, 0xa6, 0xe2, 0x91, 0xcc,
                0xf8, 0xd0, 0x63, 0xc6, 0xf8, 0x42, 0xf3, 0x9f, 0x12, 0x49, 0x25, 0x7b, 0x67, 0xce, 0x45, 0x46, 0xc2,
                0xc1, 0x9b, 0xf9, 0xce, 0x74, 0x40, 0x18, 0x87, 0x41, 0x30, 0x7f, 0xef, 0x73, 0x9f, 0x5f, 0x02, 0x64,
                0x91, 0x84, 0xeb, 0xe2, 0xa5, 0xd9, 0x78, 0xd7, 0x31, 0x2d, 0xcc, 0x29, 0x94, 0x37, 0x54, 0xe0, 0xc8,
                0xd8, 0xfc, 0x87, 0x37, 0xa1, 0xa2, 0xd4, 0xd0, 0x95, 0xca, 0x8b, 0x59, 0x30, 0x94, 0x9f, 0x07, 0xa8,
                0xec, 0x9a, 0x4c, 0x72, 0x01, 0x40, 0xf0, 0x08, 0xf8, 0x6f, 0x63, 0xea, 0xe1, 0x4a, 0x52, 0xe2, 0xea,
                0x97, 0x78, 0x8d, 0xd2, 0x64, 0xb7, 0x17, 0xa1, 0xf1, 0x67, 0x46, 0x97, 0xca, 0xee, 0xba, 0x62, 0x34,
                0x90, 0x73, 0x94, 0xca, 0x89, 0x93, 0x53, 0x68, 0x59, 0x65, 0x52, 0x48, 0x60, 0x9f, 0x01, 0x6b, 0xaa,
                0xe4, 0x01, 0x3b, 0x0c, 0x8d, 0xdb, 0x6e, 0x70, 0xa9, 0xf1, 0x2d, 0x6d, 0x42, 0xb5, 0x76, 0x1b, 0xe3,
                0x18, 0xbf, 0x25, 0x56, 0x46, 0xdc, 0x1f, 0xb2, 0x90, 0x22, 0x1a, 0x96, 0xa9, 0xcd, 0x76, 0xaf, 0x16,
                0x9f, 0xf8, 0x80, 0xe0, 0xca, 0x1c, 0x62, 0x8c, 0x10, 0xad, 0xc9, 0x4c, 0x29, 0x92, 0xca, 0x77, 0x65,
                0xeb
            };

            var packet = new Packet
            {
                FullPacket = cargo
            };

            const int expected = 2;
            var actual = PacketHandler.GetLogicalAddressIndex(packet);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void GetPhysicalPathPortIndexesFromAddress()
        {
            byte[] cargo =
            {
                0x00, 0x04, 0xfe, 0xfa, 0x4b, 0xe7, 0x5d, 0xb0, 0x07, 0xbd, 0x57, 0x83, 0x82, 0xac, 0xac, 0x66, 0x79,
                0x2e, 0x35, 0xd7, 0x3d, 0xdb, 0x80, 0x41, 0x5e, 0x98, 0x2d, 0xd1, 0x29, 0x0b, 0x98, 0x85, 0x6f, 0x52,
                0xb9, 0x86, 0x88, 0xf4, 0x48, 0xb7, 0x98, 0x17, 0x24, 0x8e, 0x11, 0xf4, 0xf5, 0x23, 0x79, 0x89, 0x10,
                0x86, 0x6e, 0x8d, 0x9c, 0x56, 0xe7, 0xa9, 0xf3, 0x8f, 0xbc, 0xe5, 0x4b, 0xa4, 0x69, 0x61, 0x96, 0xce,
                0x0f, 0x45, 0xa7, 0xa3, 0xbb, 0xfa, 0x95, 0xea, 0xeb, 0x9b, 0x64, 0xae, 0x4e, 0xa6, 0xe2, 0x91, 0xcc,
                0xf8, 0xd0, 0x63, 0xc6, 0xf8, 0x42, 0xf3, 0x9f, 0x12, 0x49, 0x25, 0x7b, 0x67, 0xce, 0x45, 0x46, 0xc2,
                0xc1, 0x9b, 0xf9, 0xce, 0x74, 0x40, 0x18, 0x87, 0x41, 0x30, 0x7f, 0xef, 0x73, 0x9f, 0x5f, 0x02, 0x64,
                0x91, 0x84, 0xeb, 0xe2, 0xa5, 0xd9, 0x78, 0xd7, 0x31, 0x2d, 0xcc, 0x29, 0x94, 0x37, 0x54, 0xe0, 0xc8,
                0xd8, 0xfc, 0x87, 0x37, 0xa1, 0xa2, 0xd4, 0xd0, 0x95, 0xca, 0x8b, 0x59, 0x30, 0x94, 0x9f, 0x07, 0xa8,
                0xec, 0x9a, 0x4c, 0x72, 0x01, 0x40, 0xf0, 0x08, 0xf8, 0x6f, 0x63, 0xea, 0xe1, 0x4a, 0x52, 0xe2, 0xea,
                0x97, 0x78, 0x8d, 0xd2, 0x64, 0xb7, 0x17, 0xa1, 0xf1, 0x67, 0x46, 0x97, 0xca, 0xee, 0xba, 0x62, 0x34,
                0x90, 0x73, 0x94, 0xca, 0x89, 0x93, 0x53, 0x68, 0x59, 0x65, 0x52, 0x48, 0x60, 0x9f, 0x01, 0x6b, 0xaa,
                0xe4, 0x01, 0x3b, 0x0c, 0x8d, 0xdb, 0x6e, 0x70, 0xa9, 0xf1, 0x2d, 0x6d, 0x42, 0xb5, 0x76, 0x1b, 0xe3,
                0x18, 0xbf, 0x25, 0x56, 0x46, 0xdc, 0x1f, 0xb2, 0x90, 0x22, 0x1a, 0x96, 0xa9, 0xcd, 0x76, 0xaf, 0x16,
                0x9f, 0xf8, 0x80, 0xe0, 0xca, 0x1c, 0x62, 0x8c, 0x10, 0xad, 0xc9, 0x4c, 0x29, 0x92, 0xca, 0x77, 0x65,
                0xeb
            };

            var packet = new Packet
            {
                FullPacket = cargo
            };

            var addressArray = PacketHandler.GetAddressArray(packet);
            var expectedPathValues = new[] { 0x00, 0x04, 0xfe };

            Assert.AreEqual(expectedPathValues[1], addressArray[1]);
        }

        [TestMethod]
        public void GetLogicalAddress()
        {
            byte[] cargo =
                {0x57, 0x01, 0x4c, 0x20, 0x2d, 0xff, 0xfb, 0x00, 0x00, 0x02, 0x00, 0x00, 0x00, 0x00, 0x08, 0x3e};
            var packet = new Packet
            {
                FullPacket = cargo
            };

            var expectedPathValues = new[] { 0x57 };
            var physicalPathValues = PacketHandler.GetAddressArray(packet);

            Assert.AreEqual(expectedPathValues[0], physicalPathValues[0]);
        }

        [TestMethod]
        public void GetCrcFromCargo()
        {
            byte[] cargo =
                {0x57, 0x01, 0x4c, 0x20, 0x2d, 0xff, 0xfb, 0x00, 0x00, 0x02, 0x00, 0x00, 0x00, 0x00, 0x08, 0x3e};
            var packet = new Packet
            {
                FullPacket = cargo
            };

            var expected = (byte)Convert.ToInt32("3e", 16);
            var actual = PacketHandler.GetCrc(packet);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void GetProtocolIdFromCargo()
        {
            byte[] cargo =
                {0x57, 0x01, 0x4c, 0x20, 0x2d, 0xff, 0xfb, 0x00, 0x00, 0x02, 0x00, 0x00, 0x00, 0x00, 0x08, 0x3e};
            var packet = new Packet
            {
                FullPacket = cargo
            };

            const int expected = 1;
            var actual = PacketHandler.GetProtocolId(packet);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void GetProtocolIdFail()
        {
            byte[] data = new byte[0];
            Packet p = new Packet()
            {
                FullPacket = data,
            };

            int result = PacketHandler.GetProtocolId(p);
            Assert.AreEqual(-1, result);
        }

        [TestMethod]
        public void GetSequenceNumberFromNonRmap()
        {
            byte[] cargo =
            {
                0x3f, 0xfa, 0x0f, 0x33, 0xd2, 0x6b, 0xef, 0x66, 0x29, 0xdb, 0x84, 0x3f, 0x1a, 0xd7, 0x68, 0x4a, 0x10,
                0xe9, 0x8e, 0x01, 0xb2, 0xf3, 0xe3, 0xed, 0x70, 0x71, 0x81, 0x0e, 0x5c, 0xfe, 0x25, 0x7d, 0x3c, 0x1a,
                0xe3, 0x50, 0xdd, 0x2a, 0x18, 0xbe, 0xaa, 0xc0, 0xa1, 0x84, 0x7c, 0x1d, 0x4a, 0x86, 0xb4, 0xf4, 0x89,
                0xba, 0x88, 0x71, 0x6d, 0x42, 0x9a, 0x7a, 0xd1, 0xd8, 0xcf, 0x35, 0xd5, 0x6f, 0x5e, 0xca, 0x8f, 0x17,
                0x27, 0x6a, 0xfd, 0x22, 0x47, 0x90, 0x3e, 0x85, 0xe9, 0x86, 0x02, 0x81, 0x5e, 0x2c, 0x40, 0x82, 0x71,
                0xf5, 0x27, 0x47, 0xf4, 0x32, 0x56, 0x43, 0x9f, 0x93, 0x4f, 0x43, 0x1b, 0xea, 0x29, 0x52
            };

            var packet = new Packet
            {
                FullPacket = cargo
            };

            const int expectedSequenceNumber = 15;
            var sequenceNumber = PacketHandler.GetSequenceNumber(packet);

            Assert.AreEqual(expectedSequenceNumber, sequenceNumber);
        }

        [TestMethod]
        public void GetSequenceNumberFromNonRmap2()
        {
            byte[] cargo =
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

            var packet = new Packet
            {
                FullPacket = cargo
            };

            const int expectedSequenceNumber = 0;
            var sequenceNumber = PacketHandler.GetSequenceNumber(packet);

            Assert.AreEqual(expectedSequenceNumber, sequenceNumber);
        }

        [TestMethod]
        public void GetSequenceNumberFromRmap()
        {
            byte[] cargo =
            {
                0x4c, 0x01, 0x7c, 0x20, 0x4a, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00,
                0x00, 0x00, 0x00, 0x04, 0x1c, 0x00, 0x00, 0x1e, 0xb0, 0x94
            };

            var packet = new RmapPacket
            {
                FullPacket = cargo
            };

            const int expectedResult = 0;
            var actualResult = PacketHandler.GetSequenceNumber(packet);

            Assert.AreEqual(expectedResult, actualResult);
        }

        [TestMethod]
        public void GetSequenceNumberFromRmap2()
        {
            byte[] cargo =
                {0x57, 0x01, 0x4c, 0x20, 0x2d, 0xff, 0xfd, 0x00, 0x00, 0x02, 0x00, 0x00, 0x00, 0x00, 0x08, 0xd6};

            var packet = new RmapPacket
            {
                FullPacket = cargo
            };

            const int expectedResult = 65533;
            var actualResult = PacketHandler.GetSequenceNumber(packet);

            Assert.AreEqual(expectedResult, actualResult);
        }

        [TestMethod]
        public void GetSequenceNumberFail()
        {
            byte[] data = new byte[0];
            Packet p = new Packet() {FullPacket = data,};
            int result = PacketHandler.GetSequenceNumber(p);
            Assert.AreEqual(-1, result);
        }

        [TestCleanup]
        public void Cleanup()
        {
            
        }
    }
}
