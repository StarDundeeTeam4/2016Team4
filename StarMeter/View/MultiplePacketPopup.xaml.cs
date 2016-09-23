using StarMeter.Controllers;
using StarMeter.Models;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace StarMeter.View
{
    /// <summary>
    /// Interaction logic for MultiplePacketPopup.xaml
    /// </summary>
    public partial class MultiplePacketPopup
    {
        public Controller Controller;

        public MultiplePacketPopup(Controller c)
        {
            InitializeComponent();
            Controller = c;
        }

        public void CreateElements(List<Packet> packets) 
        {
            foreach(var p in packets)
            {
                var btn = GetPacketButton(p);
                btn.Margin = new Thickness(5,2.5,5,3);
                btn.Height = 50;
                PacketList.Children.Add(btn);
            }
        }

        public Button GetPacketButton(Packet p)
        {
            #region Create Button for the packet
            string sty = "";

            var b = new Button();
            b.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            b.HorizontalContentAlignment = System.Windows.HorizontalAlignment.Center;
            b.Click += OpenPopup;
            
            var lab = new Label();
            lab.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            lab.HorizontalContentAlignment = System.Windows.HorizontalAlignment.Center;
            lab.FontFamily = new System.Windows.Media.FontFamily("Gill Sans MT");

            try
            {
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
                        finalAddressString = Convert.ToInt32(addressArray[0]).ToString();
                }
                else
                {
                    finalAddressString = "No Address";
                }

                lab.Content = finalAddressString; 
                
                var protocolId = p.ProtocolId;

                if (protocolId == 1)
                {
                    lab.Content = (lab.Content) + Environment.NewLine + "Protocol: " + protocolId + " (RMAP)";
                }
                else
                {
                    lab.Content = (lab.Content) + Environment.NewLine + "Protocol: " + protocolId;
                }
            }
            catch (Exception e)
            {
                lab.Content = "Unknown Packet Type";
            }

            try
            {
                b.Tag = p.PacketId;
            }
            catch (Exception)
            {
                b.Tag = "";
            }

            b.Content = lab;

            try
            {
                sty = p.IsError ? "Error" : "Success";
            }
            catch (Exception)
            {
                sty = "Error";
            }

            b.SetResourceReference(Control.StyleProperty, sty);
            return b;
            #endregion
        }

        public void OpenPopup(object sender, RoutedEventArgs e)
        {
            var b = (Button)sender;

            var text = b.Tag.ToString();
            var guid = new Guid(text);

            PacketPopup pp = new PacketPopup();
            pp.Controller = Controller;

            Packet p = Controller.FindPacket(guid);

            if (p != null)
            {
                pp.SetupElements(p); // send the packet as a parameter, along with the colour to make the header
                pp.Owner = this;
                pp.Show();
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

    }
}
