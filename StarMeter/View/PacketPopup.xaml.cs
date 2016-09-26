using StarMeter.Models;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using StarMeter.Controllers;
using StarMeter.View.Helpers;

namespace StarMeter.View
{
    public partial class PacketPopup
    {
        public PacketPopup()
        {
            InitializeComponent();
        }

        private Packet _packet;
        public Controller Controller;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="packet"></param>
        public void SetupElements(Packet packet)
        {
            if (!(packet is RmapPacket))
            {
                ViewRmapPropertiesButton.Visibility = Visibility.Hidden;
            }

            var brush = ErrorPacketFormatting.GetBrush(packet.IsError);

            _packet = packet;
            Width = 500;
            Height = 500;

            lblErrorMsg.Background = brush;

            var logo = new BitmapImage();
            logo.BeginInit();

            if (!packet.IsError)
            {
                logo.UriSource = new Uri("pack://application:,,,/Resources/tick.png");
                logo.EndInit();
                lblErrorMsg.Content = "SUCCESS";
            }
            else
            {
                logo.UriSource = new Uri("pack://application:,,,/Resources/Error.png");
                logo.EndInit();
                lblErrorMsg.Content = "ERROR: " + packet.ErrorType;
            }

            IconBG.Background = brush;
            ErrorIcon.Source = logo;

            TimeLabel.Content = packet.DateReceived.ToString("dd-MM-yyyy HH:mm:ss.fff");
            var protocolId = packet.ProtocolId;
            ProtocolLabel.Content = PacketLabelCreator.GetProtocolLabel(protocolId);

            if (packet.ErrorType.Equals(ErrorType.SequenceError)) 
            {
                SequenceNumberLabel.Foreground = Brushes.Red;
            }
            else
            {
                SequenceNumberLabel.Foreground = Brushes.White;
            }

            if (packet.ErrorType.Equals(ErrorType.DataError))
            {
                CargoButton.Background = Brushes.Red;
            }
            else
            {
                BrushConverter bc = new BrushConverter();
                CargoButton.Background = (Brush)bc.ConvertFromString("#FF4A4D54");
            }

            SequenceNumberLabel.Content = "Sequence Number: " + packet.SequenceNum;

            AddressLabel.Content = PacketLabelCreator.GetAddressLabel(packet.Address);
            LeftArrow.Visibility = _packet.PrevPacket == null 
                ? Visibility.Collapsed 
                : Visibility.Visible;

            RightArrow.Visibility = _packet.NextPacket == null 
                ? Visibility.Collapsed 
                : Visibility.Visible;
        }

        private void NextPacket(object sender, RoutedEventArgs e)
        {
            if (_packet.NextPacket != null)
            {
                var packet = Controller.FindPacket(_packet.NextPacket.GetValueOrDefault());
                SetupElements(packet);
            }
        }

        private void PrevPacket(object sender, RoutedEventArgs e)
        {
            if (_packet.PrevPacket != null)
            {
                var packet = Controller.FindPacket(_packet.PrevPacket.GetValueOrDefault());
                SetupElements(packet);
            }
        }

        private void ViewCargo(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            var brush = button.Background;

            if (_packet.Cargo != null)
            {
                var cargoView = new CargoView();
                cargoView.SetupElements(brush, _packet);
                cargoView.Owner = this;
                cargoView.Show();
            }
            else
            {
                MessageBox.Show("No Cargo");
            }
        }

        private void ExitButtonEvent(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void ShowRmapProperties(object sender, RoutedEventArgs e)
        {
            var button = (Button) sender;
            var br = button.Background;

            var rmapView = new RmapView();
            rmapView.SetupElements(br, (RmapPacket)_packet);
            rmapView.Owner = this;
            rmapView.Show();
        }

        private void LeftArrow_MouseRightButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var packetPopup = new PacketPopup();
            var packet = Controller.FindPacket(_packet.PrevPacket.GetValueOrDefault());

            ArrowMouseRightButtonDown(packet, packetPopup);
            packetPopup.Left = 0;
            packetPopup.Top = SystemParameters.VirtualScreenHeight / 2 - 250;

            packetPopup.Show();
        }

        private void RightArrow_MouseRightButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var packetPopup = new PacketPopup();
            var packet = Controller.FindPacket(_packet.NextPacket.GetValueOrDefault());

            ArrowMouseRightButtonDown(packet, packetPopup);
            packetPopup.Left = SystemParameters.VirtualScreenWidth - 500;
            packetPopup.Top = SystemParameters.VirtualScreenHeight / 2 - 250;

            packetPopup.Show();
        }

        private void ArrowMouseRightButtonDown(Packet packet, PacketPopup packetPopup)
        {
            packetPopup.SetupElements(packet);
            packetPopup.Owner = this;
            packetPopup.Controller = Controller;
            packetPopup.WindowStartupLocation = WindowStartupLocation.Manual;
        }
        
        private void cmdCopyToClipboard_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(_packet.DateReceived.ToString("dd-MM-yyyy HH:mm:ss.fff"));
            MessageBox.Show("Copied to clipboard");
        }
    }
}