using Microsoft.VisualStudio.TestTools.UnitTesting;
using StarMeter.Controllers;

namespace StarMeter.Tests.Controllers
{
    [TestClass]
    public class CrcTest
    {
        /// <summary>
        /// Checks whether the CRC class produces the correct checksum for a given packet
        /// </summary>
        [TestMethod]
        public void TestCrc()
        {
            byte[] packet = { 0x57, 0x01, 0x4c, 0x20, 0x2d, 0xff, 0xfb, 0x00, 0x00, 0x02, 0x00, 0x00, 0x00, 0x00, 0x08 };

            var result = Crc.RMAP_CalculateCRC(packet);

            Assert.AreEqual(result, 0x3e);
            System.Diagnostics.Trace.WriteLine(Crc.ByteToHexString(result));
        }

        /// <summary>
        /// Checks whether the comparison correctly returns true for a correct checksum for a given packet
        /// </summary>
        [TestMethod]
        public void TestCheckCrcForPacketTrue()
        {
            byte[] packet = { 0x57, 0x01, 0x4c, 0x20, 0x2d, 0xff, 0xfb, 0x00, 0x00, 0x02, 0x00, 0x00, 0x00, 0x00, 0x08, 0x3e };

            Assert.IsTrue(Crc.CheckCrcForPacket(packet));
        }

        /// <summary>
        /// Checks whether the comparison correctly returns false for a correct checksum for a given packet
        /// </summary>
        [TestMethod]
        public void TestCheckCrcForPacketFalse()
        {
            byte[] packet =
            {
                0x00, 0xfe, 0xfa, 0x53, 0x2d, 0xe5, 0x81, 0xd1, 0x27, 0x41, 0xd5, 0xe5, 0xfe, 0xc6, 0x67,
                0x05, 0x54, 0xdd, 0x12, 0x75, 0xf0, 0x86, 0xe4, 0xdd, 0x6c, 0x3f, 0x71, 0x49, 0x2d, 0x29,
                0x6c, 0x73, 0x99, 0x66, 0x78, 0x45, 0x83, 0xc5, 0x3b, 0x9a, 0xea, 0xa1, 0xb4, 0x45, 0xe4,
                0x06, 0xcf, 0x54, 0xd5, 0x16, 0x37, 0x96, 0xe4, 0xab, 0x6c, 0x5a, 0xb0, 0x3e
            };

            Assert.IsFalse(Crc.CheckCrcForPacket(packet));
        }
    }
}
