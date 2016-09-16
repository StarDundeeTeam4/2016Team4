using Microsoft.Win32;
using StarMeter.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.DataVisualization.Charting;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Win32;

namespace StarMeter.View
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {

        private StackPanel[] _portStacks = new StackPanel[8];

        public readonly Packet _packet1 = new Packet
        {
            
            PortNumber = 3,
            IsError = false,
            PacketId = Guid.NewGuid(),
            DateRecieved = DateTime.ParseExact("08-09-2016 15:12:50.081", "dd-MM-yyyy HH:mm:ss.fff", null),
            Cargo =
                @"00 fe fa 00 17 50 b8 f6 ca d3 9e 3c 52 74 51 9f ef 80 ba f6 75 92 de c3 aa 62 5f aa f0 de 46 28 24 7c ff 81 c5 ce a5 fa 59 57 81 49 0c 9d cd 4a 9b 7f bd f3 70 c9 c0 8a 0f 06 03 15 b0 95 36 13 2d ff 94 69 1f 88 1d 9f 44 04 26 4c 25 ec 14 cf f5 b1 65 40 bb 50 f0 a7 b4 27 6d 6b f2 07 37 0d 4a 8a 51 15 6d a7 a7 4d 55 83 97 2e e3 8a b0 98 c6 bf ba c6 9e 50 f6 80 61 6e a7 92 fe 5b d0 7e 41 c5 40 6e f7 52 cc 6c 52 7c dc d5 8f 9f 29 0b d5 50 c4 6b 61 f1 5b 7f e0 82 b8 74 1c ba 8a ce db 57 68 5a 04 b2 13 64 04 96 fb 2b 70 52 05 92 ec 0d 8c 18 4b 5a a6 0a f8 0d a8 f8 94 4c ec 65 e0 e9 d1 c2 de ef 04 9e 33 7a fe 17 d0 cc ce 94 d1 9e 19 b6 a5 b4 5f 8b 70 b4 7f 05 ad 38 7e ab 18 22 84 8f cb 30 27 80 a7 d0 ec 80 f5 35 0b 79 4d aa 73 2b b7 26 0e 69 11 21 46 85 b1 a7 c8"
                    .Split(' ')
        };

        public readonly Packet _packet2 = new Packet
        {
            IsError = true,
            PacketId = Guid.NewGuid(),
            DateRecieved = DateTime.ParseExact("08-09-2016 15:12:52.081", "dd-MM-yyyy HH:mm:ss.fff", null),
            Cargo =
                @"00 fe fa 00 17 50 b8 f6 ca d3 9e 3c 52 74 51 9f ef 80 ba f6 75 92 de c3 aa 62 5f aa f0 de 46 28 24 7c ff 81 c5 ce a5 fa 59 57 81 49 0c 9d cd 4a 9b 7f bd f3 70 c9 c0 8a 0f 06 03 15 b0 95 36 13 2d ff 94 69 1f 88 1d 9f 44 04 26 4c 25 ec 14 cf f5 b1 65 40 bb 50 f0 a7 b4 27 6d 6b f2 07 37 0d 4a 8a 51 15 6d a7 a7 4d 55 83 97 2e e3 8a b0 98 c6 bf ba c6 9e 50 f6 80 61 6e a7 92 fe 5b d0 7e 41 c5 40 6e f7 52 cc 6c 52 7c dc d5 8f 9f 29 0b d5 50 c4 6b 61 f1 5b 7f e0 82 b8 74 1c ba 8a ce db 57 68 5a 04 b2 13 64 04 96 fb 2b 70 52 05 92 ec 0d 8c 18 4b 5a a6 0a f8 0d a8 f8 94 4c ec 65 e0 e9 d1 c2 de ef 04 9e 33 7a fe 17 d0 cc ce 94 d1 9e 19 b6 a5 b4 5f 8b 70 b4 7f 05 ad 38 7e ab 18 22 84 8f cb 30 27 80 a7 d0 ec 80 f5 35 0b 79 4d aa 73 2b b7 26 0e 69 11 21 46 85 b1 a7 c8"
                    .Split(' ')
        };

        public readonly Packet _packet3 = new Packet
        {
            PortNumber = 4,
            IsError = false,
            PacketId = Guid.NewGuid(),
            DateRecieved = DateTime.ParseExact("08-09-2016 15:12:54.081", "dd-MM-yyyy HH:mm:ss.fff", null),
            Cargo =
                @"00 fe fa 00 17 50 b8 f6 ca d3 9e 3c 52 74 51 9f ef 80 ba f6 75 92 de c3 aa 62 5f aa f0 de 46 28 24 7c ff 81 c5 ce a5 fa 59 57 81 49 0c 9d cd 4a 9b 7f bd f3 70 c9 c0 8a 0f 06 03 15 b0 95 36 13 2d ff 94 69 1f 88 1d 9f 44 04 26 4c 25 ec 14 cf f5 b1 65 40 bb 50 f0 a7 b4 27 6d 6b f2 07 37 0d 4a 8a 51 15 6d a7 a7 4d 55 83 97 2e e3 8a b0 98 c6 bf ba c6 9e 50 f6 80 61 6e a7 92 fe 5b d0 7e 41 c5 40 6e f7 52 cc 6c 52 7c dc d5 8f 9f 29 0b d5 50 c4 6b 61 f1 5b 7f e0 82 b8 74 1c ba 8a ce db 57 68 5a 04 b2 13 64 04 96 fb 2b 70 52 05 92 ec 0d 8c 18 4b 5a a6 0a f8 0d a8 f8 94 4c ec 65 e0 e9 d1 c2 de ef 04 9e 33 7a fe 17 d0 cc ce 94 d1 9e 19 b6 a5 b4 5f 8b 70 b4 7f 05 ad 38 7e ab 18 22 84 8f cb 30 27 80 a7 d0 ec 80 f5 35 0b 79 4d aa 73 2b b7 26 0e 69 11 21 46 85 b1 a7 c8"
                    .Split(' ')
        };




        public MainWindow()
        {
            InitializeComponent();

            _portStacks[0] = Port1AHolder;
            _portStacks[1] = Port1BHolder;
            _portStacks[2] = Port2AHolder;
            _portStacks[3] = Port2BHolder;
            _portStacks[4] = Port3AHolder;
            _portStacks[5] = Port3BHolder;
            _portStacks[6] = Port4AHolder;
            _portStacks[7] = Port4BHolder;

        }

        //public void ActionName(object sender, RoutedEventArgs e) 
        //{
        //    lblSpace.Content = "I like space";
        //}

        private bool _mouseDown; // Set to 'true' when mouse is held down.
        private Point _mouseDownPos; // The point where the mouse button was clicked down.

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

            SizeLabelTest.Content = s.ToString();

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


        Button GetPacketButton(Packet p) 
        {
            Label l = new Label();
            l.Content = "00:00:00.000";
            l.SetResourceReference(Control.StyleProperty, "Timestamp");

            TimeList.Children.Add(l);
            
            Random r = new Random(DateTime.Now.Millisecond);
            
            string sty = "";
            
            var b = new Button();
            b.Click += OpenPopup;

            var lab = new Label();

            try
            {
                lab.Content = p.Cargo[18];
            }
            catch (Exception e) 
            {
                lab.Content = "Unknown Packet Type";
            }

            try
            {
                b.Tag = p.PacketId;
            }
            catch (Exception e) 
            {
                b.Tag = "";
            }
           
            b.Content = lab;

            try
            {
                if (p.IsError)
                {
                    sty = "Error";
                }
                else
                {
                    sty = "Success";
                }
            }
            catch (Exception) 
            {
                sty = "Error";
            }

            b.SetResourceReference(Control.StyleProperty, sty);
            return b;

        }

        StackPanel GetPanelToUse(int portNum) 
        {
            return _portStacks[portNum];
        }


        void AddPacketCollection(Packet[] packets) 
        {
            foreach (var p in packets) 
            {
                AddPacket(p);
            }
        }

        void AddPacket(Packet p) 
        {
            Button b = GetPacketButton(p);

            StackPanel sp = GetPanelToUse(p.PortNumber);            

            sp.Children.Add(b);
        }



        // TEMP
        Packet[] packets = new Packet[3];

        void TestTimeCreation(object sender, RoutedEventArgs e) 
        {

            packets[0] = _packet1;
            packets[1] = _packet2;
            packets[2] = _packet3;

            AddPacketCollection(packets);


            
            RatesLineChart.Series.Clear();

            var valueList = new List<KeyValuePair<string, int>>
            {
                new KeyValuePair<string, int>("00:00:00:000", 4),
                new KeyValuePair<string, int>("00:00:01:000", 1),
                new KeyValuePair<string, int>("00:00:04:102", 42),
                new KeyValuePair<string, int>("00:00:14:000", 21),
                new KeyValuePair<string, int>("00:00:41:000", 41),
                new KeyValuePair<string, int>("00:01:12:050", 24),
                new KeyValuePair<string, int>("00:01:17:000", 17),
                new KeyValuePair<string, int>("00:01:60:100", 19)
            };


            var lineSeries1 = new LineSeries
            {
                Title = "Data Rate",
                Foreground = Brushes.White,
                DependentValuePath = "Value",
                IndependentValuePath = "Key",
                ItemsSource = valueList
            };
            RatesLineChart.Series.Add(lineSeries1);



            var valueList2 = new List<KeyValuePair<string, int>>
            {
                new KeyValuePair<string, int>("00:00:00:000", 8),
                new KeyValuePair<string, int>("00:00:01:000", 2),
                new KeyValuePair<string, int>("00:00:04:102", 42),
                new KeyValuePair<string, int>("00:00:14:000", 23),
                new KeyValuePair<string, int>("00:00:41:000", 19),
                new KeyValuePair<string, int>("00:01:12:050", 25),
                new KeyValuePair<string, int>("00:01:17:000", 18),
                new KeyValuePair<string, int>("00:01:60:100", 19)
            };


            LineSeries lineSeries2 = new LineSeries
            {
                Foreground = Brushes.White,
                Title = "Error Rate",
                DependentValuePath = "Value",
                IndependentValuePath = "Key",
                ItemsSource = valueList2
            };
            RatesLineChart.Series.Add(lineSeries2);

            //ScrollBackToBottom();

        }
        ////////////////////////////




        //This will allow us to read the files or remove the files later.
        readonly List<String> _selectedFiles = new List<String>();
        readonly List<Grid> _fileGrids = new List<Grid>();

        private void FileSelection(object sender, RoutedEventArgs e) 
        {
            var ofd = new OpenFileDialog
            {
                Filter = "Record Files (.rec)|*.rec",
                Multiselect = true
            };


            var confirmed = ofd.ShowDialog();

            if (confirmed != true) return;
            // display file name
            string[] filename = ofd.FileNames;
                
            foreach (var s in filename)
            {
                //If the user has not already selected this file.
                if(_selectedFiles.Contains(s) == false)
                {
                    _selectedFiles.Add(s);
                    string[] split = s.Split('\\');
                    string actualName = split[split.Length - 1];
                    var fileNameWithoutExtension = actualName.Substring(0, actualName.Length - 4);

                    var g = new Grid
                    {
                        Name = "grid" + fileNameWithoutExtension,
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

                    //if actualName is "file.rec" then fileNameWithoutExtension would become "file"

                    var l = new Label
                    {
                        Name = "label" + fileNameWithoutExtension,
                        Style = (Style) Application.Current.Resources["FileSelected"],
                        Content = actualName
                    };

                    var b = new Button {Name = fileNameWithoutExtension};

                    var myCount = _selectedFiles.Count - 1;

                    b.Tag = myCount.ToString();
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
                else
                {
                    MessageBox.Show("You have already added this file.");
                }

            }
        }

        void CancelUpload(object sender, RoutedEventArgs e)
        {

            var b = (Button)sender;
            var tag = b.Tag.ToString();
            var id = int.Parse(tag);

            SelectedFiles.Children.RemoveAt(id);
            _selectedFiles.RemoveAt(id);
            _fileGrids.RemoveAt(id);

            for (var i = id; i < _fileGrids.Count; i++) 
            {
                // http://stackoverflow.com/questions/14825232/what-is-a-smarter-way-to-get-a-child-control-in-xaml
                var btn = _fileGrids[i].Children.OfType<Button>().Single(f => f.Name == "RemoveButton");
                btn.Tag = i;
            }

            
        }

        void RemoveFile(Panel grid)
        {
            foreach(UIElement child in grid.Children)
            {
                grid.Children.Remove(child);
            }

            SelectedFiles.Children.Remove(grid);
        }

        public void OpenPopup(object sender, RoutedEventArgs e) 
        {
            var b = (Button)sender;

            var br = b.Background;

            var text = b.Tag.ToString();
            var guid = new Guid(text);
            GetPacketFromGuid(guid);        // needs to return a packet
            
            PacketPopup pp = new PacketPopup();


            Packet p = FindPacket(guid);

            if (p != null)
            {

                pp.SetupElements(br, p); // send the packet as a parameter
                pp.ShowDialog();
            }
            
        }

        Packet FindPacket(Guid guid) 
        {
            foreach (var p in packets) 
            {
                if (guid.Equals(p.PacketId)) 
                {
                    return p;
                }
            }

            return null;
        }

        // function to get the Packet from the GUID provided
        static void GetPacketFromGuid(Guid guid) 
        {
        }


        //This lets us know which image to change to.
        private bool _isUpArrow = true;

        private void ShowDataVisPopup(object sender, RoutedEventArgs e)
        {

            Console.WriteLine("CLEEK");

            ImageBrush image;

            if (_isUpArrow)
            {
                DataVisButton.VerticalAlignment = VerticalAlignment.Top;
                image = new ImageBrush(new BitmapImage(new Uri(@"pack://application:,,,/Resources/down-arrow.png")));
            }
            else
            {
                //height = 1;
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

        private void MoveSlider()
        {
            DataVisualisationPopup.Height = new GridLength(_count, GridUnitType.Star); ;
        }

        private void FixStretch()
        {
            if (!_isUpArrow)
            {
                DataVisButton.VerticalAlignment = VerticalAlignment.Stretch;
            }

        }

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


        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Application.Current.Resources["Success"] = GetSuccessStyle(HeightScroller.Value);
            Application.Current.Resources["Error"] = GetErrorStyle(HeightScroller.Value);
            Application.Current.Resources["Timestamp"] = GetTimeStyle(HeightScroller.Value);            
        }

    }


}
