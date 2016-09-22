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
        private readonly Controller _controller;

        public MultiplePacketPopup(Controller c)
        {
            InitializeComponent();
            _controller = c;
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
            b.Click += OpenPopup;
            
            var lab = new Label();

            try
            {
                lab.Content = p.DateRecieved.ToString("HH:mm:ss.fff");
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
            pp.Controller = _controller;

            Packet p = _controller.FindPacket(guid);

            if (p != null)
            {
                pp.SetupElements(p); // send the packet as a parameter, along with the colour to make the header
                pp.Owner = this;
                pp.Show();
            }
        }

    }
}
