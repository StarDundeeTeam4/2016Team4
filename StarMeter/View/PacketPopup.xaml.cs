using StarMeter.Models;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using StarMeter.Controllers;

namespace StarMeter.View
{
    public partial class PacketPopup
    {
        public PacketPopup()
        {
            InitializeComponent();
        }

        private Packet _p;
        public Controller Controller;

        public void SetupElements(Packet p)
        {
            if (!(p is RmapPacket))
            {
                ViewRmapPropertiesButton.Visibility = Visibility.Hidden;
            }
            Brush br = GetBrush(p.IsError);

            _p = p;
            Width = 500;
            Height = 500;

            lblErrorMsg.Background = br;

            BitmapImage logo = new BitmapImage();
            logo.BeginInit();

            if (!p.IsError)
            {
                logo.UriSource = new Uri("pack://application:,,,/Resources/tick.png");
                logo.EndInit();
                lblErrorMsg.Content = "SUCCESS";
            }
            else
            {

                logo.UriSource = new Uri("pack://application:,,,/Resources/Error.png");
                logo.EndInit();
                lblErrorMsg.Content = "ERROR: " + p.ErrorType;
            }

            IconBG.Background = br;
            ErrorIcon.Source = logo;
            TimeLabel.Content = p.DateRecieved.ToString("dd-MM-yyyy HH:mm:ss.fff");
            var protocolId = p.ProtocolId;

            if (protocolId == 1)
            {
                ProtocolLabel.Content = "Protocol: " + protocolId + " (RMAP)";
            }
            else
            {
                ProtocolLabel.Content = "Protocol: " + protocolId;
            }


            if (p.ErrorType.Equals(ErrorType.SequenceError)) 
            {
                SequenceNumberLabel.Foreground = Brushes.Red;
            }
            else if (p.ErrorType.Equals(ErrorType.DataError))
            {
                CargoButton.Background = Brushes.Red;
            }

            SequenceNumberLabel.Content = "Sequence Number: " + p.SequenceNum;

            var addressArray = p.Address;
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
                    finalAddressString = "Logical Address: " + Convert.ToInt32(addressArray[0]);
            }
            else
            {
                finalAddressString = "No Address";
            }

            AddressLabel.Content = finalAddressString;

            LeftArrow.Visibility = _p.PrevPacket == null 
                ? Visibility.Collapsed 
                : Visibility.Visible;

            RightArrow.Visibility = _p.NextPacket == null 
                ? Visibility.Collapsed 
                : Visibility.Visible;
        }

        private Brush GetBrush(bool isError) 
        {
            if (isError)
            {
                return Brushes.Red;
            }
            else
            {
                var converter = new System.Windows.Media.BrushConverter();
               return (Brush)converter.ConvertFromString("#6699ff");
            }
        }

        private void NextPacket(object sender, RoutedEventArgs e)
        {
            if (_p.NextPacket != null)
            {
                Packet p = Controller.FindPacket(_p.NextPacket.GetValueOrDefault());
                SetupElements(p);
            }
        }

        private void PrevPacket(object sender, RoutedEventArgs e)
        {
            if (_p.PrevPacket != null)
            {
                Packet p = Controller.FindPacket(_p.PrevPacket.GetValueOrDefault());
                SetupElements(p);
            }
        }

        private void ViewCargo(object sender, RoutedEventArgs e)
        {
            var b = (Button)sender;
            var br = b.Background;

            if (_p.Cargo != null)
            {
                CargoView cv = new CargoView();
                cv.SetupElements(br, _p);
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
            this.Close();
            this.Owner.Show();
        }

        private void ShowRmapProperties(object sender, RoutedEventArgs e)
        {
            var b = (Button) sender;
            var br = b.Background;

            var cv = new RmapView();
            cv.SetupElements(br, (RmapPacket)_p);
            cv.Owner = this;
            cv.Show();
        }

        private void LeftArrow_MouseRightButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            PacketPopup pp = new PacketPopup(); 
            
            Packet p = Controller.FindPacket(_p.PrevPacket.GetValueOrDefault());
            pp.SetupElements(p);
            pp.Owner = this;
            pp.Controller = this.Controller;
            pp.WindowStartupLocation = System.Windows.WindowStartupLocation.Manual;
            pp.Left = 0;

            pp.Top = (SystemParameters.VirtualScreenHeight / 2) - 250;

            pp.Show();
        }

        private void RightArrow_MouseRightButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            PacketPopup pp = new PacketPopup();

            Packet p = Controller.FindPacket(_p.NextPacket.GetValueOrDefault());
            pp.SetupElements(p);
            pp.Owner = this;
            pp.Controller = this.Controller;
            pp.WindowStartupLocation = System.Windows.WindowStartupLocation.Manual;
            pp.Left = SystemParameters.VirtualScreenWidth - 500;

            pp.Top = (SystemParameters.VirtualScreenHeight / 2) - 250;
            pp.Show();
        }
    }
}