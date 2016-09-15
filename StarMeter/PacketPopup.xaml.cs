using System;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace StarMeter
{
    /// <summary>
    /// Interaction logic for PacketPopup.xaml
    /// </summary>
    public partial class PacketPopup : Window
    {
        public PacketPopup()
        {
            InitializeComponent();
        }

        public void SetupElements(Brush br) 
        {

            this.Width = 500;
            this.Height = 500;

            lblErrorMsg.Background = br;

            BitmapImage logo = new BitmapImage();
            logo.BeginInit();

            // change this to check the Packet error type - Pass a Packet object
            if (br.Equals(Brushes.Red))
            {
                Image finalImage = new Image();
                finalImage.Width = 80;
                
                logo.UriSource = new Uri("pack://application:,,,/Resources/Error.png");
                IconBG.Background = Brushes.Red;
                logo.EndInit();
               
                lblErrorMsg.Content = "ERROR: " + "error type";
            }
            else
            {
                IconBG.Background = Brushes.Blue;
                logo.UriSource = new Uri("pack://application:,,,/Resources/tick.png");
                logo.EndInit();

                lblErrorMsg.Content = "SUCCESS";
            }
            ErrorIcon.Source = logo;    // ,,,,, chameleon

        }

    }
}
