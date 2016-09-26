using System;
using System.Collections.Generic;
using System.IO;
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
    /// Interaction logic for ProtocolCreator.xaml
    /// </summary>
    public partial class ProtocolCreator : Window
    {
        public static KeyValuePair<int, string> CreatedObject;

        public ProtocolCreator()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var valid = IsDataValid();

            if (valid)
            {
                // add 
                try
                {
                    //TODO: change to comma thing

                    StreamWriter sr = new StreamWriter("../../Resources/ProtocolList.txt", true);

                    sr.WriteLine(txtProtocolID.Text + " (" + txtProtocolName.Text +")");

                    sr.Close();

                    CreatedObject = new KeyValuePair<int, string>(int.Parse(txtProtocolID.Text), txtProtocolName.Text);
                    MainWindow.Protocols.Add(CreatedObject);

                    this.Close();
                                                        
                }
                catch (Exception) { }
            }
        }

        bool IsDataValid()
        {
            int id = -1;
            // check number/ID - valid number?
            try
            {
                id = int.Parse(txtProtocolID.Text);
            }
            catch (Exception) { MessageBox.Show("Invalid integer input"); return false; }

            // check if ID already exists
            foreach (var p in MainWindow.Protocols)
            {
                if (p.Key.Equals(id)) { MessageBox.Show("A protocol with ID of " + id + " already exists"); return false; };
            }

            if (txtProtocolName.Text.Trim().Length < 1)
            {
                MessageBox.Show("Invalid Name");
                return false;
            }

            return true;

        }

    }
}
