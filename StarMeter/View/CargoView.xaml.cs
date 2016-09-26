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
                MainCargoContent.Text += Crc.ByteToHexString(cargoByte).Substring(2) + "  ";
            }
        }

        public void ChangeColumnEvent(object sender, RoutedEventArgs e)
        {
            if (IsValid())
            {
                var cargoByteCounter = 0;
                MainCargoContent.Text = null;
                var errorMessage = 0;
                var noOfColumns = int.Parse(ColumnChange.Text);

                if (noOfColumns >= 30)
                {
                    noOfColumns = 30;
                    ColumnChange.Text = "30";
                    errorMessage = 1;
                }
                if (noOfColumns < 1)
                {
                    noOfColumns = 10;
                    ColumnChange.Text = "1";
                    errorMessage = 2;                   
                }

                foreach (var cargoByte in _packet.Cargo)// byte
                {
                    if (cargoByteCounter.Equals(noOfColumns - 1))
                    {
                        cargoByteCounter = 0;
                        MainCargoContent.Text += Crc.ByteToHexString(cargoByte).Substring(2) + Environment.NewLine;
                    }
                    else
                    {
                        cargoByteCounter++;
                        MainCargoContent.Text += Crc.ByteToHexString(cargoByte).Substring(2) + "  ";
                    }
                }

                if (errorMessage == 1)
                {
                    MessageBox.Show("Maximum number of packets per column is 30");
                    errorMessage = 0;
                }
                if (errorMessage == 2){

                    MessageBox.Show("Minimum number of packets per column is 1");
                }
            }
        }

        public bool IsValid()
        {
            //checks that the input is a number
            try
            {
                var noOfColumns = int.Parse(ColumnChange.Text);
            }
            catch (Exception)
            {
                MessageBox.Show("Invalid integer input"); return false;
            }
            return true;
        }

        private void ExitButtonEvent(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
