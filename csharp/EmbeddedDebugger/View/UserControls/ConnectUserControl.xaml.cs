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
using EmbeddedDebugger.Model;
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
    /// Interaction logic for ConnectUserControl.xaml
    /// </summary>
    public partial class ConnectUserControl : UserControl
    {
        #region Properties
        private List<CpuNode> nodes;
        public List<CpuNode> Nodes { get => nodes; set => nodes = value; }
        public List<object> Connectors { get => ConnectorChooserUserControl.Connectors; set => ConnectorChooserUserControl.Connectors = value; }
        public object SelectedConnector { get => ConnectorChooserUserControl.SelectedConnector; set => ConnectorChooserUserControl.SelectedConnector = value; }
        public bool connected { get => ConnectorChooserUserControl.Connected; set => ConnectorChooserUserControl.Connected = value; }
        #endregion

        #region EventsHandlers
        public event EventHandler Connect = delegate { };
        public event EventHandler Disconnect = delegate { };
        public event EventHandler ShowSettings = delegate { };
        public event EventHandler SelectedConnectorChanged = delegate { };
        #endregion

        public ConnectUserControl()
        {
            InitializeComponent();
            ConnectorChooserUserControl.Connect += ConnectorChooserUserControl_Connect;
            ConnectorChooserUserControl.Disconnect += ConnectorChooserUserControl_Disconnect;
            ConnectorChooserUserControl.ShowSettings += ConnectorChooserUserControl_ShowSettings;
            ConnectorChooserUserControl.SelectedConnectorChanged += ConnectorChooserUserControl_SelectedConnectorChanged;
        }

        #region delegates
        public void HasConnected(object sender, EventArgs e)
        {
            ConnectorChooserUserControl.HasConnected(sender, e);
        }

        public void HasDisconnected(object sender, EventArgs e)
        {
            ConnectorChooserUserControl.HasDisconnected(sender, e);
        }

        public void NewCPUNodeFound(object sender, EventArgs e)
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.Invoke(delegate { NewCPUNodeFound(sender, e); });
            }
            else
            {
                NodesDataGrid.ItemsSource = nodes;
                NodesDataGrid.Items.Refresh();
            }
        }

        private void ConnectorChooserUserControl_SelectedConnectorChanged(object sender, EventArgs e)
        {
            SelectedConnectorChanged(this, e);
        }

        private void ConnectorChooserUserControl_ShowSettings(object sender, EventArgs e)
        {
            ShowSettings(sender, e);
        }

        private void ConnectorChooserUserControl_Disconnect(object sender, EventArgs e)
        {
            Disconnect(sender, e);
        }

        private void ConnectorChooserUserControl_Connect(object sender, EventArgs e)
        {
            Connect(sender, e);
        }

        //public void UpdateGuiValues()
        //{
        //    NodesDataGrid.Items.Refresh();
        //}
        #endregion
    }
}
