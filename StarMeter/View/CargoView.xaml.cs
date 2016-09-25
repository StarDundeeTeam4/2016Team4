using System;
using System.Windows;
using System.Windows.Media;
using StarMeter.Models;
using StarMeter.Controllers;
namespace StarMeter.View
{
    public partial class CargoView
    {
        private Packet _packet;

        public CargoView()
        {
            InitializeComponent();
        }

        public void SetupElements(Brush brush, Packet packet)
        {
            _packet = packet;
            foreach (var cargoByte in packet.Cargo)
            {
                MainCargoContent.Text += CRC.ByteToHexString(cargoByte).Substring(2) + "  ";
            }
        }

        public void ChangeColumnEvent(Object sender, RoutedEventArgs e)
        {
            var noOfColumns = int.Parse(ColumnChange.Text);
            var cargoByteCounter = 0;
            MainCargoContent.Text = null;
            try
            {
                foreach (byte cargoByte in _packet.Cargo)// byte
                {
                    if (noOfColumns >= 30)
                    {
                        noOfColumns = 30;
                        ColumnChange.Text = "30";

                        MessageBox.Show("Maximum number of packets per column is 30");

                    }
                    if (cargoByteCounter.Equals(noOfColumns - 1))
                    {
                        cargoByteCounter = 0;
                        MainCargoContent.Text += CRC.ByteToHexString(cargoByte).Substring(2) + Environment.NewLine;
                    }
                    else
                    {
                        cargoByteCounter++;
                        MainCargoContent.Text += CRC.ByteToHexString(cargoByte).Substring(2) + "  ";
                    }
                }
            }
            catch (Exception)
            {
            //Handle exception
            }
        }

        private void ExitButtonEvent(object sender, RoutedEventArgs e)
        {
            Close();
        }

    }
}
