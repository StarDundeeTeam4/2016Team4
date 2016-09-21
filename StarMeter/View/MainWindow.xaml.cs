using Microsoft.Win32;
using StarMeter.Models;
using System;
using System.Collections.Generic;
using System.Windows.Controls.DataVisualization.Charting;
using StarMeter.Controllers;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.DataVisualization;
using System.Windows.Controls.DataVisualization.Charting.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Threading;


namespace StarMeter.View
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private readonly Controller controller = new Controller();

        private StackPanel[] _portStacks = new StackPanel[8];
        public MainWindow()
        {
            InitializeComponent();

            // initialise the stack array
            _portStacks[0] = Port1AHolder;
            _portStacks[1] = Port1BHolder;
            _portStacks[2] = Port2AHolder;
            _portStacks[3] = Port2BHolder;
            _portStacks[4] = Port3AHolder;
            _portStacks[5] = Port3BHolder;
            _portStacks[6] = Port4AHolder;
            _portStacks[7] = Port4BHolder;

            _gifDecoder = new GifBitmapDecoder(new Uri("pack://application:,,,/Resources/rocket.gif"), BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.Default);
            LoadingIcon.Source = _gifDecoder.Frames[0];

            _LoadingTimer = new System.Timers.Timer();
            _LoadingTimer.Elapsed += _LoadingTimer_Elapsed;
            _LoadingTimer.Interval = 100;

            _LoadingTimer.Start();
        }


        void ChangeAnimFrame() 
        {
            if (animCount >= _gifDecoder.Frames.Count - 1) 
            {
                animCount = 0;
            }

            LoadingIcon.Source = _gifDecoder.Frames[animCount];

        }

        void _LoadingTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e) 
        {
            animCount++;

            LoadingIcon.Dispatcher.Invoke(new UpdateAnimation(ChangeAnimFrame));
       

        }

        private GifBitmapDecoder _gifDecoder;
        int animCount = 0;

        // needed for drawing rectangle - for zooming
        private bool _mouseDown; 
        private Point _mouseDownPos;

        // TODO - Find reference to this stuff
        #region Drag Rectangle Methods 
        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            // Capture and track the mouse.
            _mouseDown = true;
            _mouseDownPos = e.GetPosition(MainGrid);
            MainGrid.CaptureMouse();

            // Initial placement of the drag selection box.         
            Canvas.SetLeft(selectionBox, _mouseDownPos.X);
            Canvas.SetTop(selectionBox, _mouseDownPos.Y);
            selectionBox.Width = 0;
            selectionBox.Height = 0;

            // Make the drag selection box visible.
            selectionBox.Visibility = Visibility.Visible;
        }

        private void Grid_MouseUp(object sender, MouseButtonEventArgs e)
        {
            // Release the mouse capture and stop tracking it.
            _mouseDown = false;
            MainGrid.ReleaseMouseCapture();

            // Hide the drag selection box.
            selectionBox.Visibility = Visibility.Collapsed;

            var s = new Size(selectionBox.Width, selectionBox.Height);

            FiltersHeading.Content = s.ToString();

            // TODO: 
            //
            // The mouse has been released, check to see if any of the items 
            // in the other canvas are contained within mouseDownPos and 
            // mouseUpPos, for any that are, select them!
            //
        }

        private void Grid_MouseMove(object sender, MouseEventArgs e)
        {
            if (!_mouseDown) return;
            // When the mouse is held down, reposition the drag selection box.

            var mousePos = e.GetPosition(MainGrid);

            if (_mouseDownPos.X < mousePos.X)
            {
                Canvas.SetLeft(selectionBox, _mouseDownPos.X);
                selectionBox.Width = mousePos.X - _mouseDownPos.X;
            }
            else
            {
                Canvas.SetLeft(selectionBox, mousePos.X);
                selectionBox.Width = _mouseDownPos.X - mousePos.X;
            }

            if (_mouseDownPos.Y < mousePos.Y)
            {
                Canvas.SetTop(selectionBox, _mouseDownPos.Y);
                selectionBox.Height = mousePos.Y - _mouseDownPos.Y;
            }
            else
            {
                Canvas.SetTop(selectionBox, mousePos.Y);
                selectionBox.Height = _mouseDownPos.Y - mousePos.Y;
            }
        }
        #endregion


        System.Timers.Timer _LoadingTimer;


        void CreateTimeLabel(TimeSpan time) 
        {
            // create a label for the time
            # region Time label
            Label l = new Label();
            l.Content = time.ToString();
            l.SetResourceReference(Control.StyleProperty, "Timestamp");

            TimeList.Children.Add(l);
            #endregion
        }

        private Button GetPacketButton(Packet p) 
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

        // work out which panel to assign the packet to
        StackPanel GetPanelToUse(int portNum) 
        {
            return _portStacks[portNum - 1];
        }


        void AddPacketCollection(Packet[] packets) 
        {

            foreach (Packet p in packets) 
            {
                if (_previous[p.PortNumber - 1] == new TimeSpan()) 
                {
                    _previous[p.PortNumber - 1] = p.DateRecieved.TimeOfDay;
                }
            }
            

            foreach (var p in packets) 
            {
                AddPacket(p);
            }

            int start, end, total;

            total = controller.packets.Count;
            start = (pageIndex * 100) + 1;
            end = start + 99;

            if (end > total) 
            {
                end = total;
            }


            lblNumShowing.Content = "Showing " + start + " - "  + end + " of " + total + " packets";

        }

        private TimeSpan[] _previous = new TimeSpan[8];

        private void AddPacket(Packet p) 
        {
            var temp_timespans = _timespans.ToList();
            var b = GetPacketButton(p);
            var packet_timespan = p.DateRecieved.TimeOfDay;
            var sp = GetPanelToUse(p.PortNumber);

            bool found = false;

            int index = 0;

            while(found == false && temp_timespans.Count > 0)
            {
                index = temp_timespans.Count/2;
                

                if (temp_timespans[index] >= packet_timespan)
                {
                    if ((temp_timespans[index].Add(half_section) < packet_timespan))
                    {
                        found = true;
                    }
                    else
                    {
                        temp_timespans = temp_timespans.GetRange(0, index);
                        //index = index / 2;
                    }
                }
                else
                {
                    if ((temp_timespans[index].Add(section) > packet_timespan))
                    {
                        found = true;
                    }
                    else
                    {
                        temp_timespans = temp_timespans.GetRange(index, temp_timespans.Count - index);
                        //index = index + (index / 2);
                    }
                }
            }

            //This line will let us know how many empty spaces that we need to add to the stack panel.

        
            var diff = (temp_timespans[index] - _previous[p.PortNumber - 1]);
            int spaces = 0;



            while(diff.CompareTo(new TimeSpan(0,0,0,0,interval)) > 0)
            {
                diff = diff.Add(negative_section);
                spaces++;
            }


            Console.WriteLine();


            for (int i = 0; i < spaces; i++)
            {
                Label lbl = new Label();
                lbl.SetResourceReference(Control.StyleProperty, "TimeFiller");
                sp.Children.Add(lbl);
            }

            //b.Margin = new Thickness(0, 0, 0, ((HeightScroller.Value * spaces) + ((1 + spaces) * HeightScroller.Value)) + ((HeightScroller.Value) / 10));
            sp.Children.Add(b);



            _previous[p.PortNumber - 1] = temp_timespans[index];


            

        }

        //This function will remove all packets from the screen which are being displayed.
        void RemoveAllPackets()
        {
            for (int i = 0; i < 8; i++ )
            {
                var childElements = _portStacks[i].Children;

                while (childElements.Count > 0)
                {
                    childElements.Remove((UIElement)childElements[0]);
                }
            }

            var timeElements = TimeList.Children;

            while (timeElements.Count > 0)
            {
                timeElements.Remove((UIElement)timeElements[0]);
            }
        }
        #region TEMP

        void CreateDataRateGraph(Packet[] packets) 
        {
            RatesLineChart.Series.Clear();
            RatesLineChart.DataContext = null;

            Analyser a = new Analyser();
            var values = a.GetDataForLineChart(packets);


            if (!(bool)ChkErrorsOnly.IsChecked)
            {
                var lineSeries1 = new LineSeries
                {
                    Title = "Data Rate",
                    Foreground = Brushes.White,
                    DependentValuePath = "Value",
                    IndependentValuePath = "Key",
                    ItemsSource = values[0]
                };
                RatesLineChart.Series.Add(lineSeries1);
            }

            var lineSeriesError = new LineSeries
            {
                Title = "Error Rate",
                Foreground = Brushes.Black,
                DependentValuePath = "Value",
                IndependentValuePath = "Key",
                ItemsSource = values[1]
            };
            RatesLineChart.Series.Add(lineSeriesError);

            RatesLineChart.DataContext = values;


            Legend legend = ObjectFinder.FindChild<Legend>(RatesLineChart, "Legend");
            if (legend != null)
            {
                legend.Foreground = new SolidColorBrush(Colors.White);
                legend.Background = new SolidColorBrush(Colors.Transparent);
                legend.BorderBrush = new SolidColorBrush(Colors.Transparent);
            }


            System.Windows.Controls.DataVisualization.ResourceDictionaryCollection lineSeriesPalette = new System.Windows.Controls.DataVisualization.ResourceDictionaryCollection();

            Brush currentBrush = new SolidColorBrush(Color.FromRgb(20, 200, 20)); //Green
            Brush currentBrush2 = new SolidColorBrush(Color.FromRgb(200, 20, 20)); //Red


            System.Windows.ResourceDictionary pieDataPointStyles2 = new ResourceDictionary();
            Style stylePie2 = new Style(typeof(LineDataPoint));
            stylePie2.Setters.Add(new Setter(LineDataPoint.BackgroundProperty, currentBrush2));
            pieDataPointStyles2.Add("DataPointStyle", stylePie2);

            if (!(bool)ChkErrorsOnly.IsChecked)
            {
                System.Windows.ResourceDictionary pieDataPointStyles = new ResourceDictionary();
                Style stylePie = new Style(typeof(LineDataPoint));
                stylePie.Setters.Add(new Setter(LineDataPoint.BackgroundProperty, currentBrush));
                pieDataPointStyles.Add("DataPointStyle", stylePie);
                lineSeriesPalette.Add(pieDataPointStyles);
            }

            lineSeriesPalette.Add(pieDataPointStyles2);

            RatesLineChart.Palette = lineSeriesPalette;


        }

        private void TestTimeCreation(object sender, RoutedEventArgs e) 
        {
            //AddPacketCollection(packets);

            //CreateDataRateGraph();         

            //ScrollBackToBottom();


            Statisticspage sp = new Statisticspage();
            sp.Show();

        }
        #endregion
        

        //This will allow us to read the files or remove the files later.
        private readonly List<Grid> _fileGrids = new List<Grid>();

        private void FileSelection(object sender, RoutedEventArgs e) 
        {
            var ofd = new OpenFileDialog
            {
                // only allow .rec files
                Filter = "Record Files (.rec)|*.rec",
                Multiselect = true
            };

            bool? confirmed = ofd.ShowDialog();

            if (confirmed != true) return;

            // display file name
            List<string> filesAdded = controller.AddFileNames(ofd.FileNames);
                
            foreach (string fileName in filesAdded)
            {
                string actualName = fileName.Split('.')[0];
                var g = new Grid
                {
                    Name = "grid_" + actualName, //remove file extension for name
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    VerticalAlignment = VerticalAlignment.Stretch,
                    Height = 30,
                    Margin = new Thickness(0, 0, 0, 5),
                    Background = Brushes.White
                };
                var cd = new ColumnDefinition();
                var cd2 = new ColumnDefinition();
                cd.Width = new GridLength(8, GridUnitType.Star);
                cd2.Width = new GridLength(1, GridUnitType.Star);

                g.ColumnDefinitions.Add(cd);
                g.ColumnDefinitions.Add(cd2);

                Label l = new Label
                {
                    Name = "label_" + actualName,
                    Style = (Style) Application.Current.Resources["FileSelected"],
                    Content = fileName
                };

                var b = new Button { Name = actualName };

                b.Tag = fileName;
                b.Content = "X";
                b.Click += CancelUpload;
                b.Background = Brushes.Red;
                b.Foreground = Brushes.White;
                b.HorizontalContentAlignment = HorizontalAlignment.Center;
                b.VerticalContentAlignment = VerticalAlignment.Center;
                b.HorizontalAlignment = HorizontalAlignment.Stretch;
                b.VerticalAlignment = VerticalAlignment.Stretch;
                b.Name = "RemoveButton";

                Grid.SetColumn(l, 0);
                Grid.SetColumn(b, 1);

                g.Children.Add(l);
                g.Children.Add(b);
                SelectedFiles.Children.Add(g);
                _fileGrids.Add(g);

            }
        }

        void CancelUpload(object sender, RoutedEventArgs e)
        {
            Button b = (Button)sender;
            string tag = b.Tag.ToString(); //fileName

            int id = controller.RemoveFile(tag);

            SelectedFiles.Children.RemoveAt(id);
            _fileGrids.RemoveAt(id);
        }
        
        public void OpenPopup(object sender, RoutedEventArgs e) 
        {
            var b = (Button)sender;
            
            var text = b.Tag.ToString();
            var guid = new Guid(text);
            
            PacketPopup pp = new PacketPopup();
            pp.Controller = controller;

            Packet p = controller.FindPacket(guid);

            if (p != null)
            {
                pp.SetupElements(p); // send the packet as a parameter, along with the colour to make the header
                pp.Owner = this;
                pp.Show();
            }
            
        }

        private void SearchForAddress(object sender, RoutedEventArgs e)
        {
            RemoveAllPackets();
            var search = addressSearch.Text;
            foreach (var packet in controller.packets.Values)
            {
                var packetAddress = packet.Address;
                if (packetAddress != null && packetAddress.GetValue(0).ToString() == search)
                {
                    AddPacket(packet);
                }
            }
        }

        private void SearchForProtocol(object sender, RoutedEventArgs e)
        {
            RemoveAllPackets();
            var search = protocolSearch.Text;
            foreach (var packet in controller.packets.Values)
            {
                var packetProtocol = packet.ProtocolId;
                if (packetProtocol.ToString() == search)
                {
                    AddPacket(packet);
                }
            }
            protocolSearch.Text = "";
        }

        Packet FindPacket(Guid guid) 
        {

            // TODO: change this to be a lookup from dictionary

            foreach (var p in sortedPackets) 
            {
                if (guid.Equals(p.PacketId)) 
                {
                    return p;
                }
            }

            return null;
        }
        
        //This lets us know which image to change to.
        private bool _isUpArrow = true;

        private void ShowDataVisPopup(object sender, RoutedEventArgs e)
        {
            ImageBrush image;

            if (_isUpArrow)
            {
                DataVisButton.VerticalAlignment = VerticalAlignment.Top;
                image = new ImageBrush(new BitmapImage(new Uri(@"pack://application:,,,/Resources/down-arrow.png")));
            }
            else
            {
                image = new ImageBrush(new BitmapImage(new Uri(@"pack://application:,,,/Resources/up-arrow.png")))
                {
                    Stretch = Stretch.UniformToFill
                };
            }

            if (_t == null)
            {
                _t = new System.Timers.Timer();
                _t.Elapsed += TimerEventProcessor;
                _t.Interval = 10;
                _t.Start();
            }

            DataVisButton.Background = image;
        }

        public delegate void UpdateSlider();
        public delegate void UpdateAnimation();
        
        // This is the method to run when the timer is raised.
        private void TimerEventProcessor(object myObject,EventArgs myEventArgs)
        {

            // Restarts the timer and increments the counter.
            if (_isUpArrow)
            {
                _count += 1;
            }
            else 
            {
                _count -= 1;
            }

            if ((_count > 10 && _isUpArrow) || (_count < 2 && !_isUpArrow))
            {
                _t.Stop();

                DataVisualisationPopup.Dispatcher.Invoke(new UpdateSlider(FixStretch));

                _isUpArrow = !_isUpArrow;
                _t = null;
            }

            DataVisualisationPopup.Dispatcher.Invoke(new UpdateSlider(MoveSlider));
        }

        System.Timers.Timer _t;
        int _count;

        /// <summary>
        /// set the height of the packet buttons
        /// </summary>
        private void MoveSlider()
        {
            DataVisualisationPopup.Height = new GridLength(_count, GridUnitType.Star); 
        }

        /// <summary>
        /// Fixes the button at the bottom - else it looks silly
        /// </summary>
        private void FixStretch()
        {
            if (!_isUpArrow)
            {
                DataVisButton.VerticalAlignment = VerticalAlignment.Stretch;
            }

        }

        /// <summary>
        /// Get the style for an error
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        static Style GetErrorStyle(double val)
        {

            var style = new Style { TargetType = typeof(Button) };
            style.Setters.Add(new Setter(MarginProperty, new Thickness(0, 0, 0, (val / 10) - 1)));
            style.Setters.Add(new Setter(HorizontalAlignmentProperty, HorizontalAlignment.Stretch));
            style.Setters.Add(new Setter(HorizontalContentAlignmentProperty, HorizontalAlignment.Center));
            style.Setters.Add(new Setter(VerticalAlignmentProperty, VerticalAlignment.Center));
            style.Setters.Add(new Setter(ForegroundProperty, Brushes.White));
            style.Setters.Add(new Setter(BackgroundProperty, Brushes.Red));
            style.Setters.Add(new Setter(HeightProperty, val));

            return style;
        }

        /// <summary>
        /// get the style for a successful packet
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        private static Style GetSuccessStyle(double val) 
        {

            var style = new Style { TargetType = typeof(Button) };
            style.Setters.Add(new Setter(MarginProperty, new Thickness(0, 0, 0, (val / 10) - 1)));
            style.Setters.Add(new Setter(HorizontalAlignmentProperty, HorizontalAlignment.Stretch));
            style.Setters.Add(new Setter(HorizontalContentAlignmentProperty, HorizontalAlignment.Center));
            style.Setters.Add(new Setter(VerticalAlignmentProperty, VerticalAlignment.Center));
            style.Setters.Add(new Setter(ForegroundProperty, Brushes.Black));


            var converter = new System.Windows.Media.BrushConverter();

            style.Setters.Add(new Setter(Button.BackgroundProperty, (Brush)converter.ConvertFromString("#6699ff")));
            style.Setters.Add(new Setter(Button.HeightProperty, val));

            return style;
        }

        /// <summary>
        /// Get the style for a timestamp
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        public Style GetTimeStyle(double val)
        {

            var style = new Style { TargetType = typeof(Label) };
            style.Setters.Add(new Setter(MarginProperty, new Thickness(0, 0, 0, (val / 10) - 1)));
            style.Setters.Add(new Setter(HorizontalAlignmentProperty, HorizontalAlignment.Stretch));
            style.Setters.Add(new Setter(HorizontalContentAlignmentProperty, HorizontalAlignment.Center));
            style.Setters.Add(new Setter(VerticalAlignmentProperty, VerticalAlignment.Center));
            style.Setters.Add(new Setter(VerticalContentAlignmentProperty, VerticalAlignment.Center));
            style.Setters.Add(new Setter(ForegroundProperty, Brushes.Black));


            var converter = new System.Windows.Media.BrushConverter();

            style.Setters.Add(new Setter(Button.BackgroundProperty, (Brush)converter.ConvertFromString("#d9d9d9")));
            style.Setters.Add(new Setter(HeightProperty, val));

            return style;
        }

        /// <summary>
        /// Get the style for a timestamp
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        public Style GetFillerStyle(double val)
        {

            var style = new Style { TargetType = typeof(Label) };
            style.Setters.Add(new Setter(MarginProperty, new Thickness(0, 0, 0, (val / 10) - 1)));
            style.Setters.Add(new Setter(VisibilityProperty, Visibility.Hidden));


            var converter = new System.Windows.Media.BrushConverter();

            style.Setters.Add(new Setter(Button.BackgroundProperty, (Brush)converter.ConvertFromString("#b383d3")));
            style.Setters.Add(new Setter(HeightProperty, val));

            return style;
        }

        /// <summary>
        /// change the height of the objects - xzoom in and out
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Application.Current.Resources["Success"] = GetSuccessStyle(HeightScroller.Value);
            Application.Current.Resources["Error"] = GetErrorStyle(HeightScroller.Value);
            Application.Current.Resources["Timestamp"] = GetTimeStyle(HeightScroller.Value);
            Application.Current.Resources["TimeFiller"] = GetFillerStyle(HeightScroller.Value);            
        }


        //The following code hides or shows each port depending on if the filter checkbox has been checked or not.
        #region Hide and Show Ports

        void DisplayPort1A(object sender, RoutedEventArgs e)
        {
            Port1A.Width = new GridLength(1, GridUnitType.Star);
            Port1AHeader.Width = new GridLength(1, GridUnitType.Star);
        }
        void HidePort1A(object sender, RoutedEventArgs e)
        {
            Port1A.Width = new GridLength(0, GridUnitType.Star);
            Port1AHeader.Width = new GridLength(0, GridUnitType.Star);
        }
        void DisplayPort1B(object sender, RoutedEventArgs e)
        {
            Port1B.Width = new GridLength(1, GridUnitType.Star);
            Port1BHeader.Width = new GridLength(1, GridUnitType.Star);
        }
        void HidePort1B(object sender, RoutedEventArgs e)
        {
            Port1B.Width = new GridLength(0, GridUnitType.Star);
            Port1BHeader.Width = new GridLength(0, GridUnitType.Star);
        }
        

        void DisplayPort2A(object sender, RoutedEventArgs e)
        {
            Port2A.Width = new GridLength(1, GridUnitType.Star);
            Port2AHeader.Width = new GridLength(1, GridUnitType.Star);
        }
        void HidePort2A(object sender, RoutedEventArgs e)
        {
            Port2A.Width = new GridLength(0, GridUnitType.Star);
            Port2AHeader.Width = new GridLength(0, GridUnitType.Star);
        }
        void DisplayPort2B(object sender, RoutedEventArgs e)
        {
            Port2B.Width = new GridLength(1, GridUnitType.Star);
            Port2BHeader.Width = new GridLength(1, GridUnitType.Star);
        }
        void HidePort2B(object sender, RoutedEventArgs e)
        {
            Port2B.Width = new GridLength(0, GridUnitType.Star);
            Port2BHeader.Width = new GridLength(0, GridUnitType.Star);
        }


        void DisplayPort3A(object sender, RoutedEventArgs e)
        {
            Port3A.Width = new GridLength(1, GridUnitType.Star);
            Port3AHeader.Width = new GridLength(1, GridUnitType.Star);
        }
        void HidePort3A(object sender, RoutedEventArgs e)
        {
            Port3A.Width = new GridLength(0, GridUnitType.Star);
            Port3AHeader.Width = new GridLength(0, GridUnitType.Star);
        }
        void DisplayPort3B(object sender, RoutedEventArgs e)
        {
            Port3B.Width = new GridLength(1, GridUnitType.Star);
            Port3BHeader.Width = new GridLength(1, GridUnitType.Star);
        }
        void HidePort3B(object sender, RoutedEventArgs e)
        {
            Port3B.Width = new GridLength(0, GridUnitType.Star);
            Port3BHeader.Width = new GridLength(0, GridUnitType.Star);
        }


        void DisplayPort4A(object sender, RoutedEventArgs e)
        {
            Port4A.Width = new GridLength(1, GridUnitType.Star);
            Port4AHeader.Width = new GridLength(1, GridUnitType.Star);
        }
        void HidePort4A(object sender, RoutedEventArgs e)
        {
            Port4A.Width = new GridLength(0, GridUnitType.Star);
            Port4AHeader.Width = new GridLength(0, GridUnitType.Star);
        }

        void DisplayPort4B(object sender, RoutedEventArgs e)
        {
            Port4B.Width = new GridLength(1, GridUnitType.Star);
            Port4BHeader.Width = new GridLength(1, GridUnitType.Star);
        }
        void HidePort4B(object sender, RoutedEventArgs e)
        {
            Port4B.Width = new GridLength(0, GridUnitType.Star);
            Port4BHeader.Width = new GridLength(0, GridUnitType.Star);
        }


        void SelectAllPorts(object sender, RoutedEventArgs e)
        {
            Check1A.IsChecked = true;
            Check1B.IsChecked = true;
            Check2A.IsChecked = true;
            Check2B.IsChecked = true;
            Check3A.IsChecked = true;
            Check3B.IsChecked = true;
            Check4A.IsChecked = true;
            Check4B.IsChecked = true;
        }
        void DeselectAllPorts(object sender, RoutedEventArgs e)
        {
            Check1A.IsChecked = false;
            Check1B.IsChecked = false;
            Check2A.IsChecked = false;
            Check2B.IsChecked = false;
            Check3A.IsChecked = false;
            Check3B.IsChecked = false;
            Check4A.IsChecked = false;
            Check4B.IsChecked = false;
        }

        #endregion

        List<Packet> sortedPackets;

        private void cmdBeginAnalysis_Click(object sender, RoutedEventArgs e)
        {
            if (controller.filePaths.Count < 1)
            {
                MessageBox.Show("No Files Selected");
            }
            else
            {
                for(int i = 0; i < 8; i++){
                    _previous[i] = new TimeSpan();
                }

                FiltersPane.Width = new GridLength(3, GridUnitType.Star);
                FileSelectedPane.Width = new GridLength(0, GridUnitType.Star);

                bool changed = false;

                RemoveAllPackets();

                var packets = controller.ParsePackets().ToList();


                //packets = controller.packets.Values.ToList();

                sortedPackets = (from pair in packets orderby pair.DateRecieved ascending select pair).ToList();

                Packet[] firstLoad;
                try
                {
                    firstLoad = sortedPackets.GetRange(0, 100).ToArray();
                    //firstLoad = sortedPackets.Values.ToList().GetRange(0, 100);

                    //List<Packet> packsss = ;

                    //firstLoad = packsss.GetRange(0, 100).ToArray();

                }
                catch(Exception)
                {
                    //firstLoad = null;
                    firstLoad = sortedPackets.ToArray();
                }
                
                CreateAllTimeLabels(firstLoad);
                AddPacketCollection(firstLoad);

                CreateChart();

                CreateDataRateGraph(packets.ToArray());

                if (controller.packets.Count < 100) { NextPageBtn.Visibility = System.Windows.Visibility.Hidden; } else { NextPageBtn.Visibility = System.Windows.Visibility.Visible; }

            }

        }

        int interval;
        TimeSpan section = new TimeSpan();
        TimeSpan negative_section = new TimeSpan();
        TimeSpan half_section = new TimeSpan();


        TimeSpan[] _timespans;

        void CreateAllTimeLabels(Packet[] packets)
        {
            var timelist = new List<TimeSpan>();
            var tStart = packets[0].DateRecieved.TimeOfDay;
            var tEnd = packets[packets.Length - 1].DateRecieved.TimeOfDay;


            var timeDiff = tEnd - tStart;

            var milli = (int)timeDiff.TotalMilliseconds;
            
            interval = milli / packets.Length /2;
            //interval = 300;
            

            section = new TimeSpan(0, 0, 0, 0, interval);
            negative_section = section.Negate();
            half_section = new TimeSpan(0, 0, 0, 0, -interval / 2);
            var i = 0;

            var curr = tStart.Add(new TimeSpan(0, 0, 0, 0, (int)(interval * i)));
            timelist.Add(curr);

            while (curr <= tEnd)
            {
                i++;
                CreateTimeLabel(curr);

                curr = tStart.Add(new TimeSpan(0, 0, 0, 0, (int)(interval * i)));
                timelist.Add(curr);
            }
            _timespans = timelist.ToArray();
        }
        
        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            RemoveAllPackets();
            foreach (var packs in controller.packets.Values) 
            {
                Packet p = (Packet)packs;
                if (p.IsError) 
                {
                    AddPacket(p);
                }
            }

          
            CreateDataRateGraph(controller.packets.Values.ToArray());
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            RemoveAllPackets();
                        
            Packet[] packets = new Packet[controller.packets.Count];

            int count = 0;
            foreach (var p in controller.packets.Values) 
            {
                if (count < 100)
                {
                    packets[count] = (Packet)p;
                    count++;
                }
                else { break; }
            }

            AddPacketCollection(packets);
            CreateDataRateGraph(packets);
        }

        private void CreateChart() 
        {
            Analyser a = new Analyser();
            double errRate = a.CalculateErrorRateFromArray(controller.packets.Values.ToArray());
            
            
            Style style = new Style(typeof(Chart));
            Setter st1 = new Setter(Chart.BackgroundProperty,
                                        new SolidColorBrush(Colors.Transparent));
            Setter st4 = new Setter(Chart.ForegroundProperty,
                                        new SolidColorBrush(Colors.White));
            Setter st2 = new Setter(Chart.BorderBrushProperty,
                                        new SolidColorBrush(Colors.White));
            Setter st3 = new Setter(Chart.BorderThicknessProperty, new Thickness(0));

            style.Setters.Add(st1);
            style.Setters.Add(st2);
            style.Setters.Add(st3);
            style.Setters.Add(st4);

            mcChart.Style = style;
            
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
            
            Legend legend = ObjectFinder.FindChild<Legend>(mcChart, "Legend");
            if (legend != null)
            {
                legend.Foreground = new SolidColorBrush(Colors.White);
                legend.Background = new SolidColorBrush(Colors.Transparent);
                legend.BorderBrush = new SolidColorBrush(Colors.Transparent);
            }

            
            System.Windows.Controls.DataVisualization.ResourceDictionaryCollection pieSeriesPalette = new System.Windows.Controls.DataVisualization.ResourceDictionaryCollection();

            Brush currentBrush = new SolidColorBrush(Color.FromRgb(20, 200, 20)); //Green
            Brush currentBrush2 = new SolidColorBrush(Color.FromRgb(200, 20, 20)); //Red

            System.Windows.ResourceDictionary pieDataPointStyles = new ResourceDictionary();
            Style stylePie = new Style(typeof(PieDataPoint));
            stylePie.Setters.Add(new Setter(PieDataPoint.BackgroundProperty, currentBrush));
            pieDataPointStyles.Add("DataPointStyle", stylePie);

            System.Windows.ResourceDictionary pieDataPointStyles2 = new ResourceDictionary();
            Style stylePie2 = new Style(typeof(PieDataPoint));
            stylePie2.Setters.Add(new Setter(PieDataPoint.BackgroundProperty, currentBrush2));
            pieDataPointStyles2.Add("DataPointStyle", stylePie2);

            pieSeriesPalette.Add(pieDataPointStyles2);
            pieSeriesPalette.Add(pieDataPointStyles);

            mcChart.Palette = pieSeriesPalette;



            ((PieSeries)mcChart.Series[0]).ItemsSource =
            new KeyValuePair<string, double>[]{
            new KeyValuePair<string, double>("Error", errRate),
            new KeyValuePair<string, double>("Success", 1-errRate) };

            GraphPanelPie.Width = new GridLength(3, GridUnitType.Star);
            
        }

        void Reset(object sender, RoutedEventArgs e)
        {
            ChkErrorsOnly.IsChecked = false;

            controller.packets.Clear();
            controller.filePaths.Clear();

            SelectedFiles.Children.Clear();
            _fileGrids.Clear();

            RemoveAllPackets();

            GraphPanelPie.Width = new GridLength(0, GridUnitType.Star);

            CreateDataRateGraph(controller.packets.Values.ToArray());

            _count = 2;
            _isUpArrow = false;
            ShowDataVisPopup(null, null);
            SelectAllPorts(null, null);

            FileSelectedPane.Width = new GridLength(3, GridUnitType.Star);
            FiltersPane.Width = new GridLength(0, GridUnitType.Star);

        }

        int pageIndex = 0;

        void NextPage(object sender, RoutedEventArgs e)
        {
            RemoveAllPackets();
            pageIndex++;

            for (int i = 0; i < 8; i++)
            {
                _previous[i] = new TimeSpan();
            }


            if ((100 * (pageIndex + 1)) > controller.packets.Count)
            {
                NextPageBtn.Visibility = Visibility.Hidden;
            }
            else
            {
                NextPageBtn.Visibility = Visibility.Visible;
            }


            Packet[] toLoad;
            try
            {
                toLoad = sortedPackets.GetRange((100 * pageIndex), 100).ToArray();
            }
            catch (Exception)
            {
                toLoad = sortedPackets.GetRange((100 * pageIndex), controller.packets.Count - (100 * pageIndex)).ToArray();
            }

            CreateAllTimeLabels(toLoad);
            AddPacketCollection(toLoad);

            PrevPageBtn.Visibility = Visibility.Visible;

        }

        void PrevPage(object sender, RoutedEventArgs e)
        {
            RemoveAllPackets();
            pageIndex--;
            
            for (int i = 0; i < 8; i++)
            {
                _previous[i] = new TimeSpan();
            }

            if (pageIndex == 0)
            {
                PrevPageBtn.Visibility = Visibility.Hidden;
            }
            else 
            {
                PrevPageBtn.Visibility = Visibility.Visible;
            }

            Packet[] toLoad;
            try
            {
                toLoad = sortedPackets.GetRange((100 * pageIndex), 100).ToArray();
            }
            catch (Exception)
            {
                toLoad = sortedPackets.GetRange((100 * pageIndex), controller.packets.Values.ToList().Count - (100 * pageIndex)).ToArray();
            }
            
            CreateAllTimeLabels(toLoad);
            AddPacketCollection(toLoad);

            NextPageBtn.Visibility = Visibility.Visible;

        }

    }


}
