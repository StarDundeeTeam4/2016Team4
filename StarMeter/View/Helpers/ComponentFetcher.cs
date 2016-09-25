using StarMeter.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        public static Label CreateTimeLabel(TimeSpan time)
        {
            // create a label for the time
            # region Time label
            Label l = new Label();
            l.Content = time.ToString(@"hh\:mm\:ss\.fff");
            l.SetResourceReference(Control.StyleProperty, "Timestamp");
            #endregion

            return l;
        }

        /// <summary>
        /// Get a button to represent a packet
        /// </summary>
        /// <param name="p">The packet to make a button for</param>
        /// <param name="nameToSet">What to set the name as</param>
        /// <returns></returns>
        public static Button GetPacketButton(Packet p, string nameToSet)
        {
            #region Create Button for the packet
            string sty = "";

            var b = new Button();
            string nameOutput = nameToSet.Replace('.', 'M').Replace(':', '_');

            var lab = new Label();

            lab.FontFamily = new System.Windows.Media.FontFamily("Gill Sans MT");

            try
            {

                var addressArray = p.Address;
                var finalAddressString = "";

                if (addressArray != null)
                {
                    if (addressArray.Length > 1)
                    {
                        finalAddressString += "Path: ";
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
                    lab.Content = (lab.Content) + Environment.NewLine + "P: " + protocolId + " (RMAP)";
                }
                else
                {
                    lab.Content = (lab.Content) + Environment.NewLine + "P: " + protocolId;
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

            b.Name = "btn" + nameOutput;


            return b;
            #endregion
        }
    }
}
