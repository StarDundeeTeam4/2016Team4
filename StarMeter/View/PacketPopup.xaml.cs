using StarMeter.Models;
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
﻿using System;
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
        Packet _p;
        Brush _br;
        public Controller controller;

        public void SetupElements(Brush br, Packet p) 
        {
            _p = p;
            _br = br;
            this.Width = 500;
            this.Height = 500;

            lblErrorMsg.Background = br;

            BitmapImage logo = new BitmapImage();
            logo.BeginInit();


            var converter = new System.Windows.Media.BrushConverter();


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

                lblErrorMsg.Content = "ERROR";
            }


            IconBG.Background = br;
            ErrorIcon.Source = logo;   
            
            TimeLabel.Content = p.DateRecieved.ToString("dd-MM-yyyy HH:mm:ss.fff");

            
            // get protocol id
            //int protocol_id = Parser.GetProtocolId(p.Cargo.ToString(), 0);

            int protocol_id = 1;

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

            if (addressArray != null)
            {
                if (addressArray.Length > 1)
                {
                    finalAddressString += "Physical Path: ";
                    for (var i = 0; i < addressArray.Length - 1; i++)
                        finalAddressString += Convert.ToInt32(addressArray[i]) + "  ";
                }
                else
                    finalAddressString = "Logical Address: " + Convert.ToInt32(addressArray[0]).ToString();
            }
            else 
            {
                finalAddressString = "No Address";
            }

            AddressLabel.Content = finalAddressString;

            LeftArrow.Visibility = _p.PrevPacket == null ? Visibility.Collapsed : Visibility.Visible;
            RightArrow.Visibility = _p.NextPacket == null ? Visibility.Collapsed : Visibility.Visible;
        }

        private void nextPacket(object sender, RoutedEventArgs e)
        {
            if (_p.NextPacket != null)
            {
                Packet p = controller.FindPacket(_p.NextPacket.GetValueOrDefault());
                SetupElements(_br, p);
            }
        }

        private void prevPacket(object sender, RoutedEventArgs e)
        {
            if (_p.PrevPacket != null)
            {
                Packet p = controller.FindPacket(_p.PrevPacket.GetValueOrDefault());
                SetupElements(_br, p);
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
        }

    }
}

