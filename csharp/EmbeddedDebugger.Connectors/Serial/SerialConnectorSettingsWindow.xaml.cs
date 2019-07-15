/*
Embedded Debugger PC Application which can be used to debug embedded systems at a high level.
Copyright (C) 2019 DEMCON advanced mechatronics B.V.

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/
using System;
using System.Collections.Generic;
using System.IO.Ports;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace EmbeddedDebugger.Connectors.Serial
{
    /// <summary>
    /// Interaction logic for SerialConnectorSettingsWindow.xaml
    /// </summary>
    public partial class SerialConnectorSettingsWindow : Window
    {
        // Some default baud rates to use
        private int[] defaultBaudRates = { 110, 300, 600, 1200, 2400, 4800, 9600, 14400, 19200, 38400, 57600, 115200, 128000, 256000 };

        #region Properties
        private string portName;
        public string PortName { get { return portName; } set { portName = value; } }
        private int baudRate;
        public int BaudRate { get { return baudRate; } set { baudRate = value; } }
        private Parity parity;
        public Parity Parity { get { return parity; } set { parity = value; } }
        private int dataBits;
        public int DataBits { get { return dataBits; } set { dataBits = value; } }
        private StopBits stopBits;
        public StopBits StopBits { get { return stopBits; } set { stopBits = value; } }
        private Handshake handshake;
        public Handshake Handshake { get { return handshake; } set { handshake = value; } }
        private int readTimeout;
        public int ReadTimeout { get { return readTimeout; } set { readTimeout = value; } }
        private int writeTimeout;
        public int WriteTimout { get { return writeTimeout; } set { writeTimeout = value; } }
        #endregion

        public SerialConnectorSettingsWindow()
        {
            InitializeComponent();
        }

        private void PortButton_Click(object sender, RoutedEventArgs e)
        {
            string selectedPort = PortComboBox.Items.Count > 0 ? (string)PortComboBox.SelectedItem : "";
            PortComboBox.Items.Clear();
            foreach (string s in SerialPort.GetPortNames())
            {
                PortComboBox.Items.Add(s);
            }
            // Select the previously set port, if that was none or it doesn't exist anymore, go to default
            PortComboBox.SelectedIndex = !string.IsNullOrEmpty(selectedPort) && PortComboBox.Items.Contains(selectedPort) ? PortComboBox.Items.IndexOf(selectedPort) : (PortComboBox.Items.Count > 0 ? 0 : -1);
        }

        /// <summary>
        /// Cancel the configuration, leaving all changes to be discarded
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">Arguments</param>
        private void CancelButton_Click(object sender, EventArgs e)
        {
            // Close the form
            DialogResult = false;
        }

        /// <summary>
        /// Apply the given configuration
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">Arguments</param>
        private void OKButton_Click(object sender, EventArgs e)
        {
            // Make sure all of the given values are actually values we can use
            try
            {
                baudRate = int.Parse(BaudComboBox.Text);
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show("Please enter a valid baud rate");
#if DEBUG
                Console.WriteLine(ex);
#endif
                return;
            }
            try
            {
                readTimeout = int.Parse(ReadTimeoutTextBox.Text);
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show("Please enter a valid read timeout");
#if DEBUG
                Console.WriteLine(ex);
#endif
                return;
            }
            try
            {
                writeTimeout = int.Parse(WriteTimeoutTextBox.Text);
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show("Please enter a valid write timeout");
#if DEBUG
                Console.WriteLine(ex);
#endif
                return;
            }
            // Some of the values have been set programatically and cannot be nullified by the user
            portName = (string)PortComboBox.SelectedItem;
            baudRate = int.Parse(BaudComboBox.Text);
            parity = (Parity)ParityComboBox.SelectedItem;
            dataBits = (int)DataBitsComboBox.SelectedItem;
            stopBits = (StopBits)StopBitsComboBox.SelectedItem;
            handshake = (Handshake)HandshakeComboBox.SelectedItem;
            readTimeout = int.Parse(ReadTimeoutTextBox.Text);
            writeTimeout = int.Parse(WriteTimeoutTextBox.Text);
            // Close the form
            DialogResult = true;
        }

        private void BaudComboBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (!int.TryParse(e.Text, out int result))
            {
                e.Handled = true;
            }
        }

        private void ReadTimeoutTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (!int.TryParse(e.Text, out int result))
            {
                e.Handled = true;
            }
        }

        private void WriteTimeoutTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (!int.TryParse(e.Text, out int result))
            {
                e.Handled = true;
            }
        }

        /// <summary>
        /// Upon loading of the window
        /// </summary>
        /// <param name="sender">Sender of the event</param>
        /// <param name="e">Arguments</param>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Make sure all data is up to date
            PortComboBox.Items.Clear();
            foreach (string s in SerialPort.GetPortNames())
            {
                PortComboBox.Items.Add(s);
            }
            BaudComboBox.ItemsSource = defaultBaudRates;
            ParityComboBox.ItemsSource = Enum.GetValues(typeof(Parity));
            DataBitsComboBox.ItemsSource = new int[] { 5, 6, 7, 8 };
            StopBitsComboBox.ItemsSource = Enum.GetValues(typeof(StopBits)).Cast<StopBits>().Where(w => w != StopBits.None).ToList();
            HandshakeComboBox.ItemsSource = Enum.GetValues(typeof(Handshake));

            // Set the values of the boxes
            PortComboBox.SelectedIndex = string.IsNullOrEmpty(portName) ? (PortComboBox.Items.Count <= 0 ? -1 : 0) : PortComboBox.Items.IndexOf(portName);
            BaudComboBox.SelectedItem = baudRate <= 0 ? 9600 : baudRate;
            ParityComboBox.SelectedItem = parity <= 0 ? Parity.None : parity;
            DataBitsComboBox.SelectedItem = dataBits <= 0 ? 8 : dataBits;
            StopBitsComboBox.SelectedItem = stopBits <= 0 ? StopBits.One : stopBits;
            HandshakeComboBox.SelectedItem = handshake <= 0 ? Handshake.None : handshake;
            ReadTimeoutTextBox.Text = "" + (readTimeout <= 0 ? 500 : readTimeout);
            WriteTimeoutTextBox.Text = "" + (writeTimeout <= 0 ? 500 : writeTimeout);
        }
    }
}
