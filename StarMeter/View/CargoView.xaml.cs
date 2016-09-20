﻿using System;
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
        Packet Packet;

        public CargoView()
        {
            InitializeComponent();
        }

        public void SetupElements(Brush brush, Packet p)
        {
            Packet = p; 
            
            foreach (byte cargoByte in p.Cargo)// byte
            {

                MainCargoContent.Text += CRC.ByteToHexString(cargoByte).Substring(2) + "  ";
            }
            CargoData = MainCargoContent.Text;
        }

        public void ChangeColumnEvent(Object sender, RoutedEventArgs e)
        {
            var change = int.Parse(ColumnChange.Text);
            int i = 0;
            MainCargoContent.Text = null;

            foreach (byte cargoByte in Packet.Cargo)// byte
            {
               

                if( i.Equals( change-1))
                {

                    i = 0;
                    MainCargoContent.Text += CRC.ByteToHexString(cargoByte).Substring(2) + Environment.NewLine;
                }
                else
                {
                    i++;
                    MainCargoContent.Text += CRC.ByteToHexString(cargoByte).Substring(2) + "  ";
                }
            }
            CargoData = MainCargoContent.Text;          

        }


        private void ExitButtonEvent(Object sender, RoutedEventArgs e)
        {
            this.Close();
        }

    }
}
