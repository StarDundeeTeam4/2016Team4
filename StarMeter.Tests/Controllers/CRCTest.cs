using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StarMeter.Controllers;

namespace StarMeter.Tests.Controllers
{
    [TestClass]
    public class CRCTest
    {

        [TestMethod]
        public void TestCRC()
        {
            byte[] packet = { 0x57, 0x01, 0x4c, 0x20, 0x2d, 0xff, 0xfb, 0x00, 0x00, 0x02, 0x00, 0x00, 0x00, 0x00, 0x08 };

            ushort result = CRC.RMAP_CalculateCRC(packet);

            Assert.AreEqual(result, 0x3e);
            System.Diagnostics.Trace.WriteLine(CRC.ByteToHexString(result));
        }
    }
}
