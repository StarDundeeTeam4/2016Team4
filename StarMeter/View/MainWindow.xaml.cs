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


namespace StarMeter.View
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private readonly Controller _controller = new Controller();

        private readonly StackPanel[] _portStacks = new StackPanel[8];
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

            _loadingTimer = new System.Timers.Timer();
            _loadingTimer.Elapsed += _LoadingTimer_Elapsed;
            _loadingTimer.Interval = 100;

            //_LoadingTimer.Start();
        }

        private void ChangeAnimFrame()
        {
            if (_animCount >= _gifDecoder.Frames.Count - 1)
            {
                _animCount = 0;
            }
            LoadingIcon.Source = _gifDecoder.Frames[_animCount];
        }

        public void ChangeDots() 
        {
            int dots = _animCount / 6;
            string[] split = LoadingMessage.Content.ToString().Split('.');
            string dottage = "";

            for (int i = 0; i < dots; i++)
            {
                dottage += ".";
            }

			
            LoadingMessage.Content = split[0] + dottage;
        }

        private void _LoadingTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            _animCount++;
            LoadingIcon.Dispatcher.Invoke(new UpdateAnimation(ChangeAnimFrame));
            LoadingMessage.Dispatcher.Invoke(new UpdateAnimation(ChangeDots));
        }

        private readonly GifBitmapDecoder _gifDecoder;
        private int _animCount;

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

        private readonly System.Timers.Timer _loadingTimer;

        private void CreateTimeLabel(TimeSpan time)
        {
            // create a label for the time
            # region Time label
            Label l = new Label();
            l.Content = time.ToString();
            l.SetResourceReference(Control.StyleProperty, "Timestamp");

            TimeList.Children.Add(l);
            #endregion
        }

        public Button GetPacketButton(Packet p, string nameToSet)
        {
            #region Create Button for the packet
            string sty = "";

            var b = new Button();
            b.Click += OpenPopup;

            string nameOutput = nameToSet.Replace('.', 'M').Replace(':', '_');

            var lab = new Label();

            try
            {
                lab.Content = p.DateRecieved.ToString("HH:mm:ss.fff");

                var addressArray = p.Address;
                var finalAddressString = "";

                if (addressArray != null)
                {
                    if (addressArray.Length > 1)
                    {
                        finalAddressString += "Physical Path: ";
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
                    lab.Content = (lab.Content) + Environment.NewLine + "Protocol: " + protocolId + " (RMAP)";
                }
                else
                {
                    lab.Content = (lab.Content) + Environment.NewLine + "Protocol: " + protocolId;
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

            StackPanel stackPan = GetPanelToUse(p.PortNumber);

            return b;
            #endregion
        }

        // work out which panel to assign the packet to
        private StackPanel GetPanelToUse(int portNum)
        {
            return _portStacks[portNum - 1];
        }

        //This function takes a list of packets to display on the screen.
        //As we use pagination we only send a page worth of packets at one time.
        public void AddPacketCollection(Packet[] packets)
        {
            foreach (Packet p in packets)
            {
                if (_previous[p.PortNumber - 1] == new TimeSpan())
                {
                    _previous[p.PortNumber - 1] = p.DateRecieved.TimeOfDay;
                }
            }
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
                if(packets.Length > 0)
                {
                    tempTimespans.Add(new KeyValuePair<int, TimeSpan>(0, packets[0].DateRecieved.TimeOfDay));
                }
            }

            foreach (var p in packets)
            {
                AddPacket(p, tempTimespans); //Places a single packet on the screen.
            }

            var total = SortedPackets.Count;
            var start = _pageIndex * 100 + 1;
            var end = start + 99;

            if (end > total)
            {
                end = total;
            }

            if(packets.Length == 0)
            {
                lblNumShowing.Content = "No packets to display";
            }
            else
            {
                lblNumShowing.Content = "Showing " + start + " - " + end + " of " + total + " packets";

                StartTimeTextBox.Text = packets[0].DateRecieved.ToString("dd-MM-yyyy HH:mm:ss.fff");
                EndTimeTextBox.Text = packets[packets.Length - 1].DateRecieved.ToString("dd-MM-yyyy HH:mm:ss.fff");

            }
        }

        private readonly TimeSpan[] _previous = new TimeSpan[8];

        private readonly List<List<Guid>[]> _timeSpanOccupied = new List<List<Guid>[]>();

        private void AddPacket(Packet p, List<KeyValuePair<int, TimeSpan>> tempTimespans)
        {
            if (_previous[p.PortNumber - 1] != null && (_previous[p.PortNumber - 1].Seconds - p.DateRecieved.Second) >= 30) //if more than 30 seconds have passed between packets.
            {
                    //NEW TRANSMISSION
            }
            // var temp_timespans = _timespans.ToList();
            var packetTimespan = p.DateRecieved.TimeOfDay;
            var sp = GetPanelToUse(p.PortNumber);
            bool found = false;
            int index = 0;

            //The following is a binary search algorithm to find which timestamp matches the current packet in O(log n) time.
            //As our display contains a representation of time passing we cut the list in half and traverse through the list until we find
            //a timestamp which is "close enough." This is done using sections as it is extremely unlikely thatthe packet we have perfectly
            //matches a timestamp.
            while (found == false && tempTimespans.Count > 0)
            {
                index = tempTimespans.Count / 2;

                if (tempTimespans[index].Value >= packetTimespan)
                {
                    if (tempTimespans[index].Value.Add(HalfSection) < packetTimespan)
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
                    if (tempTimespans[index].Value.Add(_section) > packetTimespan)
                    {
                        found = true;
                    }
                    else
                    {
                        tempTimespans = tempTimespans.GetRange(index, tempTimespans.Count - index);
                    }
                }
            }

            _timeSpanOccupied[tempTimespans[index].Key][p.PortNumber].Add(p.PacketId);

            //The following code handles the situation where multiple packets fit into the same timestamp.
            var currentNumber = _timeSpanOccupied[tempTimespans[index].Key][p.PortNumber].Count;
            if (currentNumber > 1)
            {
                Console.WriteLine("CLASH " + tempTimespans[index].Key);
                var childObjs = GetPanelToUse(p.PortNumber).Children;
                string toFind = "btn" + tempTimespans[index].Value.ToString().Replace('.', 'M').Replace(':', '_');
                StackPanel stackPan = GetPanelToUse(p.PortNumber);

                //for (int i = 0; i < childObjs.Count; i++)
                //{
                //    try
                //    {
                //        if (((Button)childObjs[i]).Name == toFind)
                //        {
                //            ((Button)childObjs[i]).Background = Brushes.Yellow;
                //            break;
                //        }
                //    }
                //    catch (InvalidCastException) { }
                //}

                var existingBtn = (Button)sp.FindName(toFind);
                var btn = (Button)LogicalTreeHelper.FindLogicalNode(stackPan, toFind);

                if (btn.Background == Brushes.Red || p.IsError) 
                {
                    btn.Background = Brushes.Red;
                }
                else
                {
                    btn.Background = Brushes.Yellow;
                }

                // clear all event handlers here

                btn.Click -= OpenPopup;
                
                btn.Click -= ViewMultiplePackets;
                btn.Click -= ViewMultiplePackets;
                btn.Click -= ViewMultiplePackets;
                btn.Click -= ViewMultiplePackets;
                btn.Click -= ViewMultiplePackets;
                btn.Click -= ViewMultiplePackets;   // may have been assigned multiple times - just make sure!

                btn.Tag = tempTimespans[index].Key + "@" + p.PortNumber;

                btn.Click += ViewMultiplePackets;

                Label l = (Label)btn.Content;
                btn.Tag = tempTimespans[index].Key + "@" + p.PortNumber;
                l.Content = currentNumber;
            }
            else //we only have one packet to display.
            {
                var diff = (tempTimespans[index].Value - _previous[p.PortNumber - 1]); //This is the difference in time between the last packet and this one.
                int spaces = 0; 

                //In order to determine how many blank spaces to display, we continually remove a "section" from the difference
                //incrementing the number of spaces to add by one each time.
                while (diff.CompareTo(new TimeSpan(0, 0, 0, 0, _interval)) > 0) 
                {
                    diff = diff.Add(NegativeSection);
                    spaces++;
                }

                for (int i = 0; i < spaces; i++)
                {
                    Label lbl = new Label(); //Empty label filling the size that one packet takes up.
                    lbl.SetResourceReference(Control.StyleProperty, "TimeFiller");
                    sp.Children.Add(lbl);
                }

                var b = GetPacketButton(p, tempTimespans[index].Value.ToString());

                sp.Children.Add(b);
                _previous[p.PortNumber - 1] = tempTimespans[index].Value;
            }
        }

        //This function will remove all packets from the screen which are being displayed.
        public void RemoveAllPackets()
        {
            for (int i = 0; i < 8; i++)
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
            var values = a.GetDataForLineChart(SortedPackets.ToArray());


            if (!(bool)ChkErrorsOnly.IsChecked)
            {
                var lineSeries1 = new LineSeries
                {
                    IsSelectionEnabled = true,
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

        private void ViewMultiplePackets(object sender, RoutedEventArgs e)
        {
            MultiplePacketPopup mpp = new MultiplePacketPopup(_controller);
            string[] split = ((Button)sender).Tag.ToString().Split('@');
            var id = int.Parse(split[0]);
            var port = int.Parse(split[1]);
            List<Guid> guids = _timeSpanOccupied[id][port];
            List<Packet> ps = new List<Packet>();

            foreach (Guid g in guids)
            {
                ps.Add(FindPacket(g));
            }

            mpp.CreateElements(ps);
            mpp.ShowDialog();
        }

        //This will allow us to read the files or remove the files later.
        private readonly List<Grid> _fileGrids = new List<Grid>();

        private void FileSelection(object sender, RoutedEventArgs e)
        {
            LoadingIcon.Visibility = Visibility.Visible;
            LoadingMessage.Visibility = Visibility.Visible;
            LoadingMessage.Content = "Selecting File";

            _loadingTimer.Start();

            var ofd = new OpenFileDialog
            {
                // only allow .rec files
                Filter = "Record Files (.rec)|*.rec",
                Multiselect = true
            };

            bool? confirmed = ofd.ShowDialog();

            if (confirmed != true) 
            {              
                LoadingIcon.Visibility = Visibility.Hidden;
                LoadingMessage.Visibility = Visibility.Hidden;
                return;
            }

            LoadingMessage.Content = "Selecting File";

            // display file name
            List<string> filesAdded = _controller.AddFileNames(ofd.FileNames);

            foreach (string fileName in filesAdded)
            {
                string actualName = fileName.Split('.')[0];
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

            LoadingIcon.Visibility = System.Windows.Visibility.Hidden;
            LoadingMessage.Visibility = System.Windows.Visibility.Hidden;

            _loadingTimer.Stop();

        }

        //Removes a file from the list of files selected by the user.
        private void CancelUpload(object sender, RoutedEventArgs e)
        {
            Button b = (Button)sender;
            string tag = b.Tag.ToString(); //fileName
            int id = _controller.RemoveFile(tag);

            SelectedFiles.Children.RemoveAt(id);
            _fileGrids.RemoveAt(id);
        }

        //This opens the PacketPopup.xaml dialog.
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

        //Searches through the packets via a user entered address.
        private void SearchForAddress(object sender, RoutedEventArgs e)
        {
            var packList = SearchForAddress2(); 
            try
            {
                CreateAllTimeLabels(packList.Take(100).ToArray());
                AddPacketCollection(packList.ToArray().Take(100).ToArray());
            }
            catch
            {
                if (packList.Count > 0)
                {
                    CreateAllTimeLabels(packList.Take(packList.Count).ToArray());
                    AddPacketCollection(packList.ToArray().Take(packList.Count).ToArray());
                }
            }
        }
        private List<Packet> SearchForAddress2()
        {
            RemoveAllPackets();
            _pageIndex = 0;
            PrevPageBtn.Visibility = Visibility.Hidden;
            var search = addressSearch.Text;
            if(search == "")
            {
                return _controller.packets.Values.ToList();
            }
            var packList = new List<Packet>();

            foreach (var packet in SortedPackets)
            {
                var packetAddress = packet.Address;
                if (packetAddress != null && packetAddress.GetValue(0).ToString() == search)
                {
                    packList.Add(packet);
                }
            }

            return packList;

        }

        private void SearchForProtocol(object sender, RoutedEventArgs e)
        {
            RemoveAllPackets();
            _pageIndex = 0;
            PrevPageBtn.Visibility = Visibility.Hidden;
            var search = protocolSearch.Text;
            var packList = new List<Packet>();

            foreach (var packet in SortedPackets)
            {
                var packetProtocol = packet.ProtocolId;
                if (packetProtocol.ToString() == search)
                {
                    packList.Add(packet);
                }
            }

            try
            {
                CreateAllTimeLabels(packList.Take(100).ToArray());
                AddPacketCollection(packList.Take(100).ToArray());
            }
            catch
            {
                CreateAllTimeLabels(packList.Take(packList.Count).ToArray());
                AddPacketCollection(packList.Take(packList.Count).ToArray());
            }
            //protocolSearch.Text = "";
        }

        private Packet FindPacket(Guid guid)
        {
            // TODO: change this to be a lookup from dictionary

            return SortedPackets.FirstOrDefault(p => guid.Equals(p.PacketId));
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

        private bool _isRightArrow;

        private void ShowDataVisPopup2(object sender, RoutedEventArgs e)
        {
            ImageBrush image;

            if (_isRightArrow)
            {
                // GraphPanelPie.Width = new GridLength(0, GridUnitType.Star);
                image = new ImageBrush(new BitmapImage(new Uri(@"pack://application:,,,/Resources/right-arrow.png")));
            }
            else
            {
                // GraphPanelPie.Width = new GridLength(3, GridUnitType.Star);
                image = new ImageBrush(new BitmapImage(new Uri(@"pack://application:,,,/Resources/left-arrow.png")))
                {
                    //Stretch = Stretch.UniformToFill
                };

            }

            if (_t2 == null)
            {
                _t2 = new System.Timers.Timer();
                _t2.Elapsed += TimerEventProcessor2;
                _t2.Interval = 10;
                _t2.Start();
            }

            DataVisButton2.Background = image;
        }

        public delegate void UpdateSlider();
        public delegate void UpdateAnimation();

        // This is the method to run when the timer is raised.
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

            if ((_count > 10 && _isUpArrow) || (_count < 2 && !_isUpArrow))
            {
                _t.Stop();

                DataVisualisationPopup.Dispatcher.Invoke(new UpdateSlider(FixStretch));

                _isUpArrow = !_isUpArrow;
                _t = null;
            }

            DataVisualisationPopup.Dispatcher.Invoke(new UpdateSlider(MoveSlider));
        }

        private void TimerEventProcessor2(object myObject, EventArgs myEventArgs)
        {
            // Restarts the timer and increments the counter.
            if (_isRightArrow)
            {
                _count2 += 0.5;
            }
            else
            {
                _count2 -= 0.5;
            }

            if ((_count2 > 2.5 && _isRightArrow) || (_count2 < 0.5 && !_isRightArrow))
            {
                _t2.Stop();

                //GraphPanelPie.Dispatcher.Invoke(new UpdateSlider(FixStretch));

                _isRightArrow = !_isRightArrow;
                _t2 = null;
            }

            GraphPanelPie.Dispatcher.Invoke(new UpdateSlider(MoveSlider2));
        }

        System.Timers.Timer _t;
        private int _count;

        System.Timers.Timer _t2;
        private double _count2 = 2.5;

        /// <summary>
        /// set the height of the packet buttons
        /// </summary>
        private void MoveSlider()
        {
            DataVisualisationPopup.Height = new GridLength(_count, GridUnitType.Star);
        }

        private void MoveSlider2()
        {
            GraphPanelPie.Width = new GridLength(_count2, GridUnitType.Star);
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
        private static Style GetErrorStyle(double val)
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
            var style = new Style { TargetType = typeof(Label) };
            style.Setters.Add(new Setter(MarginProperty, new Thickness(0, 0, 0, (val / 10) - 1)));
            style.Setters.Add(new Setter(HorizontalAlignmentProperty, HorizontalAlignment.Stretch));
            style.Setters.Add(new Setter(HorizontalContentAlignmentProperty, HorizontalAlignment.Center));
            style.Setters.Add(new Setter(VerticalAlignmentProperty, VerticalAlignment.Center));
            style.Setters.Add(new Setter(VerticalContentAlignmentProperty, VerticalAlignment.Center));
            style.Setters.Add(new Setter(ForegroundProperty, Brushes.Black));

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

        public List<Packet> SortedPackets = new List<Packet>();

        private void cmdBeginAnalysis_Click(object sender, RoutedEventArgs e)
        {
            if (_controller.filePaths.Count < 1)
            {
                MessageBox.Show("No Files Selected");
            }
            else
            {
                for (int i = 0; i < 8; i++)
                {
                    _previous[i] = new TimeSpan();
                }

                FiltersPane.Width = new GridLength(3, GridUnitType.Star);
                FileSelectedPane.Width = new GridLength(0, GridUnitType.Star);

                RemoveAllPackets();

                var packets = _controller.ParsePackets().ToList();

                //packets = controller.packets.Values.ToList();

                SortedPackets = (from pair in _controller.packets orderby pair.Value.DateRecieved ascending select pair.Value).ToList();

                Packet[] firstLoad;
                try
                {
                    firstLoad = SortedPackets.GetRange(0, 100).ToArray();
                    //firstLoad = sortedPackets.Values.ToList().GetRange(0, 100);

                    //List<Packet> packsss = ;

                    //firstLoad = packsss.GetRange(0, 100).ToArray();
                }
                catch (Exception)
                {
                    //firstLoad = null;
                    firstLoad = SortedPackets.ToArray();
                }

                CreateAllTimeLabels(firstLoad);
                AddPacketCollection(firstLoad);
                CreateChart();
                DataVisButton2.Visibility = Visibility.Visible;
                CreateDataRateGraph(SortedPackets.ToArray());

                if (SortedPackets.Count < 100) { NextPageBtn.Visibility = Visibility.Hidden; } else { NextPageBtn.Visibility = Visibility.Visible; }

            }

        }

        private int _interval;
        private TimeSpan _section;
        public TimeSpan NegativeSection;
        public TimeSpan HalfSection;
        private TimeSpan[] _timespans;

        private void CreateAllTimeLabels(Packet[] packets)
        {
            _timeSpanOccupied.Clear();
            var l = new List<Guid>[8];

            for (int j = 0; j < l.Length; j++)
            {
                l[j] = new List<Guid>();
            }

            _timeSpanOccupied.Add(l);

            var tStart = new TimeSpan();
            var timelist = new List<TimeSpan>();
            try
            {
                tStart = packets[0].DateRecieved.TimeOfDay;
            }
            catch
            {
                return; //This code executes if there are no packets to show.
            }

            var tEnd = packets[packets.Length - 1].DateRecieved.TimeOfDay;
            var timeDiff = tEnd - tStart;

            if (timeDiff.TotalMilliseconds == 0) 
            {
                if (packets.Length > 0) 
                {
                    var list = new List<KeyValuePair<int, TimeSpan>>
                    {
                        new KeyValuePair<int, TimeSpan>(0, packets[0].DateRecieved.TimeOfDay)
                    };
                    CreateTimeLabel(packets[0].DateRecieved.TimeOfDay);
                    //AddPacket(packets[0], list);
                }
            }
            else
            {
                var milli = (int)timeDiff.TotalMilliseconds;

                _interval = milli / packets.Length / 2;
                //interval = 300;

                _section = new TimeSpan(0, 0, 0, 0, _interval);
                NegativeSection = _section.Negate();
                HalfSection = new TimeSpan(0, 0, 0, 0, -_interval / 2);
                var i = 0;

                var curr = tStart.Add(new TimeSpan(0, 0, 0, 0, (int)(_interval * i)));
                timelist.Add(curr);

                var list = new List<Guid>[8];
                for (int j = 0; j < list.Length; j++)
                {
                    list[j] = new List<Guid>();
                }

                _timeSpanOccupied.Add(list);

                while (curr <= tEnd)
                {
                    i++;
                    CreateTimeLabel(curr);

                    curr = tStart.Add(new TimeSpan(0, 0, 0, 0, (int)(_interval * i)));
                    timelist.Add(curr);

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

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            RemoveAllPackets();

            var packList = new List<Packet>();

            foreach (var packs in _controller.packets.Values)
            {
                Packet p = (Packet)packs;
                if (p.IsError)
                {
                    packList.Add(p);
                }
            }

            CreateAllTimeLabels(packList.ToArray());
            AddPacketCollection(packList.ToArray());
            CreateDataRateGraph(_controller.packets.Values.ToArray());
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            RemoveAllPackets();

            Packet[] packets = new Packet[_controller.packets.Count];

            int count = 0;
            foreach (var p in _controller.packets.Values)
            {
                if (count < 100)
                {
                    packets[count] = (Packet)p;
                    count++;
                }
                else { break; }
            }

            CreateAllTimeLabels(packets);
            AddPacketCollection(packets);
            CreateDataRateGraph(packets);
        }

        private void CreateChart()
        {
            Analyser a = new Analyser();
            double errRate = a.CalculateErrorRateFromArray(_controller.packets.Values.ToArray());


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

            ((PieSeries)mcChart.Series[0]).ItemsSource =
            new KeyValuePair<string, double>[]{
            new KeyValuePair<string, double>("Error", errRate),
            new KeyValuePair<string, double>("Success", 1-errRate) };

            RightButtonColumn.Width = new GridLength(0.25, GridUnitType.Star);
            GraphPanelPie.Width = new GridLength(2.5, GridUnitType.Star);

        }

        private void Reset(object sender, RoutedEventArgs e)
        {
            ChkErrorsOnly.IsChecked = false;

            _controller.packets.Clear();
            _controller.filePaths.Clear();

            SelectedFiles.Children.Clear();
            _fileGrids.Clear();

            RemoveAllPackets();

            RightButtonColumn.Width = new GridLength(0, GridUnitType.Star);
            GraphPanelPie.Width = new GridLength(0, GridUnitType.Star);

            CreateDataRateGraph(_controller.packets.Values.ToArray());

            _count = 2;
            _isUpArrow = false;
            ShowDataVisPopup(null, null);
            SelectAllPorts(null, null);

            FileSelectedPane.Width = new GridLength(3, GridUnitType.Star);
            FiltersPane.Width = new GridLength(0, GridUnitType.Star);

        }

        int _pageIndex;

        private void NextPage(object sender, RoutedEventArgs e)
        {
            RemoveAllPackets();
            _pageIndex++;

            for (int i = 0; i < 8; i++)
            {
                _previous[i] = new TimeSpan();
            }

            if (100 * (_pageIndex + 1) > SortedPackets.Count)
            {
                NextPageBtn.Visibility = Visibility.Hidden;
            }
            else
            {
                NextPageBtn.Visibility = Visibility.Visible;
            }

            Packet[] toLoad;

            if (SortedPackets.Count == 0)
            {
                try
                {
                    toLoad = SortedPackets.ToList().GetRange(100 * _pageIndex, 100).ToArray();
                }
                catch (Exception)
                {
                    toLoad = SortedPackets.ToList().GetRange(100 * _pageIndex, SortedPackets.Count - 100 * _pageIndex).ToArray();
                }
            }
            else
            {
                try
                {
                    toLoad = SortedPackets.GetRange(100 * _pageIndex, 100).ToArray();
                }
                catch (Exception)
                {
                    toLoad = SortedPackets.ToList().GetRange(100 * _pageIndex, SortedPackets.Count - 100 * _pageIndex).ToArray();
                }
            }
            CreateAllTimeLabels(toLoad);
            AddPacketCollection(toLoad);

            PrevPageBtn.Visibility = Visibility.Visible;

        }

        private void PrevPage(object sender, RoutedEventArgs e)
        {
            RemoveAllPackets();
            _pageIndex--;

            for (int i = 0; i < 8; i++)
            {
                _previous[i] = new TimeSpan();
            }

            PrevPageBtn.Visibility = _pageIndex == 0 
                ? Visibility.Hidden 
                : Visibility.Visible;

            Packet[] toLoad;

            if (SortedPackets.Count == 0)
            {
                try
                {
                    toLoad = _controller.packets.Values.ToList().GetRange(100 * _pageIndex, 100).ToArray();
                }
                catch (Exception)
                {
                    toLoad = _controller.packets.Values.ToList().GetRange(100 * _pageIndex, _controller.packets.Count - 100 * _pageIndex).ToArray();
                }
            }
            else
            {
                try
                {
                    toLoad = SortedPackets.GetRange(100 * _pageIndex, 100).ToArray();
                }
                catch (Exception)
                {
                    toLoad = SortedPackets.ToList().GetRange(100 * _pageIndex, _controller.packets.Count - 100 * _pageIndex).ToArray();
                }
            }

            CreateAllTimeLabels(toLoad);
            AddPacketCollection(toLoad);

            NextPageBtn.Visibility = Visibility.Visible;

        }

        void SearchForPacketsByTime(object sender, RoutedEventArgs e)
        {
            RemoveAllPackets();

            _pageIndex = 0;
            PrevPageBtn.Visibility = Visibility.Hidden;

            var start = StartTimeTextBox.Text;
            var end = EndTimeTextBox.Text;
            DateTime startTime = new DateTime();
            DateTime endTime = new DateTime();

            if (start != "")
            {
                try
                {
                    startTime = DateTime.ParseExact(start, "dd-MM-yyyy HH:mm:ss.fff", null);
                }
                catch
                {
                    MessageBox.Show("You have entered an invalid time for the start date.");
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
                    MessageBox.Show("You have entered an invalid time for the end date.");
                }
            }
            if (start != "" && end != "")
            {
                ShowPacketsBetweenTime(startTime, endTime);
            }
            else if (start == "")
            {
                ShowPacketsUntilTime(endTime);
            }
            else if (end == "")
            {
                ShowPacketsFromTime(startTime);
            }

            if (SortedPackets.Count < 100)
            {
                lblNumShowing.Content = "No packets to display";
			}
            NextPageBtn.Visibility = SortedPackets.Count < 100 
                ? Visibility.Hidden 
                : Visibility.Visible;
        }

        //Shows all packets which were received from the start time onwards
        public void ShowPacketsFromTime(DateTime start)
        {
            SortedPackets.Clear();
            RemoveAllPackets();

            SortedPackets = _controller.packets.Values.Where(p => p.DateRecieved > start).ToList();

            SortedPackets = SortedPackets.OrderBy(p => p.DateRecieved).ToList();

            Packet[] firstHundredPackets;

            firstHundredPackets = SortedPackets.Count < 100 
                ? SortedPackets.GetRange(0, SortedPackets.Count).ToArray() 
                : SortedPackets.GetRange(0, 100).ToArray();

            if (firstHundredPackets.Length > 0)
            {
                CreateAllTimeLabels(firstHundredPackets);
                AddPacketCollection(firstHundredPackets);
            }
        }

        private void ShowPacketsUntilTime(DateTime end)
        {
            SortedPackets.Clear();
            RemoveAllPackets();

            SortedPackets = _controller.packets.Values.Where(p => p.DateRecieved <= end).ToList();
            SortedPackets = SortedPackets.OrderBy(p => p.DateRecieved).ToList();

            Packet[] firstHundredPackets = SortedPackets.Count < 100 
                ? SortedPackets.GetRange(0, SortedPackets.Count).ToArray() 
                : SortedPackets.GetRange(0, 100).ToArray();

            if (firstHundredPackets.Length > 0)
            {
                CreateAllTimeLabels(firstHundredPackets);
                AddPacketCollection(firstHundredPackets);
            }
        }

        //Shows packets which were received between the start and end time.
        private void ShowPacketsBetweenTime(DateTime start, DateTime end)
        {
            SortedPackets.Clear();
            RemoveAllPackets();

            SortedPackets = _controller.packets.Values.Where(p => p.DateRecieved >= start && p.DateRecieved <= end).ToList();
            SortedPackets = SortedPackets.OrderBy(p => p.DateRecieved).ToList();

            Packet[] firstHundredPackets = SortedPackets.Count < 100 
                ? SortedPackets.GetRange(0, SortedPackets.Count).ToArray() 
                : SortedPackets.GetRange(0, 100).ToArray();

            if (firstHundredPackets.Length > 0)
            {
                CreateAllTimeLabels(firstHundredPackets);
                AddPacketCollection(firstHundredPackets);
            }
        }

        private void Image_MouseEnter(object sender, MouseEventArgs e)
        {
            HelpPanel.Visibility = Visibility.Visible;
        }

        private void Image_MouseLeave(object sender, MouseEventArgs e)
        {
            HelpPanel.Visibility = Visibility.Hidden;
        }

    }
}