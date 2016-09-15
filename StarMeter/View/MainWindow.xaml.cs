using Microsoft.Win32;
using StarMeter.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.DataVisualization.Charting;
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
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private StackPanel[] _portStacks = new StackPanel[8];

        private readonly Packet _packet1 = new Packet
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

        bool mouseDown = false; // Set to 'true' when mouse is held down.
        Point mouseDownPos; // The point where the mouse button was clicked down.

        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            // Capture and track the mouse.
            mouseDown = true;
            mouseDownPos = e.GetPosition(MainGrid);
            MainGrid.CaptureMouse();

            // Initial placement of the drag selection box.         
            Canvas.SetLeft(selectionBox, mouseDownPos.X);
            Canvas.SetTop(selectionBox, mouseDownPos.Y);
            selectionBox.Width = 0;
            selectionBox.Height = 0;

            // Make the drag selection box visible.
            selectionBox.Visibility = Visibility.Visible;
        }

        private void Grid_MouseUp(object sender, MouseButtonEventArgs e)
        {
            // Release the mouse capture and stop tracking it.
            mouseDown = false;
            MainGrid.ReleaseMouseCapture();

            // Hide the drag selection box.
            selectionBox.Visibility = Visibility.Collapsed;

            Point mouseUpPos = e.GetPosition(MainGrid);
            Size s = new Size(selectionBox.Width, selectionBox.Height);

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
            if (mouseDown)
            {
                // When the mouse is held down, reposition the drag selection box.

                Point mousePos = e.GetPosition(MainGrid);

                if (mouseDownPos.X < mousePos.X)
                {
                    Canvas.SetLeft(selectionBox, mouseDownPos.X);
                    selectionBox.Width = mousePos.X - mouseDownPos.X;
                }
                else
                {
                    Canvas.SetLeft(selectionBox, mousePos.X);
                    selectionBox.Width = mouseDownPos.X - mousePos.X;
                }

                if (mouseDownPos.Y < mousePos.Y)
                {
                    Canvas.SetTop(selectionBox, mouseDownPos.Y);
                    selectionBox.Height = mousePos.Y - mouseDownPos.Y;
                }
                else
                {
                    Canvas.SetTop(selectionBox, mousePos.Y);
                    selectionBox.Height = mouseDownPos.Y - mousePos.Y;
                }
            }
        }


        void ScrollBackToBottom() 
        {
            PacketScroller.ScrollToBottom();
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


        Packet[] packets = new Packet[3];

        void TestTimeCreation(object sender, RoutedEventArgs e) 
        {

            packets[0] = _packet1;
            packets[1] = _packet2;
            packets[2] = _packet3;

            AddPacketCollection(packets);


            
            RatesLineChart.Series.Clear();

            List<KeyValuePair<string, int>> valueList = new List<KeyValuePair<string, int>>();

            valueList.Add(new KeyValuePair<string, int>("00:00:00:000", 4));
            valueList.Add(new KeyValuePair<string, int>("00:00:01:000", 1));
            valueList.Add(new KeyValuePair<string, int>("00:00:04:102", 42));
            valueList.Add(new KeyValuePair<string, int>("00:00:14:000", 21));
            valueList.Add(new KeyValuePair<string, int>("00:00:41:000", 41));
            valueList.Add(new KeyValuePair<string, int>("00:01:12:050", 24));
            valueList.Add(new KeyValuePair<string, int>("00:01:17:000", 17));
            valueList.Add(new KeyValuePair<string, int>("00:01:60:100", 19));

            LineSeries lineSeries1 = new LineSeries();
            lineSeries1.Title = "Data Rate";
            lineSeries1.Foreground = Brushes.White;
            lineSeries1.DependentValuePath = "Value";
            lineSeries1.IndependentValuePath = "Key";
            lineSeries1.ItemsSource = valueList;
            RatesLineChart.Series.Add(lineSeries1);



            List<KeyValuePair<string, int>> valueList2 = new List<KeyValuePair<string, int>>();

            valueList2.Add(new KeyValuePair<string, int>("00:00:00:000", 8));
            valueList2.Add(new KeyValuePair<string, int>("00:00:01:000", 2));
            valueList2.Add(new KeyValuePair<string, int>("00:00:04:102", 42));
            valueList2.Add(new KeyValuePair<string, int>("00:00:14:000", 23));
            valueList2.Add(new KeyValuePair<string, int>("00:00:41:000", 19));
            valueList2.Add(new KeyValuePair<string, int>("00:01:12:050", 25));
            valueList2.Add(new KeyValuePair<string, int>("00:01:17:000", 18));
            valueList2.Add(new KeyValuePair<string, int>("00:01:60:100", 19));

            LineSeries lineSeries2 = new LineSeries();
            lineSeries2.Foreground = Brushes.White;
            lineSeries2.Title = "Error Rate";
            lineSeries2.DependentValuePath = "Value";
            lineSeries2.IndependentValuePath = "Key";
            lineSeries2.ItemsSource = valueList2;
            RatesLineChart.Series.Add(lineSeries2);

            //ScrollBackToBottom();

        }


        //This will allow us to read the files or remove the files later.
        List<String> selected_files = new List<String>();
        List<Grid> fileGrids = new List<Grid>();

        void FileSelection(object sender, RoutedEventArgs e) 
        {
            OpenFileDialog ofd = new OpenFileDialog();

            ofd.Filter = "Record Files (.rec)|*.rec";
            ofd.Multiselect = true;

            bool? confirmed = ofd.ShowDialog();

            if (confirmed == true) 
            {

                // display file name
                string[] filename = ofd.FileNames;
                
                foreach (var s in filename)
                {
                    //If the user has not already selected this file.
                    if(selected_files.Contains(s) == false)
                    {
                        selected_files.Add(s);
                        string[] split = s.Split('\\');
                        string actualName = split[split.Length - 1];
                        var fileNameWithoutExtension = actualName.Substring(0, actualName.Length - 4);

                        Grid g = new Grid();
                        g.Name = "grid" + fileNameWithoutExtension;
                        g.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
                        g.VerticalAlignment = System.Windows.VerticalAlignment.Stretch;
                        g.Height = 30;
                        g.Margin = new Thickness(0, 0, 0, 5);
                        g.Background = Brushes.White;
                        ColumnDefinition cd = new ColumnDefinition();
                        ColumnDefinition cd2 = new ColumnDefinition();
                        cd.Width = new GridLength(8, GridUnitType.Star);
                        cd2.Width = new GridLength(1, GridUnitType.Star);

                        g.ColumnDefinitions.Add(cd);
                        g.ColumnDefinitions.Add(cd2);

                        //if actualName is "file.rec" then fileNameWithoutExtension would become "file"
                        
                        Label l = new Label();
                        l.Name = "label" + fileNameWithoutExtension;
                        l.Style = (Style)Application.Current.Resources["FileSelected"];
                        l.Content = actualName;

                        Button b = new Button();
                        b.Name = fileNameWithoutExtension;

                        int count = selected_files.Count - 1;

                        b.Tag = count.ToString();
                        b.Content = "X";
                        b.Click += cancelUpload;
                        b.Background = Brushes.Red;
                        b.Foreground = Brushes.White;
                        b.HorizontalContentAlignment = System.Windows.HorizontalAlignment.Center;
                        b.VerticalContentAlignment = System.Windows.VerticalAlignment.Center;
                        b.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
                        b.VerticalAlignment = System.Windows.VerticalAlignment.Stretch;
                        b.Name = "RemoveButton";


                        Grid.SetColumn(l, 0);
                        Grid.SetColumn(b, 1);

                        g.Children.Add(l);
                        g.Children.Add(b);
                        SelectedFiles.Children.Add(g);
                        fileGrids.Add(g);



                    }
                    else
                    {
                        System.Windows.MessageBox.Show("You have already added this file.");
                    }

                }
                
            }

        }

        void cancelUpload(object sender, RoutedEventArgs e)
        {

            Button b = (Button)sender;
            string tag = b.Tag.ToString();
            int id = int.Parse(tag);

            SelectedFiles.Children.RemoveAt(id);
            selected_files.RemoveAt(id);
            fileGrids.RemoveAt(id);

            for (int i = id; i < fileGrids.Count; i++) 
            {
                var dfdsf = fileGrids[i].FindName("RemoveButton");
                // http://stackoverflow.com/questions/14825232/what-is-a-smarter-way-to-get-a-child-control-in-xaml
                var btn = (Button)fileGrids[i].Children.OfType<Button>().Single(f => f.Name == "RemoveButton");
                btn.Tag = i;
            }

            
        }

        void removeFile(Grid grid)
        {
            foreach(UIElement child in grid.Children)
            {
                grid.Children.Remove(child);
            }

            SelectedFiles.Children.Remove(grid);
        }

        void OpenPopup(object sender, RoutedEventArgs e) 
        {
            Button b = (Button)sender;

            Brush br = b.Background;

            string text = b.Tag.ToString();
            Guid guid = new Guid(text);
            GetPacketFromGUID(guid);        // needs to return a packet
            
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
        void GetPacketFromGUID(Guid guid) 
        {
            return;
        }


        //This lets us know which image to change to.
        private bool is_up_arrow = true;

        private void ShowDataVisPopup(object sender, RoutedEventArgs e)
        {

            Console.WriteLine("CLEEK");
            //t = new Timer();
            //t.Elapsed += new ElapsedEventHandler(TimerEventProcessor);
            //t.Interval = 10;
            //t.Start();
            int height = 0;
            ImageBrush image = null;

            if (is_up_arrow == true)
            {
                //height = 10;
                DataVisButton.VerticalAlignment = VerticalAlignment.Top;
                image = new ImageBrush(new BitmapImage(new Uri(@"pack://application:,,,/Resources/down-arrow.png")));
            }
            else
            {
                //height = 1;
                image = new ImageBrush(new BitmapImage(new Uri(@"pack://application:,,,/Resources/up-arrow.png")));
                image.Stretch = Stretch.UniformToFill;
            }

            if (t == null)
            {
                t = new System.Timers.Timer();
                t.Elapsed += new ElapsedEventHandler(TimerEventProcessor);
                t.Interval = 10;
                t.Start();
            }

            DataVisButton.Background = image;
        }


        public delegate void UpdateSlider();


        // This is the method to run when the timer is raised.
        private void TimerEventProcessor(Object myObject,EventArgs myEventArgs)
        {

            // Restarts the timer and increments the counter.
            if (is_up_arrow)
            {
                count += 1;
            }
            else 
            {
                count -= 1;
            }

            if ((count > 10 && is_up_arrow) || (count < 2 && !is_up_arrow))
            {
                t.Stop();

                DataVisualisationPopup.Dispatcher.Invoke(new UpdateSlider(FixStretch));
                
                Console.WriteLine("STOP");
                is_up_arrow = !is_up_arrow;
                t = null;
            }


            Console.WriteLine(count);
            DataVisualisationPopup.Dispatcher.Invoke(new UpdateSlider(MoveSlider));



        }

        System.Timers.Timer t = null;
        int count = 0;

        private void MoveSlider()
        {
            DataVisualisationPopup.Height = new GridLength(count, GridUnitType.Star); ;
        }

        private void FixStretch()
        {
            if (!is_up_arrow)
            {
                DataVisButton.VerticalAlignment = VerticalAlignment.Stretch;
            }

        }

        Style GetErrorStyle(double val)
        {

            Style style = new Style { TargetType = typeof(Button) };
            style.Setters.Add(new Setter(MarginProperty, new Thickness(0, 0, 0, (val / 10) - 1)));
            style.Setters.Add(new Setter(HorizontalAlignmentProperty, HorizontalAlignment.Stretch));
            style.Setters.Add(new Setter(HorizontalContentAlignmentProperty, HorizontalAlignment.Center));
            style.Setters.Add(new Setter(VerticalAlignmentProperty, VerticalAlignment.Center));
            style.Setters.Add(new Setter(ForegroundProperty, Brushes.White));
            style.Setters.Add(new Setter(Button.BackgroundProperty, Brushes.Red));
            style.Setters.Add(new Setter(Button.HeightProperty, val));

            return style;
        }
        Style GetSuccessStyle(double val) 
        {

            Style style = new Style { TargetType = typeof(Button) };
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
        Style GetTimeStyle(double val)
        {

            Style style = new Style { TargetType = typeof(Label) };
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
