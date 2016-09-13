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
        public void PassingStringReturnsCorrectDateTime()
        {
            var parser = new Parser();
            const string stringDateTime = "08-09-2016 14:27:53.726";

            var result = parser.ParseDateTime(stringDateTime);

            var correctDateTime = new DateTime(2016,09,08, 14, 27, 53, 726);
            Assert.AreEqual(correctDateTime, result);
        }
    }
}
