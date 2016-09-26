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

            var valid = IsValid();
            

            if (valid)
            {
                var cargoByteCounter = 0;
                var noOfColumns = 0;
                MainCargoContent.Text = null;
                int ErrorMessage = 0;

                noOfColumns = int.Parse(ColumnChange.Text);

                if (noOfColumns >= 30)
                {
                    noOfColumns = 30;
                    ColumnChange.Text = "30";
                    ErrorMessage = 1;
                    

                }
                if (noOfColumns < 1)
                {
                    noOfColumns = 10;
                    ColumnChange.Text = "1";
                    ErrorMessage = 2;                   

                }


                foreach (byte cargoByte in _packet.Cargo)// byte
                {

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

                if (ErrorMessage == 1)
                {

                    MessageBox.Show("Maximum number of packets per column is 30");
                    ErrorMessage = 0;
                }
                if (ErrorMessage == 2){

                    MessageBox.Show("Minimum number of packets per column is 1");
                    ErrorMessage = 0;
                }
            }
           
        }

        bool IsValid()
        {

            //checks that the input is a number
            try
            {
                var noOfColumns = int.Parse(ColumnChange.Text);
            }
            catch (Exception) { MessageBox.Show("Invalid integer input"); return false; }
            return true;

        }

        private void ExitButtonEvent(object sender, RoutedEventArgs e)
        {
            Close();
        }

    }
}
