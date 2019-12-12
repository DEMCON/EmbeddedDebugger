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
using EmbeddedDebugger.Connectors.Interfaces;
using EmbeddedDebugger.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace EmbeddedDebugger.View.UserControls
{
    /// <summary>
    /// Interaction logic for CpuNodeChooserUserControl.xaml
    /// </summary>
    public partial class ConnectorChooserUserControl : UserControl
    {
        private SystemViewModel systemViewModel;

        #region Properties
        private List<IConnector> connectors;
        public List<IConnector> Connectors
        {
            get => this.connectors;
            set
            {
                this.connectors = value;
                ConnectorChooserComboBox.IsSynchronizedWithCurrentItem = true;
                ConnectorChooserComboBox.ItemsSource = this.connectors.OrderBy(x => x.ToString());
            }
        }
        #endregion

        public ConnectorChooserUserControl()
        {
            InitializeComponent();
        }

        private void ConnectButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.ConnectorChooserComboBox.SelectedItem is IConnector connector)
            {
                this.systemViewModel.ConnectConnector(connector);
            }
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.ConnectorChooserComboBox.SelectedItem is IConnector connector)
            {
                this.systemViewModel.ShowConnectorSettings(connector);
            }
        }

        private void DisconnectButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.ConnectorChooserComboBox.SelectedItem is IConnector connector)
            {
                this.systemViewModel.DisconnectConnector(connector);
            }
        }

        public void Refresh()
        {
            if (!this.Dispatcher.CheckAccess())
            {
                this.Dispatcher.Invoke(this.Refresh);
                return;
            }

            bool connected = this.systemViewModel.ConnectorConnected();
            this.ConnectButton.IsEnabled = !connected;
            this.SettingsButton.IsEnabled = !connected;
            this.DisconnectButton.IsEnabled = connected;
            this.ConnectorChooserComboBox.IsEnabled = !connected;
        }

        public void Update(object o, EventArgs e)
        {
            this.Refresh();
        }

        private void ConnectorChooserUserControl_OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue is ViewModelManager vmmOld)
            {
                vmmOld.RefreshLow -= this.Update;
            }
            if (e.NewValue is ViewModelManager vmm)
            {
                this.systemViewModel = vmm.SystemViewModel;
                vmm.RefreshLow += this.Update;
            }
        }

        private void ConnectorChooserUserControl_OnLoaded(object sender, RoutedEventArgs e)
        {
            this.Connectors = this.systemViewModel.GetConnectors();
        }
    }
}
