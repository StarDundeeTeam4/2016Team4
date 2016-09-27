using System;
using StarMeter.Models;

namespace StarMeter.View.Helpers
{
    public class PacketLabelCreator
    {
        public static string GetAddressLabel(byte[] packetAddress)
        {
            var finalAddressString = "";
            if (packetAddress != null)
            {
                if (packetAddress.Length > 1)
                {
                    finalAddressString += "Physical Path: ";
                    for (var i = 0; i < packetAddress.Length - 1; i++)
                    {
                        finalAddressString += Convert.ToInt32(packetAddress[i]) + "  ";
                    }
                }
                else
                {
                    finalAddressString = Convert.ToInt32(packetAddress[0]).ToString();
                }
            }
            else
            {
                finalAddressString = "No Address";
            }
            return finalAddressString;
        }

        public static string GetProtocolLabel(int protocolId)
        {
            if (protocolId == 1)
            {
                return "Protocol: " + protocolId + " (RMAP)";
            }
            return "Protocol: " + protocolId;
        }

        public static string GetSourcePathAddress(RmapPacket packet)
        {
            string sourcePathAdd = null;
            for (var i = 0; i < packet.SecondaryAddress.Length - 1; i++)
            {
                sourcePathAdd += Convert.ToInt32(packet.SecondaryAddress[i]) + "  ";
            }
            return sourcePathAdd;
        }
    }
}
