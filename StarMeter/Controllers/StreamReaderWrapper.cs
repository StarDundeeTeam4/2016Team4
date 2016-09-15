using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StarMeter.Interfaces;

namespace StarMeter.Controllers
{
    internal class StreamReaderWrapper : IStreamReader
    {
        private readonly StreamReader _streamReader;

        public StreamReaderWrapper (string path)
        {
            _streamReader = new StreamReader(path);
        }

        public string ReadLine()
        {
            return _streamReader.ReadLine();
        }

        public int Peek()
        {
            return _streamReader.Peek();
        }
    }
}
