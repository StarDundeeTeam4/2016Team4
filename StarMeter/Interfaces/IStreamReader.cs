using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarMeter.Interfaces
{
    public interface IStreamReader
    {
        string ReadLine();
        int Peek();
    }
}
