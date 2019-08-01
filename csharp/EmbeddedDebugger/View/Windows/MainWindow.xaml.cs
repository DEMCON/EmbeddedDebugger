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
using EmbeddedDebugger.Properties;
using EmbeddedDebugger.ViewModel;
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
using System.Windows.Threading;

namespace EmbeddedDebugger.View.Windows
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ModelManager model;

        public MainWindow()
        {
            InitializeComponent();

            //model = new ModelManager();
            //viewModelManager = new ViewModelManager(model);
            //viewModelManager = Resources.
            model = ((ViewModelManager)Application.Current.Resources["ViewModelManager"]).ModelManager;
            // Set up the ConnectUserControl
            model.HasConnected += ConnectUserControl.HasConnected;
            model.HasDisconnected += ConnectUserControl.HasDisconnected;
            model.NewCPUNodeFound += ConnectUserControl.NewCPUNodeFound;
            ConnectUserControl.Connect += model.ConnectRequest;
            ConnectUserControl.Disconnect += model.DisconnectRequest;
            ConnectUserControl.ShowSettings += model.ShowSettings;
            ConnectUserControl.SelectedConnectorChanged += model.ConnectorChanged;
            ConnectUserControl.Connectors = model.Connectors;
            ConnectUserControl.Nodes = model.Nodes;

            // Set up the RegisterUserControl
            model.NewCPUNodeFound += RegisterUserControl.NewCPUNodeFound;
            model.ConfigCompletelyLoaded += RegisterUserControl.ConfigurationCompletelySend;
            RegisterUserControl.Nodes = model.Nodes;
            RegisterUserControl.RequestOnce += model.RequestOnce;

            // Set up the TerminalUserControl
            model.NewCPUNodeFound += TerminalUserControl.NewCPUNodeFound;
            TerminalUserControl.Nodes = model.Nodes;
            TerminalUserControl.NewMessage += model.NewDebugMessageToEmbedded;

            // Set up the TraceUserControl
            model.NewCPUNodeFound += TraceUserControl.NewCPUNodeFound;
            TraceUserControl.Nodes = model.Nodes;

            // Set up the LoggingUserControl
            LoggingUserControl.Logger = model.Logger;

            // Set up the eventhandlers here 
            model.HasConnected += Model_HasConnected;
            model.HasDisconnected += Model_HasDisconnected;

            // Set up the settings
            VariableTypeAsCPP.IsChecked = Settings.Default.CPPVariableTypes;
            RPCEnabledMenuItem.IsChecked = Settings.Default.RPCEnabled;
            RPCEnabledMenuItem_Click(this, new RoutedEventArgs());
        }

        private void Model_HasDisconnected(object sender, EventArgs e)
        {
            if (!ConnectorStatusBarItem.Dispatcher.CheckAccess())
            {
                ConnectorStatusBarItem.Dispatcher.Invoke(delegate { Model_HasDisconnected(sender, e); });
                return;
            }
            ConnectorStatusBarItem.Content = "Disconnected";
        }

        private void Model_HasConnected(object sender, EventArgs e)
        {
            if (!ConnectorStatusBarItem.Dispatcher.CheckAccess())
            {
                ConnectorStatusBarItem.Dispatcher.Invoke(delegate { Model_HasConnected(sender, e); });
                return;
            }
            ConnectorStatusBarItem.Content = $"Connected over {model.Connector}";
        }

        private void ExitMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void RPCEnabledMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (RPCEnabledMenuItem.IsChecked)
            {
                try
                {
                    model.RpcInterface.Start();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    MessageBox.Show(ex.Message);
                }
                Settings.Default.RPCEnabled = true;
                Settings.Default.Save();
            }
            else
            {
                model.RpcInterface.Stop();
                Settings.Default.RPCEnabled = false;
                Settings.Default.Save();
            }
        }

        private void RPCPortMenuItem_Click(object sender, RoutedEventArgs e)
        {
            RPCSettingsWindow rsw = new RPCSettingsWindow()
            {
                Port = model.RpcInterface.Port,
            };
            if ((bool)rsw.ShowDialog())
            {
                Settings.Default.RPCPort = rsw.Port;
                Settings.Default.Save();
                model.RpcInterface.Stop();
                model.RpcInterface.Port = rsw.Port;
                model.RpcInterface.Start();
            }
        }

        private void VariableTypeAsCPP_Checked(object sender, RoutedEventArgs e)
        {
            Settings.Default.CPPVariableTypes = true;
            Settings.Default.Save();
            RegisterUserControl.ReadWriteRegisterUserControl.RegisterDataGrid.Items.Refresh();
        }

        private void VariableTypeAsCPP_Unchecked(object sender, RoutedEventArgs e)
        {
            Settings.Default.CPPVariableTypes = false;
            Settings.Default.Save();
            RegisterUserControl.ReadWriteRegisterUserControl.RegisterDataGrid.Items.Refresh();
        }

        private void AboutMenuItem_Click(object sender, RoutedEventArgs e)
        {
            new AboutBox(this).ShowDialog();
        }
    }
}
