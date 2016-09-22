using System.IO;
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

        public void Close()
        {
            _streamReader.Close();
        }
    }
}
