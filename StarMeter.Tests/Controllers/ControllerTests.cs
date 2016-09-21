using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StarMeter.Controllers;

namespace StarMeter.Tests.Controllers
{
    [TestClass]
    public class ControllerTests
    {
        private Controller _controller;

        [TestInitialize]
        public void Initialize()
        {
            _controller = new Controller();
        }

        [TestMethod]
        public void TestAddFileNames()
        {
            Assert.AreEqual(_controller.filePaths.Count, 0);

            string[] toAdd = {"test1", "test2"};
            _controller.AddFileNames(toAdd);

            Assert.AreEqual(_controller.filePaths.Count, 2);
            Assert.AreEqual(_controller.filePaths.First(), "test1");
            Assert.AreEqual(_controller.filePaths.Last(), "test2");
        }

        [TestMethod]
        public void TestGetFileNames()
        {
            _controller.filePaths.Add(@"C:\\Users\\Phil\\Desktop\\tp\\test1_link1.rec");
            _controller.filePaths.Add(@"C:\\Users\\Phil\\Desktop\\tp\\test2_link1.rec");

            string[] response = _controller.GetFileNames();
            string[] expected = {"test1_link1.rec", "test2_link1.rec"};

            Assert.IsTrue(response.SequenceEqual(expected));
        }

        [TestMethod]
        public void RemoveFile()
        {
            _controller.filePaths.Add(@"C:\\Users\\Phil\\Desktop\\tp\\test1_link1.rec");
            _controller.filePaths.Add(@"C:\\Users\\Phil\\Desktop\\tp\\test2_link1.rec");

            _controller.RemoveFile("test2_link1.rec");

            List <string> response = _controller.filePaths;
            List<string> expected = new List<string> {@"C:\\Users\\Phil\\Desktop\\tp\\test1_link1.rec"};

            Assert.IsTrue(response.SequenceEqual(expected));
        }

        [TestCleanup]
        public void Cleanup()
        {
            _controller = null;
        }
    }
}
