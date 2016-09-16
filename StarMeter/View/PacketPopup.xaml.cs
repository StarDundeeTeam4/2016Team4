using StarMeter.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.DataVisualization.Charting;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace StarMeter.View
{
    /// <summary>
    /// Interaction logic for PacketPopup.xaml
    /// </summary>
    public partial class PacketPopup
    {
        public PacketPopup()
        {
            InitializeComponent();
        }
        Packet _p;
        public void SetupElements(Brush br, Packet p) 
        {
            _p = p;
            this.Width = 500;
            this.Height = 500;

            lblErrorMsg.Background = br;

            BitmapImage logo = new BitmapImage();
            logo.BeginInit();


            var converter = new System.Windows.Media.BrushConverter();
            IconBG.Background = (Brush)converter.ConvertFromString("#6699ff");

            logo.UriSource = new Uri("pack://application:,,,/Resources/tick.png");
            logo.EndInit();

            lblErrorMsg.Content = "SUCCESS";
           

            ErrorIcon.Source = logo;   
            
            TimeLabel.Content = p.DateRecieved.ToString();


            var protocol_id = 1;

            if (protocol_id == 1)
            {
                ProtocolLabel.Content = ("Protocol: " + (protocol_id).ToString() + " (RMAP)");
            }
            else
            {
                ProtocolLabel.Content = ("Protocol: " + (protocol_id).ToString());
            }

            var addressArray = p.Address;
            var finalAddressString = "";
            if (addressArray.Length > 1)
            {
                finalAddressString += "Physical Path: ";
                for (var i = 0; i < addressArray.Length - 1; i++)
                    finalAddressString +=  Convert.ToInt32(addressArray[i]) + "  ";
            }
            else
                finalAddressString = "Logical Address: "+Convert.ToInt32(addressArray[0]).ToString();

            AddressLabel.Content = finalAddressString;

         }

        private void ViewCargo(object sender, RoutedEventArgs e)
        {
            var b = (Button)sender;
            var br = b.Background;
                             
            CargoView cv = new CargoView();
            cv.SetupElements(br, _p); 
            cv.Show();

            
        }

        private void ExitButtonEvent(Object sender, RoutedEventArgs e)
        {
            this.Close();
        }

    }
}
