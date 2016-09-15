using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StarMeter;

namespace StarMeter.Tests
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestParseDateTimeType()
        {
            Parser parser = new Parser();

            string str = "31/10/2016";
            DateTime date = parser.parseDateTime(str);
            Assert.IsInstanceOfType(date, typeof(DateTime));
        }
    }
}
