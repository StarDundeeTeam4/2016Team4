using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StarMeter.Controllers;

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
        public void GiveOnePacketRecording()
        {
            var parser = new Parser();
            parser.ParseFile();

        }

    }
}
