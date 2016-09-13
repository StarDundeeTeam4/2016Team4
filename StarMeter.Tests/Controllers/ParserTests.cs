using System;
using System.Collections;
using System.Diagnostics;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StarMeter.Controllers;
using StarMeter.Models;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace StarMeter.Tests.Controllers
{
    [TestClass]
    public class ParserTests : Parser
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
        public void ParsePacketTest()
        {
            const string testData = @"08-09-2016 13:57:18.107
3

08-09-2016 13:57:29.249
P
01 fe 00 01 02 03 04 05 06 07
EOP

08-09-2016 13:58:23.546";

            var parser = new ParserTests();
            var r = new StringReader(testData);

            IEnumerable<Packet> e = parser.ParsePacket(r);
            Assert.AreEqual(1, e.Count());
        }
    }
}
