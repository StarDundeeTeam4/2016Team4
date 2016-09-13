using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
                height = 10;
                image = new ImageBrush(new BitmapImage(new Uri(@"pack://application:,,,/Resources/down-arrow.png")));
            }
            else
            {
                height = 1;
                image = new ImageBrush(new BitmapImage(new Uri(@"pack://application:,,,/Resources/up-arrow.png")));
            }

            DataVisualisationPopup.Height = new GridLength(height, GridUnitType.Star);
            DataVisButton.Background = image;
            is_up_arrow = !is_up_arrow;
        }
        
        
        // This is the method to run when the timer is raised.
        private void TimerEventProcessor(Object myObject,EventArgs myEventArgs)
        {
            t.Stop();


            // Restarts the timer and increments the counter.
            count += 1;
            t.Enabled = true;

            Console.WriteLine(count);

            DataVisualisationPopup.Height = new GridLength(count, GridUnitType.Star); ;

            //if (count > 40)
            //{
            //    t.Stop();
            //    Console.WriteLine("STOP");
            //}

        }

        Timer t;
        int count = 0;

    }
}
