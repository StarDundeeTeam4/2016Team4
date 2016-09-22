namespace StarMeter.Interfaces
{
    public interface IStreamReader
    {
        string ReadLine();
        int Peek();
        void Close();
    }
}
