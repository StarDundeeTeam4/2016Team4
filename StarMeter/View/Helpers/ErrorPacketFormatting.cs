using System.Windows.Media;

namespace StarMeter.View.Helpers
{
    public class ErrorPacketFormatting
    {
        public static Brush GetBrush(bool isError)
        {
            if (isError)
            {
                return Brushes.Red;
            }
            var converter = new BrushConverter();
            return (Brush)converter.ConvertFromString("#6699ff");
        }
    }
}
