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

        public void SetupElements(Packet packet)
        {
            if (!(packet is RmapPacket))
            {
                ViewRmapPropertiesButton.Visibility = Visibility.Hidden;
            }
            var br = GetBrush(packet.IsError);

            _packet = packet;
            Width = 500;
            Height = 500;

            lblErrorMsg.Background = br;

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

            IconBG.Background = br;
            ErrorIcon.Source = logo;

            TimeLabel.Content = packet.DateReceived.ToString("dd-MM-yyyy HH:mm:ss.fff");
            var protocolId = packet.ProtocolId;
            ProtocolLabel.Content = PacketLabelCreator.GetProtocolLabel(protocolId);

            if (packet.ErrorType.Equals(ErrorType.SequenceError)) 
            {
                SequenceNumberLabel.Foreground = Brushes.Red;
            }
            else if (packet.ErrorType.Equals(ErrorType.DataError))
            {
                CargoButton.Background = Brushes.Red;
            }

            SequenceNumberLabel.Content = "Sequence Number: " + packet.SequenceNum;

            var addressArray = packet.Address;
            var finalAddressString = "";

            if (addressArray != null)
            {
                if (addressArray.Length > 1)
                {
                    finalAddressString += "Physical Path: ";
                    for (var i = 0; i < addressArray.Length - 1; i++)
                        finalAddressString += Convert.ToInt32(addressArray[i]) + "  ";
                }
                else
                {
                    finalAddressString = "Logical Address: " + Convert.ToInt32(addressArray[0]);
                }
            }
            else
            {
                finalAddressString = "No Address";
            }

            AddressLabel.Content = finalAddressString;

            LeftArrow.Visibility = _packet.PrevPacket == null 
                ? Visibility.Collapsed 
                : Visibility.Visible;

            RightArrow.Visibility = _packet.NextPacket == null 
                ? Visibility.Collapsed 
                : Visibility.Visible;
        }

        private static Brush GetBrush(bool isError) 
        {
            if (isError)
            {
                return Brushes.Red;
            }
            var converter = new BrushConverter();
            return (Brush)converter.ConvertFromString("#6699ff");
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
            var b = (Button)sender;
            var br = b.Background;

            if (_packet.Cargo != null)
            {
                var cv = new CargoView();
                cv.SetupElements(br, _packet);
                cv.Owner = this;
                cv.Show();
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
            //packetPopup.Owner = this;
            packetPopup.Controller = Controller;
            packetPopup.WindowStartupLocation = WindowStartupLocation.Manual;
        }
        
        private void cmdCopyToClipboard_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(_packet.DateReceived.ToString("dd-MM-yyyy HH:mm:ss.fff"));
        }
    }
}