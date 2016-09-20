using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StarMeter.Controllers;

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

        [TestCleanup]
        public void Cleanup()
        {
            _errorDetector = null;
        }
    }
}
