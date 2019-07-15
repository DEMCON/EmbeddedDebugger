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
using System.Linq;
using System.Net;
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

namespace EmbeddedDebugger.Connectors.TCP
{
    /// <summary>
    /// Interaction logic for TcpConnectorSettingsWindow.xaml
    /// </summary>
    public partial class TcpConnectorSettingsWindow : Window
    {
        #region Properties
        private string hostName;
        public string HostName { get => hostName; set => hostName = value; }
        private int port;
        public int Port { get => port; set => port = value; }
        public bool AsServer { get => AsServerCheckBox.IsChecked == true; set => AsServerCheckBox.IsChecked = value; }
        #endregion

        public TcpConnectorSettingsWindow()
        {
            InitializeComponent();
        }

        private void PortTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (!int.TryParse(e.Text, out int restul))
            {
                e.Handled = true;
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(HostTextBox.Text))
            {
                System.Windows.Forms.MessageBox.Show("Please enter a valid hostname");
                return;
            }
            HostName = HostTextBox.Text;
            if (!int.TryParse(PortTextBox.Text, out port))
            {
                System.Windows.Forms.MessageBox.Show("Please enter a valid port");
                return;
            }
            DialogResult = true;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Set the textboxes to defined or default values
            HostTextBox.Text = hostName == null ? "127.0.0.1" : hostName.ToString();
            PortTextBox.Text = "" + (port <= 0 ? 59283 : port);
        }

        private void LocalhostCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            HostTextBox.IsEnabled = false;
            HostTextBox.Text = "127.0.0.1";
        }

        private void LocalhostCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            HostTextBox.IsEnabled = true;
            HostTextBox.Text = GetIPAddress();
        }

        private string GetIPAddress()
        {
            foreach (IPAddress IP in Dns.GetHostEntry(Environment.MachineName).AddressList)
            {
                if (IP.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    return Convert.ToString(IP);
                }
            }
            return "";
        }
    }
}
