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
using StarMeter.View.Helpers;
using System.Timers;
using Microsoft.VisualBasic;
using System.IO;
using System.Reflection;
using System.Threading;

namespace StarMeter.View
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private readonly Controller _controller = new Controller();
        private readonly Analyser _analyser = new Analyser();

        private readonly StackPanel[] _portStacks = new StackPanel[8];
        public static List<KeyValuePair<int, string>> Protocols = new List<KeyValuePair<int,string>>();

        public static int PageIndex;

        // loading icon
        private readonly GifBitmapDecoder _gifDecoder;
        private int _animCount;
        private readonly System.Timers.Timer _loadingTimer;

        // status variables for timers
        private bool _isUpArrow = true;
        private bool _isLeftArrow;
        private bool _isRightArrow;
        
        // timers for sliding the various panels in - and a max value to count up to before stopping the timer
        System.Timers.Timer _t;
        private int _count;
        System.Timers.Timer _t2;
        private double _count2 = 2.75;
        System.Timers.Timer _t3;
        private double _count3 = 2.75;

        // displaying custom colours
        BrushConverter _brushConvertor = new BrushConverter();
        
        // display by time
        private readonly TimeSpan[] _previous = new TimeSpan[8];
        private readonly List<List<Guid>[]> _timeSpanOccupied = new List<List<Guid>[]>();
        private int _interval;
        private TimeSpan _section;
        private TimeSpan _negativeSection;
        private TimeSpan _halfSection;
        private TimeSpan[] _timespans;
        
        // this will allow us to read the files or remove the files later.
        private readonly List<Grid> _fileGrids = new List<Grid>();
        
        // delegates to allow GUI changes to be made from a Timer/seperate thread
        public delegate void UpdateSlider();
        public delegate void UpdateAnimation();
        public delegate void ResetColour();
        public delegate void CreatePacketPage();
        
        // error list objects
        Button _lblFoundObj;
        Brush _colourBeforeHighlight;
        System.Timers.Timer _showHighlightedPacket;

        // toggling the error list open/close
        bool _isErrorListOpen = true;
        bool _isFilesDisplayedOpen = true;

        // packets in an ordered list
        public List<Packet> SortedPackets = new List<Packet>();

        Packet[] _tempStore;

        List<KeyValuePair<string, int>>[] _lineChartData; 

        /// <summary>
        /// Constructor for the class/Window
        /// </summary> 
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

            // setup thw loading Icon from the file 
            _gifDecoder = new GifBitmapDecoder(new Uri("pack://application:,,,/Resources/rocket.gif"), BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.Default);
            LoadingIcon.Source = _gifDecoder.Frames[0];

            // setup the timer for the icon
            _loadingTimer = new System.Timers.Timer();
            _loadingTimer.Elapsed += _LoadingTimer_Elapsed;
            _loadingTimer.Interval = 80;

            LoadProtocolList();

        }

        void LoadProtocolList() 
        {
            try
            {
                //TODO: change to comma thing

                StreamReader sr = new StreamReader("../../Resources/ProtocolList.txt");

                string line = sr.ReadLine();

                while (line != null)
                {
                    string[] split = line.Split('(');
                    int key = int.Parse(split[0]);
                    string name = split[1];

                    KeyValuePair<int, string> kvp = new KeyValuePair<int, string>(key, name);
                    Protocols.Add(kvp);

                    line = sr.ReadLine();
                }

                sr.Close();
                AddProtocolObjects();

                
            }
            catch (Exception e) { }
        }

        void RemoveProtocolFromFile(string protocol) 
        {
            var allLines = File.ReadAllLines("../../Resources/ProtocolList.txt");

            StreamWriter sw = new StreamWriter("../../Resources/ProtocolList.txt", false);

            foreach (var l in allLines) 
            {
                if (!l.Split('@')[0].Trim().Equals(protocol.Split('@')[0].Trim())) 
                {
                    sw.WriteLine(l);
                }
            }

            sw.Close();

        }

        void RemoveProtocol(object sender, RoutedEventArgs e) 
        {
            var objectSelected = ((ComboBoxItem)sender);
            var content = objectSelected.Content.ToString();
            RemoveProtocolFromFile(content);
            ProtocolSelected.Items.Remove(objectSelected);
        }

        void AddProtocolObjects() 
        {
            foreach (var p in Protocols)
            {
                // add to list
                ComboBoxItem cb = new ComboBoxItem();
                cb.Content = p.Key + " (" + p.Value;
                cb.FontFamily = new FontFamily("Gill Sans MT");
                cb.FontSize = 14;
                cb.VerticalContentAlignment = System.Windows.VerticalAlignment.Center;
                cb.HorizontalContentAlignment = System.Windows.HorizontalAlignment.Left;
                cb.VerticalAlignment = System.Windows.VerticalAlignment.Stretch;
                cb.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
                cb.MouseRightButtonUp += RemoveProtocol;

                ProtocolSelected.Items.Add(cb);
            }
        }


        int _prevCount = 0;

        /// <summary>
        /// Update which frame of the gif to display
        /// </summary>
        private void ChangeAnimFrame()
        {
            if (_animCount >= _gifDecoder.Frames.Count - 1)
            {
                _animCount = 0; // go back to start
            }
            LoadingIcon.Source = _gifDecoder.Frames[_animCount];
           
        }

        /// <summary>
        /// Display the '...' after loading message
        /// </summary>
        public void ChangeDots()
        {
            // work out how many dots to display
            int dots = _animCount / 6;
            string[] split = LoadingMessage.Content.ToString().Split('.');
            string dottage = "";

            for (int i = 0; i < dots; i++)
            {
                dottage += ".";
            }

            // display them
            LoadingMessage.Content = split[0] + dottage;
        }

        /// <summary>
        /// Each 'Tick' of the loading timer
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _LoadingTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            _animCount++;
            LoadingIcon.Dispatcher.Invoke(new UpdateAnimation(ChangeAnimFrame));
            LoadingMessage.Dispatcher.Invoke(new UpdateAnimation(ChangeDots));
        }
        
        /// <summary>
        /// Work out which panel to assign the packet to
        /// </summary>
        /// <param name="portNum">The port nuber of the packet</param>
        /// <returns>Which stack panel to make the parent</returns>
        private StackPanel GetPanelToUse(int portNum)
        {
            return _portStacks[portNum - 1];
        }

        /// <summary>
        /// Display a collection of packets on the screen
        /// </summary>
        /// <param name="packets">The array of packets to display</param>
        public void AddPacketCollection(Packet[] packets)
        {
            // loop through each packet, setting the initial value of each element in the _previous array
            foreach (Packet p in packets)
            {
                if (_previous[p.PortNumber - 1] == new TimeSpan())
                {
                    _previous[p.PortNumber - 1] = p.DateReceived.TimeOfDay;
                }
            }

            // loop through each packet and add a timespan for each
            var tempTimespans = new List<KeyValuePair<int, TimeSpan>>();
            if (packets.Length > 1)
            {
                TimeSpan[] times = _timespans.ToArray();
                tempTimespans = new List<KeyValuePair<int, TimeSpan>>();

                for (int i = 0; i < times.Length; i++)
                {
                    tempTimespans.Add(new KeyValuePair<int, TimeSpan>(i, times[i]));
                }
            }
            else
            {
                if (packets.Length > 0)
                {
                    tempTimespans.Add(new KeyValuePair<int, TimeSpan>(0, packets[0].DateReceived.TimeOfDay));
                }
            }

            foreach (var p in packets)
            {
                AddPacket(p, tempTimespans); // add each single packet on the screen.
            }
            
        }

        /// <summary>
        /// Add a single packet to the columns
        /// </summary>
        /// <param name="p">The packet to add</param>
        /// <param name="tempTimespans">The timespans to use</param>
        private void AddPacket(Packet p, List<KeyValuePair<int, TimeSpan>> tempTimespans)
        {
            // var temp_timespans = _timespans.ToList();
            var packetTimespan = p.DateReceived.TimeOfDay;
            var sp = GetPanelToUse(p.PortNumber);
            bool found = false;
            int index = 0;


            // The following is a binary search algorithm to find which timestamp matches the current packet in O(log n) time.
            // As our display contains a representation of time passing we cut the list in half and traverse through the list until we find
            // a timestamp which is "close enough." This is done using sections as it is extremely unlikely that the packet we have perfectly
            // matches a timestamp.
            while (found == false && tempTimespans.Count > 0)
            {
                index = tempTimespans.Count / 2;

                if (tempTimespans[index].Value >= packetTimespan)
                {
                    if (tempTimespans[index].Value.Add(_halfSection) <= packetTimespan)
                    {
                        found = true;
                    }
                    else
                    {
                        tempTimespans = tempTimespans.GetRange(0, index);
                    }
                }
                else
                {
                    if (tempTimespans[index].Value.Add(_section) >= packetTimespan)
                    {
                        found = true;
                    }
                    else
                    {
                        tempTimespans = tempTimespans.GetRange(index, tempTimespans.Count - index);
                    }
                }
            }
            _timeSpanOccupied[tempTimespans[index].Key][p.PortNumber - 1].Add(p.PacketId);
            var currentNumber = _timeSpanOccupied[tempTimespans[index].Key][p.PortNumber - 1].Count;

            // if 2 packets share the same timespan, reconfigure the button to open up the MultiplePacketPopup
            if (currentNumber > 1)
            {
                var childObjs = GetPanelToUse(p.PortNumber).Children;
                string toFind = "btn" + tempTimespans[index].Value.ToString().Replace('.', 'M').Replace(':', '_');
                StackPanel stackPan = GetPanelToUse(p.PortNumber);

                var existingBtn = (Button)sp.FindName(toFind);
                var btn = (Button)LogicalTreeHelper.FindLogicalNode(stackPan, toFind);

                if (btn.Background == Brushes.Red || p.IsError)
                {
                    btn.Background = Brushes.Red;
                }

                // clear all event handlers 
                btn.Click -= OpenPopup;

                btn.Click -= ViewMultiplePackets;
                btn.Click -= ViewMultiplePackets;
                btn.Click -= ViewMultiplePackets;
                btn.Click -= ViewMultiplePackets;
                btn.Click -= ViewMultiplePackets;
                btn.Click -= ViewMultiplePackets;   // may have been assigned multiple times - just make sure!

                btn.Tag = tempTimespans[index].Key + "@" + p.PortNumber;

                btn.Click += ViewMultiplePackets;
                
                btn.ToolTip = null;

                Label l = (Label)btn.Content;
                btn.Tag = tempTimespans[index].Key + "@" + p.PortNumber;
                l.Content = currentNumber + " PACKETS";
                l.Foreground = Brushes.Yellow;
            }
            else //we only have one packet to display.
            {
                var diff = (tempTimespans[index].Value - _previous[p.PortNumber - 1]); //This is the difference in time between the last packet and this one.
                int spaces = 0;

                //In order to determine how many blank spaces to display, we continually remove a "section" from the difference
                //incrementing the number of spaces to add by one each time.
                while (diff.CompareTo(new TimeSpan(0, 0, 0, 0, _interval)) > 0)
                {
                    diff = diff.Add(_negativeSection);
                    spaces++;
                }

                for (int i = 0; i < spaces; i++)
                {
                    Label lbl = new Label(); //Empty label filling the size that one packet takes up.
                    lbl.SetResourceReference(Control.StyleProperty, "TimeFiller");
                    sp.Children.Add(lbl);
                }

                var b = ComponentFetcher.GetPacketButton(p, tempTimespans[index].Value.ToString());

                b.Click += OpenPopup;

                sp.Children.Add(b);
                _previous[p.PortNumber - 1] = tempTimespans[index].Value;
            }
        }
                        
        /// <summary>
        /// Remove all packets from the screen
        /// </summary>
        public void RemoveAllPackets()
        {
            // loop through all child elements of the stacks, remove all components
            for (int i = 0; i < 8; i++)
            {
                var childElements = _portStacks[i].Children;

                while (childElements.Count > 0)
                {
                    childElements.Remove((UIElement)childElements[0]);
                }
            }
            var timeElements = TimeList.Children;

            // same for the time labels
            while (timeElements.Count > 0)
            {
                timeElements.Remove((UIElement)timeElements[0]);
            }
        }
        
        /// <summary>
        /// Create the Line graph
        /// </summary>
        /// <param name="packets"></param>
        void CreateDataRateGraph(Packet[] packets)
        {
            RatesLineChart.Series.Clear();
            RatesLineChart.DataContext = null;

            _lineChartData = _analyser.GetDataForLineChart(SortedPackets.ToArray());

            FormatLineChart(0);
        }

        /// <summary>
        /// Format the output for the line chart
        /// </summary>
        /// <param name="values">The values to add to the chart</param>
        void FormatLineChart(int index) 
        {

            RatesLineChart.Series.Clear();
            
            // add the error data
            var lineSeries = new LineSeries
            {
                Title = "Error Rate",
                Foreground = Brushes.Black,
                DependentValuePath = "Value",
                IndependentValuePath = "Key",
                ItemsSource = _lineChartData[index]
            };
            RatesLineChart.Series.Add(lineSeries);

            RatesLineChart.DataContext = _lineChartData[index];

            // format the legend
            Legend legend = ObjectFinder.FindChild<Legend>(RatesLineChart, "Legend");
            if (legend != null)
            {
                legend.FontFamily = new System.Windows.Media.FontFamily("Gill Sans MT");
                legend.Visibility = Visibility.Hidden;
                legend.Foreground = new SolidColorBrush(Colors.White);
                legend.Background = new SolidColorBrush(Colors.Transparent);
                legend.BorderBrush = new SolidColorBrush(Colors.Transparent);
            }


            // set the colours for the graph
            System.Windows.Controls.DataVisualization.ResourceDictionaryCollection lineSeriesPalette = new System.Windows.Controls.DataVisualization.ResourceDictionaryCollection();
            Brush currentBrush;


            if (index == 1)
            {
                currentBrush = new SolidColorBrush(Color.FromRgb(200, 20, 20)); //Red
            }
            else 
            {
                currentBrush = new SolidColorBrush(Color.FromRgb(20, 200, 20)); //Green
            }
            
            System.Windows.ResourceDictionary pieDataPointStyles2 = new ResourceDictionary();
            Style stylePie2 = new Style(typeof(LineDataPoint));
            stylePie2.Setters.Add(new Setter(LineDataPoint.BackgroundProperty, currentBrush));
            pieDataPointStyles2.Add("DataPointStyle", stylePie2);

            RatesLineChart.FontSize = 10;
            
            // set the palette
            lineSeriesPalette.Add(pieDataPointStyles2);
            RatesLineChart.Palette = lineSeriesPalette;
        }

        /// <summary>
        /// Open the menu to view multiple packets
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ViewMultiplePackets(object sender, RoutedEventArgs e)
        {
            MultiplePacketPopup mpp = new MultiplePacketPopup(_controller);
            string[] split = ((Button)sender).Tag.ToString().Split('@');
            var id = int.Parse(split[0]);
            var port = int.Parse(split[1]);
            List<Guid> guids = _timeSpanOccupied[id][port - 1];
            List<Packet> ps = new List<Packet>();

            foreach (Guid g in guids)
            {
                ps.Add(FindPacket(g));
            }

            mpp.Controller = _controller;
            mpp.Owner = this;
            mpp.CreateElements(ps);
            mpp.ShowDialog();
        }

        /// <summary>
        /// Open the file dialog and select a file
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FileSelection(object sender, RoutedEventArgs e)
        {
            // open a file dialog
            var ofd = new OpenFileDialog
            {
                // only allow .rec files
                Filter = "Record Files (.rec)|*.rec",
                Multiselect = true
            };

            bool? confirmed = ofd.ShowDialog();

            // if the user cancels, hide the form and hide the loading icon
            if (confirmed != true)
            {
                return;
            }

            LoadingMessage.Content = "Selecting File";

            // display file name in list
            List<string> filesAdded = _controller.AddFileNames(ofd.FileNames);
            AddFileSelected(filesAdded);
            
        }
        
        /// <summary>
        /// Add the selected files to the list being displayed on the screen
        /// </summary>
        /// <param name="filesAdded">The list of files to add</param>
        void AddFileSelected(List<string> filesAdded) 
        {
            foreach (string fileName in filesAdded)
            {
                string actualName = fileName.Split('.')[0];

                // create a grid to hold the elements
                var g = new Grid
                {
                    Name = "grid_" + actualName, //remove file extension for name
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    VerticalAlignment = VerticalAlignment.Stretch,
                    Height = 60,
                    Margin = new Thickness(0, 0, 0, 5),
                    Background = Brushes.White
                };
                var cd = new ColumnDefinition();
                var cd2 = new ColumnDefinition();
                cd.Width = new GridLength(6, GridUnitType.Star);
                cd2.Width = new GridLength(1, GridUnitType.Star);

                g.ColumnDefinitions.Add(cd);
                g.ColumnDefinitions.Add(cd2);

                // add a label (filename) and button (remove) to he grid
                Label l = new Label
                {
                    Name = "label_" + actualName,
                    Style = (Style)Application.Current.Resources["FileSelected"],
                    Content = fileName
                };

                var b = new Button
                {
                    Name = actualName,
                    Tag = fileName,
                    Content = "X"
                };

                b.Click += CancelUpload;
                b.FontFamily = new FontFamily("Gill Sans MT");
                b.FontSize = 14;
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

        /// <summary>
        /// Remove a file from the list of selected files
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CancelUpload(object sender, RoutedEventArgs e)
        {
            Button b = (Button)sender;
            string tag = b.Tag.ToString(); //fileName
            int id = _controller.RemoveFile(tag);

            SelectedFiles.Children.RemoveAt(id);
            _fileGrids.RemoveAt(id);
        }

        /// <summary>
        /// Open the Packet Popup dialog
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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
                pp.SetupElements(p); // send the packet as a parameter
                //pp.Owner = this;
                pp.Show();
            }
        }

        /// <summary>
        /// Find a packet based on its Guid
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        private Packet FindPacket(Guid guid)
        {
            // TODO: change this to be a lookup from dictionary
            return SortedPackets.FirstOrDefault(p => guid.Equals(p.PacketId));
        }

        /// <summary>
        /// Show the line chart panel
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ShowDataVisPopup(object sender, RoutedEventArgs e)
        {
            ImageBrush image;

            if (_isUpArrow)
            {
                //DataVisButton.VerticalAlignment = VerticalAlignment.Top;
                image = new ImageBrush(new BitmapImage(new Uri(@"pack://application:,,,/Resources/chart button2.png")))
                {
                    Stretch = Stretch.Uniform
                };
                DataRadioButtons.Visibility = Visibility.Visible;
            }
            else
            {
                image = new ImageBrush(new BitmapImage(new Uri(@"pack://application:,,,/Resources/chart button.png")))
                {
                    Stretch = Stretch.Uniform
                };
                DataRadioButtons.Visibility = Visibility.Hidden;
            }

            // start the timer
            if (_t == null)
            {
                _t = new System.Timers.Timer();
                _t.Elapsed += TimerEventProcessor;
                _t.Interval = 10;
                _t.Start();
            }

            DataVisButton.Background = image;
        }
        
        /// <summary>
        /// Show the RHS (Stats) panel
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ShowDataVisPopup2(object sender, RoutedEventArgs e)
        {
            ImageBrush image;

            if (_isRightArrow)
            {
                image = new ImageBrush(new BitmapImage(new Uri(@"pack://application:,,,/Resources/stats button.png")))
                {
                    Stretch = Stretch.Uniform
                };
            }
            else
            {
                image = new ImageBrush(new BitmapImage(new Uri(@"pack://application:,,,/Resources/stats button2.png")))
                {
                    Stretch = Stretch.Uniform
                };
            }

            // start the timer
            if (_t2 == null)
            {
                _t2 = new System.Timers.Timer();
                _t2.Elapsed += TimerEventProcessor2;
                _t2.Interval = 10;
                _t2.Start();
            }

            DataVisButton2.Background = image;
        }

        /// <summary>
        /// Show the Filters panel
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ShowDataVisPopup3(object sender, RoutedEventArgs e)
        {
            ImageBrush image;

            if (_isLeftArrow)
            {
                image = new ImageBrush(new BitmapImage(new Uri(@"pack://application:,,,/Resources/filter button2.png")))
                {
                    Stretch = Stretch.Uniform
                };
            }
            else
            {
                image = new ImageBrush(new BitmapImage(new Uri(@"pack://application:,,,/Resources/filter button.png")))
                {
                    Stretch = Stretch.Uniform
                };
            }

            // start the timer
            if (_t3 == null)
            {
                _t3 = new System.Timers.Timer();
                _t3.Elapsed += TimerEventProcessor3;
                _t3.Interval = 10;
                _t3.Start();
            }

            DataVisButton3.Background = image;
        }

        /// <summary>
        /// Controls the bottom panel sliding in
        /// </summary>
        /// <param name="myObject"></param>
        /// <param name="myEventArgs"></param>
        private void TimerEventProcessor(object myObject, EventArgs myEventArgs)
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

            // if the count has reached it's limit, stop the timer
            if ((_count > 10 && _isUpArrow) || (_count < 2 && !_isUpArrow))
            {
                _t.Stop();

                DataVisualisationPopup.Dispatcher.Invoke(new UpdateSlider(FixStretch));

                _isUpArrow = !_isUpArrow;
                _t = null;
            }

            DataVisualisationPopup.Dispatcher.Invoke(new UpdateSlider(MoveSlider));
        }

        /// <summary>
        /// Controls the RHS panel sliding in
        /// </summary>
        /// <param name="myObject"></param>
        /// <param name="myEventArgs"></param>
        private void TimerEventProcessor2(object myObject, EventArgs myEventArgs)
        {
            // Restarts the timer and increments the counter.
            if (_isRightArrow)
            {
                _count2 += 0.25;
            }
            else
            {
                _count2 -= 0.25;
            }

            // if the count has reached it's limit, stop the timer
            if ((_count2 > 2.75 && _isRightArrow) || (_count2 < 0.25 && !_isRightArrow))
            {
                _t2.Stop();
                
                _isRightArrow = !_isRightArrow;
                _t2 = null;
            }

            GraphPanelPie.Dispatcher.Invoke(new UpdateSlider(MoveSlider2));
        }

        /// <summary>
        /// Controls the LHS panel sliding in 
        /// </summary>
        /// <param name="myObject"></param>
        /// <param name="myEventArgs"></param>
        private void TimerEventProcessor3(object myObject, EventArgs myEventArgs)
        {
            // Restarts the timer and increments the counter.
            if (_isLeftArrow)
            {
                _count3 += 0.25;
            }
            else
            {
                _count3 -= 0.25;
            }

            if ((_count3 > 2.75 && _isLeftArrow) || (_count3 < 0.25 && !_isLeftArrow))
            {
                _t3.Stop();

                // if the count has reached it's limit, stop the timer
                _isLeftArrow = !_isLeftArrow;
                _t3 = null;
            }

            FiltersPane.Dispatcher.Invoke(new UpdateSlider(MoveSlider3));
        }

        #region Panel slider timers
        /// <summary>
        /// set the height of the bottom panel
        /// </summary>
        private void MoveSlider()
        {
            DataVisualisationPopup.Height = new GridLength(_count, GridUnitType.Star);
        }
        /// <summary>
        /// Sets the width of the RHS
        /// </summary>
        private void MoveSlider2()
        {
            GraphPanelPie.Width = new GridLength(_count2, GridUnitType.Star);
        }
        /// <summary>
        /// Sets the width of the LHS panel
        /// </summary>
        private void MoveSlider3()
        {
            FiltersPane.Width = new GridLength(_count3, GridUnitType.Star);
        }
        #endregion

        /// <summary>
        /// Fixes the button at the bottom - else it looks silly
        /// </summary>
        private void FixStretch()
        {
            if (!_isUpArrow)
            {
                //DataVisButton.VerticalAlignment = VerticalAlignment.Stretch;
            }
        }
        
        /// <summary>
        /// Change the height of the objects - xzoom in and out
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
        
        /// <summary>
        /// Create a list of the files for which data is being display
        /// </summary>
        void CreateFilesDisplayedList()
        {
            SelectedFiles2.Children.Clear();

            // for each file selected, create a label and add it to the list
            foreach (var s in _controller.FilePaths)
            {
                string actualName2 = (s.Split('\\').Last());

                Label l = new Label()
                {
                    Style = (Style)Application.Current.Resources["FileSelected"],
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    VerticalAlignment = VerticalAlignment.Stretch,
                    Height = 40,
                    Margin = new Thickness(0, 0, 0, 5),
                };

                l.Content = actualName2;
                SelectedFiles2.Children.Add(l);

            }

        }

        /// <summary>
        /// Calculate stats based of the loaded data
        /// </summary>
        void CalculateStats() 
        {

            

            lblNumPackets.Content = "Total Data Characters: " + _analyser.CalculateTotalNoOfDataChars(_controller.Packets);
            lblPacketsPerSec.Content = "Packets per Second: " + Math.Round(_analyser.CalculatePacketRatePerSecond(_controller.Packets), 5);

            ErrorHeader.Content = "Errors (" + _analyser.CalculateTotalNoOfErrorPackets(_controller.Packets) + " total):";
        }

        /// <summary>
        /// When the user clicks the 'Begin Analysis button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cmdBeginAnalysis_Click(object sender, RoutedEventArgs e)
        {
            // check that files exist
            if (_controller.FilePaths.Count < 1)
            {
                MessageBox.Show("No Files Selected");
            }
            else
            {
                LoadingScreen.Visibility = Visibility.Visible;
                LoadingMessage.Content = "Loading";
                _loadingTimer.Start();

                // if there are files...

                // reset previous (before we begin time calculations)
                for (int i = 0; i < 8; i++)
                {
                    _previous[i] = new TimeSpan();
                }


                System.Threading.Thread thr = new System.Threading.Thread(new ParameterizedThreadStart(DoWork));
                thr.SetApartmentState(ApartmentState.STA);
                thr.IsBackground = true;
                thr.Start();

            }

        }


        void DoLoading(object arg) 
        {
            // get the list of packets
            var packets = _controller.ParsePackets().ToList();

            // for presentation purposes?
            //Thread.Sleep(1000);

        }

        void DoWork(object arg) 
        {

            System.Threading.Thread thrLoad = new System.Threading.Thread(new ParameterizedThreadStart(DoLoading));
            thrLoad.SetApartmentState(ApartmentState.STA);
            thrLoad.IsBackground = true;
            thrLoad.Start();

            thrLoad.Join();

            SortedPackets = (from pair in _controller.Packets orderby pair.Value.DateReceived ascending select pair.Value).ToList();
            
            PageIndex = 0;
            
            this.Dispatcher.Invoke(new CreatePacketPage(CreatePage));
            
            // reset _previous for the next load
            for (int i = 0; i < 8; i++)
            {
                _previous[i] = new TimeSpan();
            }

        }


        void CreatePage() 
        {
            // display file list
            CreateFilesDisplayedList();

            // create the pie chart
            CreateChart();

            // display the rest of the the elements
            SetupElements(false);
            CalculateStats();
            DisplayErrorList();
            DisplaySidePanels();

            LoadingScreen.Visibility = System.Windows.Visibility.Hidden;
            _loadingTimer.Stop();

            cmdApplyFilters.Background = (Brush)_brushConvertor.ConvertFromString("#FF4A4D54");
        }

        /// <summary>
        /// Show the side panels (for graphs/filters etc)
        /// </summary>
        void DisplaySidePanels()
        {
            // hide/show the relevant panels and buttons
            FiltersPane.Width = new GridLength(3, GridUnitType.Star);
            FileSelectedPane.Width = new GridLength(0, GridUnitType.Star);
            LeftSidePanel.Width = new GridLength(0.25, GridUnitType.Star);

            DataVisButton2.Visibility = Visibility.Visible;
            DataVisButton3.Visibility = Visibility.Visible;
            DataVisButton.Visibility = Visibility.Visible;
            HeightScroller.Visibility = Visibility.Visible;

            RightButtonColumn.Width = new GridLength(0.25, GridUnitType.Star);

            ImageBrush image = new ImageBrush(new BitmapImage(new Uri(@"pack://application:,,,/Resources/stats button2.png")))
            {
                Stretch = Stretch.Uniform
            };
            DataVisButton2.Background = image;

            _isRightArrow = true;
            _count2 = 0;


            try
            {
                StartTimeTextBox.Text = SortedPackets[0].DateReceived.ToString("dd-MM-yyyy HH:mm:ss.fff");
                EndTimeTextBox.Text = SortedPackets[SortedPackets.Count - 1].DateReceived.ToString("dd-MM-yyyy HH:mm:ss.fff");
            }
            catch (Exception) { }
        }

        /// <summary>
        /// Create the elements for the error list
        /// </summary>
        void DisplayErrorList()
        {
            ErrorListPanel.Children.Clear();

            // loop through all pckets which are flagged as errors
            Packet[] packs = (_controller.Packets.Values.Where(packet => packet.IsError)).ToArray();

            foreach (var p in packs)
            {
                // create button
                Button b = new Button();
                b.Content = p.DateReceived.ToString("HH:mm:ss.fff") + ":   " + p.ErrorType + " (Port " + p.PortNumber + ")";
                b.Foreground = Brushes.White;
                b.HorizontalContentAlignment = System.Windows.HorizontalAlignment.Left;
                b.Margin = new Thickness(0, 0, 0, 5);
                b.Background = (Brush)_brushConvertor.ConvertFromString("#4ca8a8a8");
                b.BorderThickness = new Thickness(0);
                b.Height = 30;

                b.Tag = p.PortNumber.ToString() + '@' + p.PacketId + "@" + p.DateReceived.ToString("dd-MM-yyyy HH:mm:ss.fff");

                b.Click += GoToPacket;

                ErrorListPanel.Children.Add(b);
            }
        }

        /// <summary>
        /// Creates the labels for the time list (down the left hand side)
        /// </summary>
        /// <param name="packets">The packets to relate the times to</param>
        private void CreateAllTimeLabels(Packet[] packets, bool doubleInterval)
        {
            packets = (from pair in packets orderby pair.DateReceived ascending select pair).ToArray();

            _timeSpanOccupied.Clear();
            var l = new List<Guid>[8];

            var tStart = new DateTime();
            var timelist = new List<TimeSpan>();
            try
            {
                tStart = packets[0].DateReceived;
            }
            catch
            {
                return; //This code executes if there are no packets to show.
            }

            DateTime tEnd = packets[packets.Length - 1].DateReceived;
            var timeDiff = tEnd - tStart;

            if (timeDiff.TotalMilliseconds == 0)
            {
                if (packets.Length > 0)
                {
                    var list = new List<Guid>[8];
                    for (int j = 0; j < list.Length; j++)
                    {
                        list[j] = new List<Guid>();
                    }

                    _timeSpanOccupied.Add(list);
                    Button lbl = ComponentFetcher.CreateTimeLabel(packets[0].DateReceived);

                    lbl.Click += LoadSpecificTime;

                    TimeList.Children.Add(lbl);

                }
            }
            else
            {
                var milli = (int)timeDiff.TotalMilliseconds;
               
                _interval = milli / packets.Length / 2;
                
                _section = new TimeSpan(0, 0, 0, 0, _interval);
                _negativeSection = _section.Negate();
                _halfSection = new TimeSpan(0, 0, 0, 0, -_interval / 2);

                if (_halfSection == new TimeSpan(0, 0, 0, 0, 0)) 
                {
                    _halfSection = new TimeSpan(0, 0, 0, 0, -1);
                }

                var i = 0;

                var curr = tStart.Add(new TimeSpan(0, 0, 0, 0, (int)(_interval * i)));
                timelist.Add(curr.TimeOfDay);

                var list = new List<Guid>[8];
                for (int j = 0; j < list.Length; j++)
                {
                    list[j] = new List<Guid>();
                }

                _timeSpanOccupied.Add(list);

                while (curr <= tEnd)
                {
                    i++;
                    Button lbl = ComponentFetcher.CreateTimeLabel(curr);
                    lbl.Click += LoadSpecificTime;
                    TimeList.Children.Add(lbl);

                    curr = tStart.Add(new TimeSpan(0, 0, 0, 0, (int)(_interval * i)));
                    timelist.Add(curr.TimeOfDay);

                    var list2 = new List<Guid>[8];

                    for (int j = 0; j < list2.Length; j++)
                    {
                        list2[j] = new List<Guid>();
                    }

                    _timeSpanOccupied.Add(list2);
                }
                _timespans = timelist.ToArray();
            }
        }
        
        void LoadSpecificTime(object sender, RoutedEventArgs e) 
        {
            string tag = ((Button)sender).Tag.ToString();
            
            // may need to tweak this to go backwards instead of forward
            DateTime start = DateTime.ParseExact(tag, "dd-MM-yyyy HH:mm:ss.fff", null) + _halfSection;
            DateTime end = start + _section;


            // take copy of sortedList
            _tempStore = new Packet[SortedPackets.Count];
            SortedPackets.CopyTo(_tempStore, 0);

            // apply filters
            List<Packet> packets = ApplyFilters(SortedPackets.ToArray(), start, end);

            SortedPackets = (from pair in packets orderby pair.DateReceived ascending select pair).ToList();
            
            FiltersPane.Width = new GridLength(0, GridUnitType.Star);
            GraphPanelPie.Width = new GridLength(0, GridUnitType.Star);
            DataVisualisationPopup.Height = new GridLength(0, GridUnitType.Star);
            RightButtonColumn.Width = new GridLength(0, GridUnitType.Star);
            LeftPanelButton.Width = new GridLength(0, GridUnitType.Star);
            cmdBackToNormalData.Height = new GridLength(1.2, GridUnitType.Star);
            TimeLabels.Width = new GridLength(0, GridUnitType.Pixel);
            TimeHeader.Width = new GridLength(0, GridUnitType.Pixel);
            HeightScroller.Visibility = System.Windows.Visibility.Hidden;

            lblViewingSpecific.Content = "Viewing Packets between " + start.ToString("dd/MM/yyyy HH:mm:ss.fff") + " and " + end.ToString("dd/MM/yyyy HH:mm:ss.fff");

            RemoveAllPackets();

            AddPacketsInOrder(SortedPackets);

        }

        void AddPacketsInOrder(List<Packet> packets) 
        {

            int[] count = new int[8];

            for (int i = 0; i < packets.Count; i++) 
            {
                var sp = GetPanelToUse(packets[i].PortNumber);

                for (int j = 0; j < 2 * count[packets[i].PortNumber - 1]; j++)
                {
                    var lbl = new Label();
                    lbl.SetResourceReference(Control.StyleProperty, "TimeFiller");
                    sp.Children.Add(lbl);
                }

                for (int j = 0; j < 8; j++) 
                {
                    count[j]++;
                }
                count[packets[i].PortNumber - 1] = 0;

                var b = ComponentFetcher.GetPacketButton(packets[i], "");
                b.Click += OpenPopup;
                sp.Children.Add(b);
            }

            PacketScroller.ScrollToTop();

        }

        /// <summary>
        /// When the checkbox for displaying 'Errors only' is checked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            cmdApplyFilters.Background = (Brush)_brushConvertor.ConvertFromString("#FF37A300");
        }

        /// <summary>
        /// When the checkbox for displaying 'Errors only' is unchecked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            cmdApplyFilters.Background = (Brush)_brushConvertor.ConvertFromString("#FF37A300");
        }

        /// <summary>
        /// Set up the style and the appearance of the pie chart
        /// </summary>
        /// <param name="errRate"></param>
        void SetupPieChart(double errRate) 
        {
            Style style = new Style(typeof(Chart));
            Setter st1 = new Setter(BackgroundProperty,
                                        new SolidColorBrush(Colors.Transparent));
            Setter st4 = new Setter(ForegroundProperty,
                                        new SolidColorBrush(Colors.White));
            Setter st2 = new Setter(BorderBrushProperty,
                                        new SolidColorBrush(Colors.White));
            Setter st3 = new Setter(BorderThicknessProperty, new Thickness(0));

            style.Setters.Add(st1);
            style.Setters.Add(st2);
            style.Setters.Add(st3);
            style.Setters.Add(st4);

            ErrorPieChart.Style = style;

            EdgePanel ep = ObjectFinder.FindChild<EdgePanel>(ErrorPieChart, "ChartArea");
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

            Legend legend = ObjectFinder.FindChild<Legend>(ErrorPieChart, "Legend");
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

            ErrorPieChart.Palette = pieSeriesPalette;

            ((PieSeries)ErrorPieChart.Series[0]).ItemsSource =
            new KeyValuePair<string, double>[]{
            new KeyValuePair<string, double>("Error", errRate),
            new KeyValuePair<string, double>("Success", 1-errRate) };

            ErrorPerc.Content = "Error Percentage: " + Math.Round(errRate * 100, 4) + "%";

        }

        /// <summary>
        /// Create the Pie chart
        /// </summary>
        private void CreateChart()
        {
            var errRate = _analyser.CalculateErrorRateFromArray(_controller.Packets.Values.ToArray());

            SetupPieChart(errRate);
        }

        /// <summary>
        /// Reset all filters
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Reset(object sender, RoutedEventArgs e)
        {
            ChkErrorsOnly.IsChecked = false;

            addressSearch.Text = "";
            ProtocolSelected.SelectedValue = 0;

            _count = 2;
            _isUpArrow = false;
            SelectAllPorts(null, null);
                        
            try
            {
                StartTimeTextBox.Text = "";
                EndTimeTextBox.Text = "";
            }
            catch (Exception) { }
            
            // reset the colour of the Apply button1
            cmdApplyFilters.Background = (Brush)_brushConvertor.ConvertFromString("#FF37A300");
        }
        
        /// <summary>
        /// Move to the next page
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NextPage(object sender, RoutedEventArgs e)
        {
            if (SortedPackets.Count == 0) return;
            RemoveAllPackets();
            PageIndex++;

            for (int i = 0; i < 8; i++)
            {
                _previous[i] = new TimeSpan();
            }

            if (100 * (PageIndex + 1) > SortedPackets.Count)
            {
                NextPageBtn.Visibility = Visibility.Hidden;
            }
            else
            {
                NextPageBtn.Visibility = Visibility.Visible;
            }

            Packet[] toLoad = PageFetcher.FetchPage(SortedPackets);

            CreateAllTimeLabels(toLoad, false);
            AddPacketCollection(toLoad);

            if (PageIndex > 0)
            {
                PrevPageBtn.Visibility = Visibility.Visible;
            }
            else 
            {
                PrevPageBtn.Visibility = Visibility.Hidden;
            }



            PacketScroller.ScrollToVerticalOffset(0);

            UpdateShowingLabel();
        
        }

        /// <summary>
        /// Move to the previous page
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PrevPage(object sender, RoutedEventArgs e)
        {
            RemoveAllPackets();
            PageIndex--;

            for (int i = 0; i < 8; i++)
            {
                _previous[i] = new TimeSpan();
            }

            PrevPageBtn.Visibility = PageIndex == 0
                ? Visibility.Hidden
                : Visibility.Visible;

            Packet[] toLoad = PageFetcher.FetchPage(SortedPackets);

            CreateAllTimeLabels(toLoad, false);
            AddPacketCollection(toLoad);

            NextPageBtn.Visibility = Visibility.Visible;

            UpdateShowingLabel();

        }

        /// <summary>
        /// Displays which packets are being displayed ("Showing X - Y of Z packets")
        /// </summary>
        void UpdateShowingLabel() 
        {
            if (SortedPackets.Count == 0)
            {
                lblNumShowing.Visibility = System.Windows.Visibility.Hidden;

                NoPacketsArea.Width = new GridLength(50000, GridUnitType.Star);
                NoPacketsHead.Width = new GridLength(50000, GridUnitType.Star);
                TimeLabels.Width = new GridLength(0, GridUnitType.Pixel);
            }
            else
            {
                lblNumShowing.Visibility = System.Windows.Visibility.Visible;

                NoPacketsArea.Width = new GridLength(0, GridUnitType.Star);
                TimeLabels.Width = new GridLength(140, GridUnitType.Pixel);
                NoPacketsHead.Width = new GridLength(0, GridUnitType.Star);

                int start = (PageIndex * 100) + 1;

                int end;

                if ((start + 99) > SortedPackets.Count)
                {
                    end = SortedPackets.Count;
                }
                else 
                {
                    end = start + 99;
                }

                lblNumShowing.Content = "Showing " + start + " - " + end + " of " + SortedPackets.Count + " packets";

               
            }
        }
        
        /// <summary>
        /// When the user hovers on the Help icon
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Image_MouseEnter(object sender, MouseEventArgs e)
        {
            HelpPanel.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// When the user stops hovering on the Help icon
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Image_MouseLeave(object sender, MouseEventArgs e)
        {
            HelpPanel.Visibility = Visibility.Hidden;
        }

        /// <summary>
        /// Return to the File Selection panel
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GoBackToFileSelection(object sender, RoutedEventArgs e)
        {
            ResetWindow();
        }

        // <summary>
        /// Return to the File Selection panel
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ClearSelectedFiles(object sender, RoutedEventArgs e)
        {
            SelectedFiles.Children.Clear();
            _controller.FilePaths.Clear();
            _fileGrids.Clear();
        }     

        /// <summary>
        /// Reset all elements to go back to the file selection screen
        /// </summary>
        void ResetWindow()
        {
            _controller.Packets.Clear();         
            FileSelectedPane.Width = new GridLength(3, GridUnitType.Star);
            FiltersPane.Width = new GridLength(0, GridUnitType.Star);
            GraphPanelPie.Width = new GridLength(0, GridUnitType.Star);
            LeftSidePanel.Width = new GridLength(0, GridUnitType.Star);
            DataVisualisationPopup.Height = new GridLength(1, GridUnitType.Star);

            _isUpArrow = true;
            ImageBrush image = new ImageBrush(new BitmapImage(new Uri(@"pack://application:,,,/Resources/chart button.png")))
            {
                Stretch = Stretch.Uniform
            };

            DataVisButton.Background = image;

            DataRadioButtons.Visibility = Visibility.Hidden;

            SortedPackets.Clear();

            NextPageBtn.Visibility = Visibility.Hidden;
            PrevPageBtn.Visibility = Visibility.Hidden;
            DataVisButton3.Visibility = Visibility.Hidden;
            DataVisButton2.Visibility = Visibility.Hidden;
            DataVisButton.Visibility = Visibility.Hidden;

            HeightScroller.Visibility = Visibility.Hidden;

            RemoveAllPackets();

            NoPacketsArea.Width = new GridLength(50000, GridUnitType.Star);
            NoPacketsHead.Width = new GridLength(50000, GridUnitType.Star);
            lblNumShowing.Visibility = Visibility.Hidden;
            TimeLabels.Width = new GridLength(0, GridUnitType.Pixel); 
        }

        /// <summary>
        /// Apply the selected filters to the data 
        /// </summary>
        /// <param name="packets">The packets to filter through</param>
        /// <param name="start">The start time to look from</param>
        /// <param name="end">The end time to look up to</param>
        /// <returns></returns>
        private List<Packet> ApplyFilters(Packet[] packets, DateTime start, DateTime end)
        {
            var packetsFound = new List<Packet>();

            foreach (var packet in packets)
            {
                #region Time Checks
                var validTime = true;

                if ((start != new DateTime()) && (end != new DateTime()))
                {
                    validTime = LogicHelper.IsBetweenTimes(packet, start, end);
                }
                else if (start == new DateTime() && end == new DateTime())
                {
                    validTime = true;
                }
                else if (start == new DateTime())
                {
                    validTime = LogicHelper.IsBeforeTime(packet, end);
                }
                else if (end == new DateTime())
                {
                    validTime = LogicHelper.IsAfterTime(packet, start);
                }
                #endregion
                #region Error Checks
                var matchesError = true;
                var errorsOnly = !ChkErrorsOnly.IsChecked;
                if (errorsOnly != null && !((bool)errorsOnly || packet.IsError))
                {
                    matchesError = false;
                }
                #endregion
                #region Protocol checks
                var protoSearch = ProtocolSelected.Text.Split('(');

                bool validProtocol;
                if (protoSearch.Length > 1 && protoSearch[0].Length > 0)
                {
                    validProtocol = ProtocolSelected.SelectedIndex == 0 
                        ? LogicHelper.HexAddressSearch(packet, protoSearch[0].Trim())
                        : LogicHelper.DecimalProtocolSearch(packet, protoSearch[0].Trim());
                }
                else
                {
                    validProtocol = true;
                }
                #endregion
                #region Address checks

                var addrSearch = addressSearch.Text.Trim();
                bool validAddress;
                var typeOfSearch = AddressTypeDropdown.Text;

                if (addrSearch.Length > 0)
                {
                    validAddress = typeOfSearch == "0d" 
                        ? LogicHelper.DecimalAddressSearch(packet, addrSearch) 
                        : LogicHelper.HexAddressSearch(packet, addrSearch);
                }
                else
                {
                    validAddress = true;
                }

                #endregion
                
                if (validTime && matchesError && validProtocol && validAddress)
                {
                    packetsFound.Add(packet);
                }
            }
            return packetsFound;
        }
                
        /// <summary>
        /// When the user clicks on the 'Apply filters' button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cmdApplyFilters_Click(object sender, RoutedEventArgs e)
        {
            RemoveAllPackets();
            SortedPackets.Clear();

            var start = StartTimeTextBox.Text;
            var end = EndTimeTextBox.Text;
            DateTime startTime = new DateTime();
            DateTime endTime = new DateTime();

            bool apply = true;

            // check if a valid start/end DateTime has been entered
            if (start != "")
            {
                try
                {
                    startTime = DateTime.ParseExact(start, "dd-MM-yyyy HH:mm:ss.fff", null);
                }
                catch
                {
                    MessageBox.Show("You have entered an invalid time for the start date.");
                    apply = false;
                }
            }
            if (end != "")
            {
                try
                {
                    endTime = DateTime.ParseExact(end, "dd-MM-yyyy HH:mm:ss.fff", null);
                }

                catch
                {
                    apply = false;
                }
            }

            // if the dates are valid, apply the filters and display the packets
            if (apply)
            {
                List<Packet> packets = ApplyFilters(_controller.Packets.Values.ToArray(), startTime, endTime);

                SortedPackets = (from pair in packets orderby pair.DateReceived ascending select pair).ToList();

                PageIndex = 0;

                SetupElements(false);

                // reset _previous for the next load
                for (int i = 0; i < 8; i++)
                {
                    _previous[i] = new TimeSpan();
                }
            }
        }

        /// <summary>
        /// Setup the elements to display packets
        /// </summary>
        void SetupElements(bool doubleInterval) 
        {
            Packet[] toLoad = PageFetcher.FetchPage(SortedPackets);


            TimeList.Children.Clear();
            RemoveAllPackets();

            CreateAllTimeLabels(toLoad, doubleInterval);
            AddPacketCollection(toLoad);
            CreateDataRateGraph(SortedPackets.ToArray());


            cmdApplyFilters.Background = (Brush)_brushConvertor.ConvertFromString("#FF4A4D54");

            PacketScroller.ScrollToVerticalOffset(0);

            PrevPageBtn.Visibility = Visibility.Hidden;

            NextPageBtn.Visibility = ((PageIndex + 1)* 100) > SortedPackets.Count 
                ? Visibility.Hidden
                : Visibility.Visible;


            UpdateShowingLabel();
            
        }

        /// <summary>
        /// When the user types in the address box
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void addressSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            cmdApplyFilters.Background = (Brush)_brushConvertor.ConvertFromString("#FF37A300");
        }

        /// <summary>
        /// When the user types in the protocol box
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void protocolSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            cmdApplyFilters.Background = (Brush)_brushConvertor.ConvertFromString("#FF37A300");
        }

        /// <summary>
        /// When the user types in the Start Time textbox
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void StartTimeTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            cmdApplyFilters.Background = (Brush)_brushConvertor.ConvertFromString("#FF37A300");
        }

        /// <summary>
        /// When the user types in the End Time box
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EndTimeTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            cmdApplyFilters.Background = (Brush)_brushConvertor.ConvertFromString("#FF37A300");
        }

        /// <summary>
        /// Collapse/expand the errorlist
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cmdCollapseErrorList_Click(object sender, RoutedEventArgs e)
        {
            // toggle the bool and set the heights and images accordingly
            _isErrorListOpen = !_isErrorListOpen;

            if (_isErrorListOpen)
            {
                ErrorCollapse.Height = new GridLength(4, GridUnitType.Star);
                ErrorAreaCollapse.Height = new GridLength(2.5, GridUnitType.Star);                 
                ImageBrush image = new ImageBrush(new BitmapImage(new Uri(@"pack://application:,,,/Resources/minus.png")));
                cmdCollapseErrorList.Background = image;
                ErrorDescription.Height = new GridLength(1, GridUnitType.Star);
            }
            else
            {
                ErrorCollapse.Height = new GridLength(0, GridUnitType.Star);
                ErrorAreaCollapse.Height = new GridLength(.3, GridUnitType.Star);
                ImageBrush image = new ImageBrush(new BitmapImage(new Uri(@"pack://application:,,,/Resources/plus.png")));
                ErrorDescription.Height = new GridLength(0, GridUnitType.Star);
                cmdCollapseErrorList.Background = image;
            }
        }

        /// <summary>
        /// Collapse/expand the errorlist
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cmdCollapseFilesDisplayed_Click(object sender, RoutedEventArgs e)
        {
            // toggle the bool and set the heights and images accordingly
            _isFilesDisplayedOpen = !_isFilesDisplayedOpen;

            if (_isFilesDisplayedOpen)
            {
                FileDisplayCollapse.Height = new GridLength(4, GridUnitType.Star);
                FileAreaCollapsible.Height = new GridLength(2.5, GridUnitType.Star);
                ImageBrush image = new ImageBrush(new BitmapImage(new Uri(@"pack://application:,,,/Resources/minus.png")));
                cmdCollapseFilesDisplayed.Background = image;
            }
            else
            {
                FileDisplayCollapse.Height = new GridLength(0, GridUnitType.Star);
                FileAreaCollapsible.Height = new GridLength(.36, GridUnitType.Star);
                ImageBrush image = new ImageBrush(new BitmapImage(new Uri(@"pack://application:,,,/Resources/plus.png")));
                cmdCollapseFilesDisplayed.Background = image;
            }
        }

        /// <summary>
        /// When the user clicks an error in the Error list, find the errounous packet and go to it
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GoToPacket(object sender, RoutedEventArgs e) 
        {
            string tag = ((Button)sender).Tag.ToString();

            string[] split = tag.Split('@');

            int foundIndex = -1;
            for (int i = 0; i < SortedPackets.Count; i++ )
            {
                if (SortedPackets[i].PacketId.ToString() == split[1])
                {
                    foundIndex = i;
                    break;
                }
            }

            if (foundIndex > -1)
            {
                PageIndex = (foundIndex / 100) - 1;
                NextPage(null, null);

                var portToSearch = int.Parse(split[0]);

                var time = DateTime.ParseExact(split[2], "dd-MM-yyyy HH:mm:ss.fff", null);

                _lblFoundObj = FindObjectInPort(portToSearch, time);

                if (_lblFoundObj != null)
                {
                    // scroll to the label
                    _lblFoundObj.BringIntoView();
                    
                    var copy = _lblFoundObj.Background;
                    _colourBeforeHighlight = copy;
                    _lblFoundObj.Background = Brushes.LimeGreen;
                    _showHighlightedPacket = new System.Timers.Timer();
                    _showHighlightedPacket.Interval = 1400;
                    _showHighlightedPacket.Elapsed += _showHighlightedPacket_Elapsed;
                    _showHighlightedPacket.Start();

                }
                else 
                {
                    MessageBox.Show("This packet is not currntly being displayed - please check your filters");
                }


            }
            else 
            {
                MessageBox.Show("Error is not currently being shown - please adjust your filters");
            }
            
        }

        Button FindObjectInPort(int portToSearch, DateTime time) 
        {
            var timeLabels = TimeList.Children.OfType<Button>();
            
            foreach (var t in timeLabels)
            {
                string n = t.Content.ToString();

                var dt = DateTime.ParseExact(time.ToString("dd-MM-yyyy") + " " + n, "dd-MM-yyyy HH:mm:ss.fff", null);

                TimeSpan ts = time - dt;

                if (ts < _section)
                {
                    _lblFoundObj = (Button)t;
                    break;
                }
            }

            return _lblFoundObj;

        }

        /// <summary>
        /// Return the Time label to the original colour
        /// </summary>
        private void ChangeImageColour() 
        {
            _lblFoundObj.Background = _colourBeforeHighlight;
        }

        /// <summary>
        /// Make the Time label, corresponding to the selected error, Green for X amount of time
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="tee"></param>
        private void _showHighlightedPacket_Elapsed(object sender, ElapsedEventArgs eea)
        {
            _lblFoundObj.Dispatcher.Invoke(new ResetColour(ChangeImageColour));

            _showHighlightedPacket.Stop();
        }

        /// <summary>
        /// Get the style for an error
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        public static Style GetErrorStyle(double val)
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
        /// Get the style for a successful packet
        /// </summary>
        /// <param name="val">The height of the object</param>
        /// <returns></returns>
        private static Style GetSuccessStyle(double val)
        {
            var style = new Style { TargetType = typeof(Button) };
            style.Setters.Add(new Setter(MarginProperty, new Thickness(0, 0, 0, (val / 10) - 1)));
            style.Setters.Add(new Setter(HorizontalAlignmentProperty, HorizontalAlignment.Stretch));
            style.Setters.Add(new Setter(HorizontalContentAlignmentProperty, HorizontalAlignment.Center));
            style.Setters.Add(new Setter(VerticalAlignmentProperty, VerticalAlignment.Center));
            style.Setters.Add(new Setter(ForegroundProperty, Brushes.Black));

            var converter = new BrushConverter();
            style.Setters.Add(new Setter(BackgroundProperty, (Brush)converter.ConvertFromString("#6699ff")));
            style.Setters.Add(new Setter(HeightProperty, val));

            return style;
        }

        /// <summary>
        /// Get the style for a timestamp
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        public Style GetTimeStyle(double val)
        {
            var style = new Style { TargetType = typeof(Button) };
            style.Setters.Add(new Setter(MarginProperty, new Thickness(0, 0, 0, (val / 10) - 1)));
            style.Setters.Add(new Setter(HorizontalAlignmentProperty, HorizontalAlignment.Stretch));
            style.Setters.Add(new Setter(HorizontalContentAlignmentProperty, HorizontalAlignment.Center));
            style.Setters.Add(new Setter(VerticalAlignmentProperty, VerticalAlignment.Center));
            style.Setters.Add(new Setter(VerticalContentAlignmentProperty, VerticalAlignment.Center));
            style.Setters.Add(new Setter(ForegroundProperty, Brushes.Black));
            style.Setters.Add(new Setter(BorderThicknessProperty, new Thickness(0)));

            var converter = new BrushConverter();

            style.Setters.Add(new Setter(BackgroundProperty, (Brush)converter.ConvertFromString("#d9d9d9")));
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

            var converter = new BrushConverter();

            style.Setters.Add(new Setter(BackgroundProperty, (Brush)converter.ConvertFromString("#b383d3")));
            style.Setters.Add(new Setter(HeightProperty, val));

            return style;
        }

        private void BackToNormalPage_Click(object sender, RoutedEventArgs e)
        {
            //TODO: restore to previous data
            SortedPackets = _tempStore.ToList();

            SetupElements(false);


            if (!_isLeftArrow)
            {
                FiltersPane.Width = new GridLength(3, GridUnitType.Star);
            }
            else
            {
                FiltersPane.Width = new GridLength(0, GridUnitType.Star);
            }

            if (!_isRightArrow)
            {
                GraphPanelPie.Width = new GridLength(3, GridUnitType.Star);
            }
            else
            {
                GraphPanelPie.Width = new GridLength(0, GridUnitType.Star);
            }

            if (!_isUpArrow)
            {
                DataVisualisationPopup.Height = new GridLength(10, GridUnitType.Star);
            }
            else
            {
                DataVisualisationPopup.Height = new GridLength(1, GridUnitType.Star);
            }
            

            // restore panels to the correct sizes
            RightButtonColumn.Width = new GridLength(0.25, GridUnitType.Star);
            LeftPanelButton.Width = new GridLength(1, GridUnitType.Star);
            PortHeadings.Height = new GridLength(1, GridUnitType.Star);
            cmdBackToNormalData.Height = new GridLength(0, GridUnitType.Star);
            TimeLabels.Width = new GridLength(140, GridUnitType.Pixel);
            TimeHeader.Width = new GridLength(140, GridUnitType.Pixel);
            HeightScroller.Visibility = System.Windows.Visibility.Visible;
        }

        private void ProtocolSelected_SelectionChanged(object sender, EventArgs e)
        {
            if (ProtocolSelected.Text == "Add New Protocol") 
            {
                // add protocol to list
                ProtocolCreator pc = new ProtocolCreator();
                pc.ShowDialog();

                if (ProtocolCreator.CreatedObject.Key != -1)
                {
                    KeyValuePair<int, string> kvp = ProtocolCreator.CreatedObject;

                    // add to list
                    ComboBoxItem cb = new ComboBoxItem();
                    cb.Content = kvp.Key + " (" + kvp.Value + ")";
                    cb.FontFamily = new FontFamily("Gill Sans MT");
                    cb.FontSize = 14;
                    cb.VerticalContentAlignment = System.Windows.VerticalAlignment.Center;
                    cb.HorizontalContentAlignment = System.Windows.HorizontalAlignment.Left;
                    cb.VerticalAlignment = System.Windows.VerticalAlignment.Stretch;
                    cb.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
                    cb.MouseRightButtonUp += RemoveProtocol;

                    ProtocolSelected.Items.Add(cb);

                    ProtocolSelected.SelectedValue = ProtocolSelected.Items.Count - 1;
                }
                else 
                {
                    ProtocolSelected.SelectedValue = 0;
                }

            }

            cmdApplyFilters.Background = (Brush)_brushConvertor.ConvertFromString("#FF37A300");
        }


        private string[] lineDisplayLabels = new string[] { "How many packets were received", "How many errors were picked up", "How many data characters were read" }; 

        private void ChangeLineGraphData(object sender, EventArgs e)
        {
            try
            {
                var id = int.Parse(((RadioButton)sender).Tag.ToString());

                FormatLineChart(id);

                lblLineGraphDisplaying.Text = "Currently Displaying: " + lineDisplayLabels[id] + " between the intervals shown";

            }
            catch (Exception) { }
        }

        private void OpenTimeEdit(object sender, RoutedEventArgs e)
        {
            var tag = int.Parse(((Button)sender).Tag.ToString());

            string content = "";
            string timeToChange = "";
            if (tag == 0) { content = StartTimeTextBox.Text; timeToChange = "Start"; }
            if (tag == 1) { content = EndTimeTextBox.Text; timeToChange = "End"; }

            TimeSelector ts = new TimeSelector();
            ts.SetupElements(content, timeToChange);

            ts.ShowDialog();

            if(ts.DateCreated != "")
            {
                if (tag == 0) { StartTimeTextBox.Text = ts.DateCreated; }
                if (tag == 1) { EndTimeTextBox.Text = ts.DateCreated; }
            }

        }
    }
}