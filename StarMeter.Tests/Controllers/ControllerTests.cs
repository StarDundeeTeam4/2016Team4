using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StarMeter.Controllers;

namespace StarMeter.Tests.Controllers
{
    [TestClass]
    public class ControllerTests
    {
        private Controller controller;

        [TestInitialize]
        public void Initialize()
        {
            controller = new Controller();
        }

        [TestMethod]
        public void TestAddFileNames()
        {
            Assert.AreEqual(controller.filePaths.Count, 0);

            string[] toAdd = {"test1", "test2"};
            controller.AddFileNames(toAdd);

            Assert.AreEqual(controller.filePaths.Count, 2);
            Assert.AreEqual(controller.filePaths.First(), "test1");
            Assert.AreEqual(controller.filePaths.Last(), "test2");
        }

        [TestMethod]
        public void TestGetFileNames()
        {
            controller.filePaths.Add(@"C:\\Users\\Phil\\Desktop\\tp\\test1_link1.rec");
            controller.filePaths.Add(@"C:\\Users\\Phil\\Desktop\\tp\\test2_link1.rec");

            string[] response = controller.GetFileNames();
            string[] expected = {"test1_link1.rec", "test2_link1.rec"};

            Assert.IsTrue(response.SequenceEqual(expected));
        }

        [TestMethod]
        public void RemoveFile()
        {
            controller.filePaths.Add(@"C:\\Users\\Phil\\Desktop\\tp\\test1_link1.rec");
            controller.filePaths.Add(@"C:\\Users\\Phil\\Desktop\\tp\\test2_link1.rec");

            controller.RemoveFile("test2_link1.rec");

            List <string> response = controller.filePaths;
            List<string> expected = new List<string>();
            expected.Add(@"C:\\Users\\Phil\\Desktop\\tp\\test1_link1.rec");

            Assert.IsTrue(response.SequenceEqual(expected));
        }

        [TestCleanup()]
        public void Cleanup()
        {
            controller = null;
        }
    }
}
