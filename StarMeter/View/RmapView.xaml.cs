using System.Collections;
using System.Text;
using System.Windows;
using System.Windows.Media;
using StarMeter.Models;
using StarMeter.View.Helpers;
using System;

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

            if (packet.SecondaryAddress.Length == 1 && packet.SecondaryAddress[0] > 32)
            {
                SourcePathAddressLabel.Content = "Source " + PacketLabelCreator.GetAddressLabel(packet.DestinationAddress);
            }
            else
            {
                var finalAddressString = "Physical Path: ";
                for (var i = 0; i < packet.SecondaryAddress.Length - 1; i++)
                {
                    finalAddressString += Convert.ToInt32(packet.SecondaryAddress[i]) + "  ";
                }
                SourcePathAddressLabel.Content = finalAddressString;
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
