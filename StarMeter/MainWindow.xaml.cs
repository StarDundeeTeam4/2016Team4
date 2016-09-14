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
            mouseDownPos = e.GetPosition(theGrid);
            theGrid.CaptureMouse();

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
            theGrid.ReleaseMouseCapture();

            // Hide the drag selection box.
            selectionBox.Visibility = Visibility.Collapsed;

            Point mouseUpPos = e.GetPosition(theGrid);
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

                Point mousePos = e.GetPosition(theGrid);

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

        void TestTimeCreation(object sender, RoutedEventArgs e) 
        {
            Label l = new Label();
            l.Content = "NEW LABEL";
            Style s = FindResource("Timestamp") as Style;
            l.Style = s;
            TimeList.Children.Add(l);


            for (int i = 0; i < 8; i++) 
            {
                Label lo = new Label();
                lo.Content = "LOOP LABEL";
                Style so = FindResource("Timestamp") as Style;
                lo.Style = so;
                TimeList.Children.Add(lo);
                Thread.Sleep(100);
            }


        }


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

                string output = "";

                foreach (var s in filename)
                {
                    string[] split = s.Split('\\');
                    string actualName = split[split.Length - 1];

                    output += actualName + ",\n";
                }

                if(output.Length > 1)
                {
                    output = output.Substring(0, output.Length - 2);
                }

                FileSelected.Content = output;

            }


            //List<KeyValuePair<string, int>> MyValue = new List<KeyValuePair<string, int>>();
            //MyValue.Add(new KeyValuePair<string, int>("Administration", 20));
            //MyValue.Add(new KeyValuePair<string, int>("Management", 36));
            //MyValue.Add(new KeyValuePair<string, int>("Development", 89));
            //MyValue.Add(new KeyValuePair<string, int>("Support", 270));
            //MyValue.Add(new KeyValuePair<string, int>("Sales", 140));

            //LineChart1.DataContext = MyValue;


        }


        void OpenPopup(object sender, RoutedEventArgs e) 
        {
            Button b = (Button)sender;

            Brush br = b.Background;

            string text = b.Tag.ToString();
            Guid guid = new Guid(text);
            GetPacketFromGUID(guid);        // needs to return a packet
            
            PacketPopup pp = new PacketPopup();

            var host = new Window();
            host.Content = pp;
            pp.SetupElements(); // send the packet as a parameter
            host.ShowDialog();

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

    }


}
