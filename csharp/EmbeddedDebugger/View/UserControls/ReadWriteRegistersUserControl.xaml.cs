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
using System.Drawing;
using System.Globalization;
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
    /// Interaction logic for ReadWriteRegistersUserControl.xaml
    /// </summary>
    public partial class ReadWriteRegistersUserControl : UserControl
    {
        #region Properties
        private IList<Register> registers;
        public IList<Register> Registers
        {
            get => registers;
            set
            {
                registers = value;
            }
        }
        #endregion

        #region EventHandlers
        public event EventHandler<Register> RegisterPlottingChanged = delegate { };
        public delegate void ResetTimeHandler(int decimation_ms);
        public event ResetTimeHandler ResetTime = delegate { };
        public event EventHandler RequestOnce = delegate { };
        #endregion

        public ReadWriteRegistersUserControl()
        {
            InitializeComponent();
        }

        public void NewRegisterAdded()
        {
            if (!RegisterDataGrid.Dispatcher.CheckAccess())
            {
                RegisterDataGrid.Dispatcher.Invoke(delegate { NewRegisterAdded(); });
                return;
            }
            RegisterDataGrid.TreeItemsDataSource = registers;
        }

        private void RefreshAllButton_Click(object sender, RoutedEventArgs e)
        {
            if (registers == null) return;
            foreach (Register register in registers)
            {
                register.RequestNewValue();
            }
        }

        private async void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox tb = (TextBox)sender;
            int startLength = tb.Text.Length;

            await Task.Delay(300);
            if (startLength == tb.Text.Length)
                RegisterDataGrid.SearchValue = SearchTextBox.Text;
        }

        private void ResetTimeButton_Click(object sender, RoutedEventArgs e)
        {
            int decimation_ms = 10;
            if (!int.TryParse(Decimation.Text, out decimation_ms))
                decimation_ms = 10;

            Decimation.Text = decimation_ms.ToString();

            ResetTime(decimation_ms);
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            ((Register)RegisterDataGrid.CurrentItem).RequestNewValue();
        }

        private void PlotCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            Register r = (Register)RegisterDataGrid.CurrentItem;
            if (r == null) return;
            RegisterPlottingChanged(this, r);
        }

        private void PlotCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            Register r = (Register)RegisterDataGrid.CurrentItem;
            if (r == null) return;
            RegisterPlottingChanged(this, r);
        }

        private void RemoveAllChannels_Click(object sender, RoutedEventArgs e)
        {
            foreach (Register r in registers.First().CpuNode.DebugChannelRegisters)
            {
                r.ChannelMode = DebugProtocol.Enums.ChannelMode.Off;
            }
        }

        private void ReadOnceChannels_Click(object sender, RoutedEventArgs e)
        {
            RequestOnce(sender, e);
        }

        private void ExpandAll_Click(object sender, RoutedEventArgs e)
        {
            RegisterDataGrid.ExpandAll();
        }

        private void CollapseAll_Click(object sender, RoutedEventArgs e)
        {
            RegisterDataGrid.CollapseAll();
        }

        private void RegisterDataGrid_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            foreach (DataGridCellInfo di in e.RemovedCells)
            {
                if (!(di.Item is Register))
                    continue;

                Register r = (Register)di.Item;
                if (r != null && r.IsWritable)
                    r.EnableValueUpdates = true;
            }

            foreach (DataGridCellInfo di in e.AddedCells)
            {
                if (!(di.Item is Register))
                    continue;

                Register r = (Register)di.Item;
                if (r != null && r.IsWritable &&
                    RegisterDataGrid.CurrentColumn != null &&
                    RegisterDataGrid.CurrentColumn.Header != null && RegisterDataGrid.CurrentColumn.Header.Equals("Value"))
                {
                    r.EnableValueUpdates = false;
                }
            }
        }

        private void ClearFilter_MouseDown(object sender, MouseButtonEventArgs e)
        {
            SearchTextBox.Clear();
        }
    }
}
