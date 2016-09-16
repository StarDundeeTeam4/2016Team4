using StarMeter.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.DataVisualization.Charting;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace StarMeter.View
{
    /// <summary>
    /// Interaction logic for Statisticspage.xaml
    /// </summary>
    public partial class Statisticspage : Window
    {
        public Statisticspage()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Analyser a = new Analyser();
            double errRate = a.CalculateErrorRate(Parser.PacketDict);

            
            ((PieSeries)mcChart.Series[0]).ItemsSource =
            new KeyValuePair<string, double>[]{
            new KeyValuePair<string, double>("Error", errRate),
            new KeyValuePair<string, double>("Success", 1-errRate) };
        }
    }
}
