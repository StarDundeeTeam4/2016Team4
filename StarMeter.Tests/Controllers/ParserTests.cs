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
        }

        [TestMethod]
        public void GivenAnIntGetPacketTypeShouldReturnPortNumberTest()
        {
            var inputLine = "1";
            var parser = new Parser();
            var expectedResult = "port number";

            var actualResult = parser.GetPacketType(inputLine);


            Assert.AreEqual(actualResult, expectedResult);
        }

        [TestMethod]
        public void GivenACharGetPacketTypeShouldReturnPacketTest()
        {
            var inputLine = "P";
            var parser = new Parser();
            var expectedResult = "packet";

            var actualResult = parser.GetPacketType(inputLine);

            Assert.AreEqual(actualResult, expectedResult);
        }
            
    }
}
