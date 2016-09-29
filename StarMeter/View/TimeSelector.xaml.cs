using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace StarMeter.View
{
    /// <summary>
    /// Interaction logic for TimeSelector.xaml
    /// </summary>
    public partial class TimeSelector : Window
    {
        public string DateCreated = "";

        public TimeSelector()
        {
            InitializeComponent();
        }

        public void SetupElements(string content, string timeToChange) 
        {
            // split the date already selected, and put into the correct text boxes
            try{

                this.Title = "Select the " + timeToChange + " time";

                string[] splitDate = content.Split('-');
                txtDay.Text = splitDate[0];
                txtMonth.Text = splitDate[1];
                txtYear.Text = splitDate[2].Substring(0, 4);

                string time = content.Split(' ')[1];
                string[] splitTime = time.Split(':');
                txtHour.Text = splitTime[0];
                txtMinute.Text = splitTime[1];
                txtSecond.Text = splitTime[2].Split('.')[0];
                txtMilli.Text = splitTime[2].Split('.')[1];

            }catch(Exception){}
        }

        private string GetDateString()
        {
            string date = txtDay.Text;
            date += "-" + txtMonth.Text;
            date += "-" + txtYear.Text;

            string time = txtHour.Text;
            time += ":" + txtMinute.Text;
            time += ":" + txtSecond.Text;
            time += "." + txtMilli.Text;
            
            return date + " " + time;

        }

        public void Okay(object sender, RoutedEventArgs e) 
        {
            DateCreated = GetDateString();
            this.Close();
        }

        public void Cancel(object sender, RoutedEventArgs e) 
        {
            DateCreated = "";
            this.Close();
        }
    }
}
