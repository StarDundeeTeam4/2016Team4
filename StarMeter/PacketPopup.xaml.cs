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
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace StarMeter
{
    /// <summary>
    /// Interaction logic for PacketPopup.xaml
    /// </summary>
    public partial class PacketPopup : Page
    {
        public PacketPopup()
        {
            InitializeComponent();
        }

        public void SetupElements(Brush br) 
        {
            TestLabel.Content = "This is where some content goes";
            TestLabel.Foreground = Brushes.White;
            BackgroundColour.Background = Brushes.Blue;
        }

    }
}
