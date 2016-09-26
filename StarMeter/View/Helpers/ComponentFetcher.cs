using StarMeter.Models;
using System;
using System.Windows;
using System.Windows.Controls;

namespace StarMeter.View.Helpers
{
    public class ComponentFetcher
    {
        /// <summary>
        /// Create a label to display a time
        /// </summary>
        /// <param name="time">The time to display</param>
        /// <returns></returns>
        public static Button CreateTimeLabel(DateTime time)
        {
            // create a label for the time
            # region Time label
            var timeLabel = new Button
            {
                Content = time.TimeOfDay.ToString(@"hh\:mm\:ss\.fff")
            };
            timeLabel.SetResourceReference(FrameworkElement.StyleProperty, "Timestamp");
            timeLabel.Tag = time.ToString("dd-MM-yyyy HH:mm:ss.fff");
            #endregion
            return timeLabel;
        }

        /// <summary>
        /// Get a button to represent a packet
        /// </summary>
        /// <param name="packet">The packet to make a button for</param>
        /// <param name="nameToSet">What to set the name as</param>
        /// <returns></returns>
        public static Button GetPacketButton(Packet packet, string nameToSet)
        {
            #region Create Button for the packet
            string packetStatus;
            var packetButton = new Button();
            var nameOutput = nameToSet.Replace('.', 'M').Replace(':', '_');
            var packetLabel = new Label
            {
                FontFamily = new System.Windows.Media.FontFamily("Gill Sans MT")
            };

            try
            {
                var finalAddressString = PacketLabelCreator.GetAddressLabel(packet.Address);
                packetLabel.Content = finalAddressString;

                var protocolId = packet.ProtocolId;
                packetLabel.Content += Environment.NewLine + PacketLabelCreator.GetProtocolLabel(protocolId);
            }
            catch (Exception)
            {
                packetLabel.Content = "Unknown Packet Type";
            }

            try
            {
                packetButton.Tag = packet.PacketId;
            }
            catch (Exception)
            {
                packetButton.Tag = "";
            }

            packetButton.Content = packetLabel;

            try
            {
                packetStatus = packet.IsError 
                    ? "Error" 
                    : "Success";
            }
            catch (Exception)
            {
                packetStatus = "Error";
            }

            packetButton.SetResourceReference(FrameworkElement.StyleProperty, packetStatus);
            packetButton.Name = "btn" + nameOutput;
            return packetButton;
            #endregion
        }
    }
}
