using Microsoft.Win32;
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
        public MainWindow()
        {
            InitializeComponent();
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


        void TestTimeCreation(object sender, RoutedEventArgs e) 
        {

            for (int i = 0; i < 8; i++) 
            {
                Label lo = new Label();
                lo.Content = "00:00:00.000";
                lo.SetResourceReference(Control.StyleProperty, "Timestamp");
                
                TimeList.Children.Add(lo);


                Random r = new Random(DateTime.Now.Millisecond);

                Thread.Sleep(2);

                int inte = r.Next(0, 10);

                string sty = null;

                


                var b = new Button();
                b.Click += OpenPopup;


                var lab = new Label();
                lab.Content = "NEW PACKET\nNew Data";

                b.Tag = "f622066e-f9f5-4529-93b1-d1d50146cc1d";
                b.Content = lab; 
                
                if (inte > 8)
                {
                    sty = "Error";
                    var err = new Label();
                    err.Content = "PAR";
                }
                else
                {
                    sty = "Success";
                }

                b.SetResourceReference(Control.StyleProperty, sty); 
                
                
                
                var b2 = new Button();
                b2.Click += OpenPopup;

                var lab2 = new Label();
                lab2.Content = "NEW PACKET\nMore Data";

                b2.Tag = "f622066e-f9f5-4529-93b1-d1d50146cc1d";
                b2.Content = lab2;

                if (inte > 5)
                {
                    sty = "Error";
                }
                else
                {
                    sty = "Success";
                }

                b2.SetResourceReference(Control.StyleProperty, sty);

                Port1AHolder.Children.Add(b);
                Port1BHolder.Children.Add(b2);



            }


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
			
            pp.SetupElements(br); // send the packet as a parameter
            pp.ShowDialog();


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
            style.Setters.Add(new Setter(Button.BackgroundProperty, Brushes.Blue));
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
            style.Setters.Add(new Setter(BackgroundProperty, Brushes.SkyBlue));
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
