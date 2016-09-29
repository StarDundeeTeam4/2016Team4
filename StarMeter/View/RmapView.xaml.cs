using System.Collections;
using System.Text;
using System.Windows;
using System.Windows.Media;
using StarMeter.Models;
using StarMeter.View.Helpers;

namespace StarMeter.View
{
    public partial class RmapView
    {
        public RmapView()
        {
            InitializeComponent();
        }

        public void SetupElements(Brush brush, RmapPacket packet)
        {
            if (packet.PacketType.Contains("Reply"))
            {
                DestinationKeyLabel.Content = "Status: ";
                DestinationKeyLabel.Content += packet.DestinationKey > 0 
                    ? "Command Failed Execution" 
                    : "Command Successful";
            }
            else
            {
                DestinationKeyLabel.Content = "Destination Key: ";
                DestinationKeyLabel.Content += packet.DestinationKey.ToString();
            }

            if (packet.SecondaryAddress.Length == 0)
            {
                SourcePathAddressLabel.Content += "No Path Address";
            }
            else
            {
                SourcePathAddressLabel.Content = PacketLabelCreator.GetSourcePathAddress(packet);
            }
            CommandByteLabel.Content += ToBitString(Reverse(packet.CommandByte));
            PacketTypeLabel.Content += packet.PacketType;
        }



        public static string ToBitString(BitArray bits)
        {
            var stringBuilder = new StringBuilder();
            for (var i = 0; i < bits.Count; i++)
            {
                var character = bits[i] 
                    ? '1' 
                    : '0';
                stringBuilder.Append(character);
            }
            return stringBuilder.ToString();
        }
      
        private void ExitButtonEvent(object sender, RoutedEventArgs e)
        {
            Close();
        }

        //Modified from - Tim Lloyd - StackOverFlow
        public BitArray Reverse(BitArray array)
        {
            var result = array;
            var length = result.Length;
            var mid = length / 2;

            for (var i = 0; i < mid; i++)
            {
                var bit = result[i];
                result[i] = result[length - i - 1];
                result[length - i - 1] = bit;
            }
            return result;
        }
    }
}
