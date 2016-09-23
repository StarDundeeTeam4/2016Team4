using StarMeter.Controllers;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.DataVisualization;
using System.Windows.Controls.DataVisualization.Charting;
using System.Windows.Controls.DataVisualization.Charting.Primitives;
using System.Windows.Media;

namespace StarMeter.View
{
    /// <summary>
    /// Interaction logic for Statisticspage.xaml
    /// </summary>
    public partial class Statisticspage
    {
        public Statisticspage()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Analyser a = new Analyser();
            double errRate = .5;
            
            ((PieSeries)mcChart.Series[0]).ItemsSource =
            new KeyValuePair<string, double>[]{
            new KeyValuePair<string, double>("Error", errRate),
            new KeyValuePair<string, double>("Success", 1-errRate) };

            mcChart.Series[0].LegendItems.Clear();

            Style style = new Style(typeof(Chart));
            Setter st1 = new Setter(Chart.BackgroundProperty,
                                        new SolidColorBrush(Colors.Red));
            Setter st4 = new Setter(Chart.ForegroundProperty,
                                        new SolidColorBrush(Colors.Red));
            Setter st2 = new Setter(Chart.BorderBrushProperty,
                                        new SolidColorBrush(Colors.White));
            Setter st3 = new Setter(Chart.BorderThicknessProperty, new Thickness(0));

            style.Setters.Add(st1);
            style.Setters.Add(st2);
            style.Setters.Add(st3);
            style.Setters.Add(st4);

            mcChart.Style = style;

            //mcChart.ChartAreaStyle = style;

            EdgePanel ep = ObjectFinder.FindChild<EdgePanel>(mcChart, "ChartArea");
            if (ep != null)
            {
                var grid = ep.Children.OfType<Grid>().FirstOrDefault();
                if (grid != null)
                {
                    grid.Background = new SolidColorBrush(Colors.Transparent);
                }

                var border = ep.Children.OfType<Border>().FirstOrDefault();
                if (border != null)
                {
                    border.BorderBrush = new SolidColorBrush(Colors.Transparent);
                }
            }

            DataPoint point = ObjectFinder.FindChild<DataPoint>(mcChart, "DataPoint");
            if (point != null)
            {
                point.Background = new SolidColorBrush(Colors.Red);
                point.BorderBrush = new SolidColorBrush(Colors.Blue);
            }

            Legend legend = ObjectFinder.FindChild<Legend>(mcChart, "Legend");
            if (legend != null)
            {
                legend.Foreground = new SolidColorBrush(Colors.White);
                legend.Background = new SolidColorBrush(Colors.Transparent);
                legend.BorderBrush = new SolidColorBrush(Colors.Transparent);
            }

            ResourceDictionaryCollection pieSeriesPalette = new ResourceDictionaryCollection();

            Brush currentBrush = new SolidColorBrush(Color.FromRgb(20, 200, 20)); //Green
            Brush currentBrush2 = new SolidColorBrush(Color.FromRgb(200, 20, 20)); //Red

            ResourceDictionary pieDataPointStyles = new ResourceDictionary();
            Style stylePie = new Style(typeof(PieDataPoint));
            stylePie.Setters.Add(new Setter(BackgroundProperty, currentBrush));
            pieDataPointStyles.Add("DataPointStyle", stylePie);

            ResourceDictionary pieDataPointStyles2 = new ResourceDictionary();
            Style stylePie2 = new Style(typeof(PieDataPoint));
            stylePie2.Setters.Add(new Setter(BackgroundProperty, currentBrush2));
            pieDataPointStyles2.Add("DataPointStyle", stylePie2);

            pieSeriesPalette.Add(pieDataPointStyles2);
            pieSeriesPalette.Add(pieDataPointStyles);

            mcChart.Palette = pieSeriesPalette;
        }

        private static Style GetNewDataPointStyle()
        {
            Color background = Color.FromRgb(20, 20, 20);
            Style style = new Style(typeof(DataPoint));
            Setter st1 = new Setter(BackgroundProperty,
                                        new SolidColorBrush(background));
            Setter st2 = new Setter(BorderBrushProperty,
                                        new SolidColorBrush(Colors.White));
            Setter st3 = new Setter(BorderThicknessProperty, new Thickness(0.1));

            Setter st4 = new Setter(TemplateProperty, null);
            style.Setters.Add(st1);
            style.Setters.Add(st2);
            style.Setters.Add(st3);
            style.Setters.Add(st4);
            return style;
        }
    }
}
