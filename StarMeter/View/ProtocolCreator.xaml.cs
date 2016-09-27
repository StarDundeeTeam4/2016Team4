using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;

namespace StarMeter.View
{
    /// <summary>
    /// Interaction logic for ProtocolCreator.xaml
    /// </summary>
    public partial class ProtocolCreator
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

                    var streamWriter = new StreamWriter("../../Resources/ProtocolList.txt", true);
                    streamWriter.WriteLine(txtProtocolID.Text + " (" + txtProtocolName.Text +")");
                    streamWriter.Close();

                    CreatedObject = new KeyValuePair<int, string>(int.Parse(txtProtocolID.Text), txtProtocolName.Text);
                    MainWindow.Protocols.Add(CreatedObject);

                    Close();
                }
                catch (Exception) { }
            }
        }

        private bool IsDataValid()
        {
            int id;
            // check number/ID - valid number?
            try
            {
                id = int.Parse(txtProtocolID.Text);

                if (id < 0)
                {
                    MessageBox.Show("Invalid integer input"); return false;       
                }

            }
            catch (Exception)
            {
                MessageBox.Show("Invalid integer input"); return false;
            }

            // check if ID already exists
            if (MainWindow.Protocols.Any(p => p.Key.Equals(id)))
            {
                MessageBox.Show("A protocol with ID of " + id + " already exists"); return false;
            }

            if (txtProtocolName.Text.Trim().Length < 1)
            {
                MessageBox.Show("Invalid Name");
                return false;
            }
            return true;
        }

        private void CancelClick(object sender, RoutedEventArgs e)
        {
            CreatedObject = new KeyValuePair<int, string>(-1, "Failed");
            Close();
        }
    }
}
