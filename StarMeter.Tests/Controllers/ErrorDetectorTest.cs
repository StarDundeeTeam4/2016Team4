using Microsoft.VisualStudio.TestTools.UnitTesting;
using StarMeter.Controllers;
using StarMeter.Models;

namespace StarMeter.Tests.Controllers
{
    [TestClass]
    public class ErrorDetectorTest
    {
        private ErrorDetector _errorDetector;

        [TestInitialize]
        public void Initialise()
        {
            _errorDetector = new ErrorDetector();
        }

        [TestMethod]
        public void TestBabblingFool()
        {
            byte[] data = { 0x2d, 0x01, 0x0c, 0x00, 0x57, 0xff, 0xfb, 0x00, 0x00, 0x00, 0x08, 0x2e, 0xf3, 0xe3, 0x58, 0x99, 0xaa, 0xef, 0xe5, 0x20, 0x25 };
            Packet packet1 = new Packet()
            {
                FullPacket = data,
                SequenceNum = 01
            };
            Packet packet2 = new Packet()
            {
                FullPacket = data,
                SequenceNum = 02
            };

            var expectedResult = ErrorTypes.DataError;

            var actualResult = _errorDetector.GetErrorType(packet1, packet2);

            Assert.AreEqual(expectedResult, actualResult);
        }

        [TestMethod]
        public void TestSequenceError()
        {
            byte[] data = { 0x57, 0x01, 0x4c, 0x20, 0x2d, 0xff, 0xfb, 0x00, 0x00, 0x02, 0x00, 0x00, 0x00, 0x00, 0x08, 0x12 };
            byte[] data2 = { 0x57, 0x01, 0x4c, 0x20, 0x2d, 0xff, 0xfa, 0x00, 0x00, 0x02, 0x00, 0x00, 0x00, 0x00, 0x08, 0x12 };
            Packet packet1 = new Packet()
            {
                FullPacket = data,
                SequenceNum = 65531
            };
            Packet packet2 = new Packet()
            {
                FullPacket = data2,
                SequenceNum = 65530
            };

            var expectedResult = ErrorTypes.SequenceError;

            var actualResult = _errorDetector.GetErrorType(packet1, packet2);

            Assert.AreEqual(expectedResult, actualResult);  
        }

        [TestMethod]
        public void TestTimeoutError()
        {
            byte[] data = { 0x57, 0x01, 0x4c, 0x20, 0x2d, 0xff, 0xfb, 0x00, 0x00, 0x02, 0x00, 0x00, 0x00, 0x00, 0x08, 0x12 };
            byte[] data2 = { 0x57, 0x01, 0x4c, 0x20, 0x2d, 0xff, 0xfa };
            Packet packet1 = new Packet()
            {
                FullPacket = data,
                SequenceNum = 65531
            };
            Packet packet2 = new Packet()
            {
                FullPacket = data2,
                SequenceNum = 65530
            };

            var expectedResult = ErrorTypes.Timeout;

            var actualResult = _errorDetector.GetErrorType(packet1, packet2);

            Assert.AreEqual(expectedResult, actualResult);  
        }

        [TestMethod]
        public void TestParityError()
        {
            byte[] data = { 0x4c, 0x01, 0x7c, 0x20, 0x4a, 0x00, 0x1f, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x04, 0xfa, 0x00, 0x00, 0x19, 0x42, 0x01 };
            byte[] data2 = { 0x4c, 0x01, 0x7c, 0x20, 0x4a, 0x00, 0x20, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x04, 0xdb, 0x00, 0x00, 0x11, 0xcf, 0x94 };
            Packet packet1 = new Packet()
            {
                FullPacket = data,
                SequenceNum = 31
            };
            Packet packet2 = new Packet()
            {
                FullPacket = data2,
                SequenceNum = 32
            };

            var expectedResult = ErrorTypes.DataError;

            var actualResult = _errorDetector.GetErrorType(packet1, packet2);

            Assert.AreEqual(expectedResult, actualResult);  
        }

        [TestCleanup]
        public void Cleanup()
        {
            _errorDetector = null;
        }
    }
}
