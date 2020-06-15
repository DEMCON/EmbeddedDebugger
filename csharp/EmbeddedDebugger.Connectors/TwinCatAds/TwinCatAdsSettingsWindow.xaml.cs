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

namespace EmbeddedDebugger.Connectors.TwinCatAds
{
    /// <summary>
    /// Interaction logic for TwinCatAdsSettingsWindow.xaml
    /// </summary>
    public partial class TwinCatAdsSettingsWindow
    {
        #region Properties
        public string HostName { get; set; }
        public int Port { get; set; }

        #endregion

        public TwinCatAdsSettingsWindow()
        {
            this.InitializeComponent();
        }

        private void PortTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (!int.TryParse(e.Text, out int _))
            {
                e.Handled = true;
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(this.HostTextBox.Text))
            {
                System.Windows.Forms.MessageBox.Show("Please enter a valid hostname");
                return;
            }

            this.HostName = this.HostTextBox.Text;
            if (!int.TryParse(this.PortTextBox.Text, out int port))
            {
                System.Windows.Forms.MessageBox.Show("Please enter a valid port");
                return;
            }
            this.Port = port;
            this.DialogResult = true;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Set the textboxes to defined or default values
            this.HostTextBox.Text = this.HostName == null ? "localhost" : this.HostName.ToString();
            this.PortTextBox.Text = "" + (this.Port <= 0 ? 851 : this.Port);
        }

        private void LocalhostCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            this.HostTextBox.IsEnabled = false;
            this.HostTextBox.Text = "127.0.0.1";
        }

        private void LocalhostCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            this.HostTextBox.IsEnabled = true;
            this.HostTextBox.Text = GetIpAddress();
        }

        private static string GetIpAddress()
        {
            foreach (IPAddress ip in Dns.GetHostEntry(Environment.MachineName).AddressList)
            {
                if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    return Convert.ToString(ip);
                }
            }
            return "";
        }
    }
}
