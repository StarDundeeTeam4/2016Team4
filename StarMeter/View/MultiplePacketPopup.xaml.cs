using StarMeter.Controllers;
using StarMeter.Models;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using StarMeter.View.Helpers;

namespace StarMeter.View
{
    /// <summary>
    /// Interaction logic for MultiplePacketPopup.xaml
    /// </summary>
    public partial class MultiplePacketPopup
    {
        public Controller Controller;

        public MultiplePacketPopup(Controller controller)
        {
            InitializeComponent();
            Controller = controller;
        }

        public void CreateElements(List<Packet> packets) 
        {
            foreach(var packet in packets)
            {
                var button = GetPacketButton(packet);
                button.Margin = new Thickness(5, 2.5, 5, 3);
                button.Height = 50;
                PacketList.Children.Add(button);
            }
        }

        public Button GetPacketButton(Packet packet)
        {
            #region Create Button for the packet
            string sty;

            var button = new Button
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                HorizontalContentAlignment = HorizontalAlignment.Center
            };
            button.Click += OpenPopup;

            var buttonLabel = new Label
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                HorizontalContentAlignment = HorizontalAlignment.Center,
                FontFamily = new System.Windows.Media.FontFamily("Gill Sans MT")
            };

            try
            {
                var addressArray = packet.DestinationAddress;
                buttonLabel.Content = PacketLabelCreator.GetAddressLabel(addressArray);
                var protocolId = packet.ProtocolId;
                buttonLabel.Content += Environment.NewLine + PacketLabelCreator.GetProtocolLabel(protocolId);
            }
            catch (Exception)
            {
                buttonLabel.Content = "Unknown Packet Type";
            }

            try
            {
                button.Tag = packet.PacketId;
            }
            catch (Exception)
            {
                button.Tag = "";
            }

            button.Content = buttonLabel;

            try
            {
                sty = packet.IsError ? "Error" : "Success";
            }
            catch (Exception)
            {
                sty = "Error";
            }

            button.SetResourceReference(StyleProperty, sty);

            if (packet.IsError)
            {
                button.ToolTip = packet.ErrorType;
            }
            else
            {
                button.ToolTip = "No Errors";
            }

            return button;
            #endregion
        }

        public void OpenPopup(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            var buttonTag = button.Tag.ToString();
            var packetId = new Guid(buttonTag);

            var packetPopup = new PacketPopup
            {
                Controller = Controller
            };

            var packet = Controller.FindPacket(packetId);

            if (packet != null)
            {
                packetPopup.SetupElements(packet); // send the packet as a parameter, along with the colour to make the header
                packetPopup.Owner = this;
                packetPopup.Show();
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

    }
}
