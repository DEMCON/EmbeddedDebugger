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

namespace EmbeddedDebugger.View.UserControls
{
    /// <summary>
    /// Interaction logic for CpuNodeChooserUserControl.xaml
    /// </summary>
    public partial class ConnectorChooserUserControl : UserControl
    {
        #region Properties
        private List<object> connectors;
        public List<object> Connectors
        {
            get => connectors;
            set
            {
                connectors = value;
                ConnectorChooserComboBox.IsSynchronizedWithCurrentItem = true;
                ConnectorChooserComboBox.ItemsSource = Connectors.OrderBy(x => x.ToString());
            }
        }

        private object selectedConnector;
        public object SelectedConnector { get => selectedConnector; set => selectedConnector = value; }

        private bool connected = false;
        public bool Connected { get => connected; set => connected = value; }
        #endregion

        #region EventHandlers
        public event EventHandler Connect = delegate { };
        public event EventHandler Disconnect = delegate { };
        public event EventHandler ShowSettings = delegate { };
        public event EventHandler SelectedConnectorChanged = delegate { };
        #endregion

        public ConnectorChooserUserControl()
        {
            InitializeComponent();
        }

        public void HasConnected(object sender, EventArgs e)
        {
            connected = true;
            Refresh();
        }

        public void HasDisconnected(object sender, EventArgs e)
        {
            connected = false;
            Refresh();
        }

        private void ConnectButton_Click(object sender, RoutedEventArgs e)
        {
            Connect(this, new EventArgs());
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            ShowSettings(this, new EventArgs());
        }

        private void DisconnectButton_Click(object sender, RoutedEventArgs e)
        {
            Disconnect(this, new EventArgs());
        }

        public void Refresh()
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.Invoke(new Action(Refresh));
                return;
            }
            ConnectButton.IsEnabled = !connected;
            SettingsButton.IsEnabled = !connected;
            DisconnectButton.IsEnabled = connected;
            ConnectorChooserComboBox.IsEnabled = !connected;
        }

        private void ConnectorChooserComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            selectedConnector = ConnectorChooserComboBox.SelectedItem;
            SelectedConnectorChanged(this, new EventArgs());
        }
    }
}
