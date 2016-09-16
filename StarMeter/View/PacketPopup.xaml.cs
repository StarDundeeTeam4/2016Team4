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

        public void SetupElements(Brush br, Packet p) 
        {

            this.Width = 500;
            this.Height = 500;

            lblErrorMsg.Background = br;

            BitmapImage logo = new BitmapImage();
            logo.BeginInit();


            //if (p.IsError)
            //{

            //    logo.UriSource = new Uri("pack://application:,,,/Resources/Error.png");
            //    IconBG.Background = Brushes.Red;
            //    logo.EndInit();
               
            //    lblErrorMsg.Content = "ERROR: " + p.ErrorType;
            //}
            //else
            //{
                var converter = new System.Windows.Media.BrushConverter();
                IconBG.Background = (Brush)converter.ConvertFromString("#6699ff");

                logo.UriSource = new Uri("pack://application:,,,/Resources/tick.png");
                logo.EndInit();

                lblErrorMsg.Content = "SUCCESS";
            //}

            ErrorIcon.Source = logo;   
            
            TimeLabel.Content = p.DateRecieved.ToString();


            var protocol_id = 1;//p.GetProtocolID();

            if (protocol_id == 1)
            {
                ProtocolLabel.Content = ("Protocol: " + (protocol_id).ToString() + " (RMAP)");
            }
            else
            {
                ProtocolLabel.Content = ("Protocol: " + (protocol_id).ToString());
            }
            
            

            // SequenceNumberLabel.Content = ???;

        }

    }
}
