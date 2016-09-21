using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using StarMeter.Models;
using StarMeter.Controllers;
namespace StarMeter.View
{

    public partial class RmapView : Window
    {

        public RmapView()
        {
            InitializeComponent();
        }

        public void SetupElements(Brush brush, RmapPacket p)
        {

            if (p.PacketType.Contains("Reply"))
            {
                DestinationKeyLabel.Content = "Status: ";
                DestinationKeyLabel.Content += p.DestinationKey > 0 ? "Command Failed Execution" : "Command Succesful";
            }
            else
            {
                DestinationKeyLabel.Content = "Destination Key: ";
                DestinationKeyLabel.Content += p.DestinationKey.ToString();
            }

            if (p.SourcePathAddress.Length == 0)
            {
                SourcePathAddressLabel.Content += "No Path Address";
            }
            else
            {
                for (var i = 0; i < p.SourcePathAddress.Length - 1; i++)
                    SourcePathAddressLabel.Content += Convert.ToInt32(p.SourcePathAddress[i]) + "  ";
            }

            CommandByteLabel.Content += ToBitString(Reverse(p.CommandByte));
            PacketTypeLabel.Content += p.PacketType;

        }

        public static string ToBitString(BitArray bits)
        {
            var sb = new StringBuilder();

            for (int i = 0; i < bits.Count; i++)
            {
                char c = bits[i] ? '1' : '0';
                sb.Append(c);
            }

            return sb.ToString();
        }
      
        private void ExitButtonEvent(Object sender, RoutedEventArgs e)
        {
            this.Close();
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
