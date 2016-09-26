using StarMeter.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StarMeter.View.Helpers
{
    public class PageFetcher
    {
        /// <summary>
        /// Try to get the first 100 packets - to display. If not 100 available, just get as many as possible
        /// </summary>
        /// <param name="allPackets">The list of packets from which to get a page</param>
        /// <returns></returns>
        public static Packet[] FetchPage(List<Packet> allPackets)
        {
            var page = MainWindow.PageIndex;    // which page we need the data from (0 = first 100 packets, 1 = second 100 packets etc)
            try
            {
                return allPackets.GetRange(100 * page, 100).ToArray();
            }
            catch (Exception)
            {
                return allPackets.ToList().GetRange(100 * page, allPackets.Count - 100 * page).ToArray();
            }
        }
    }
}
