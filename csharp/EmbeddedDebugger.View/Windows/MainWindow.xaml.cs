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
using EmbeddedDebugger.Properties;
using EmbeddedDebugger.ViewModel;
using System;
using System.Windows;

namespace EmbeddedDebugger.View.Windows
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {

        public MainWindow()
        {
            InitializeComponent();

            this.DataContext = new ViewModelManager();
            // Set up the settings
            VariableTypeAsCPP.IsChecked = Settings.Default.CPPVariableTypes;
        }

        private void ExitMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void RPCEnabledMenuItem_Click(object sender, RoutedEventArgs e)
        {
        //    if (RPCEnabledMenuItem.IsChecked)
        //    {
        //        try
        //        {
        //            model?.RpcInterface?.Start();
        //        }
        //        catch (Exception ex)
        //        {
        //            Console.WriteLine(ex);
        //            MessageBox.Show(ex.Message);
        //        }
        //        Settings.Default.RPCEnabled = true;
        //        Settings.Default.Save();
        //    }
        //    else
        //    {
        //        model?.RpcInterface?.Stop();
        //        Settings.Default.RPCEnabled = false;
        //        Settings.Default.Save();
        //    }
        }

        private void RPCPortMenuItem_Click(object sender, RoutedEventArgs e)
        {
        //    RPCSettingsWindow rsw = new RPCSettingsWindow()
        //    {
        //        Port = model.RpcInterface.Port,
        //    };
        //    if ((bool)rsw.ShowDialog())
        //    {
        //        Settings.Default.RPCPort = rsw.Port;
        //        Settings.Default.Save();
        //        model.RpcInterface.Stop();
        //        model.RpcInterface.Port = rsw.Port;
        //        model.RpcInterface.Start();
        //    }
        }

        private void VariableTypeAsCPP_Checked(object sender, RoutedEventArgs e)
        {
            Settings.Default.CPPVariableTypes = true;
            Settings.Default.Save();
            //RegisterUserControl.ReadWriteRegisterUserControl.RegisterDataGrid.Items.Refresh();
        }

        private void VariableTypeAsCPP_Unchecked(object sender, RoutedEventArgs e)
        {
            Settings.Default.CPPVariableTypes = false;
            Settings.Default.Save();
            //RegisterUserControl.ReadWriteRegisterUserControl.RegisterDataGrid.Items.Refresh();
        }

        private void AboutMenuItem_Click(object sender, RoutedEventArgs e)
        {
            new AboutBox(this).ShowDialog();
        }
    }
}
