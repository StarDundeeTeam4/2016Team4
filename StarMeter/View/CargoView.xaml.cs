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
using System.Windows.Shapes;
using StarMeter.Models;
using StarMeter.Controllers;
namespace StarMeter
{

    public partial class CargoView : Window
    {
        String CargoData;

        public CargoView()
        {
            InitializeComponent();
        }

        public void SetupElements(Brush brush, Packet p)
        {


            foreach (string cargoByte in p.Cargo)// byte
            {
              //  MainCargoContent.Text += Crc.ByteToHexString(cargoByte).Substring(2) + "  ";
            }
            CargoData = MainCargoContent.Text;
        }

        private void ExitButtonEvent(Object sender, RoutedEventArgs e)
        {
            this.Close();
        }

    }
}
