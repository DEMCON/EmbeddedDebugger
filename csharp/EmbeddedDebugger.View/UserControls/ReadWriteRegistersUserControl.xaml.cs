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
using EmbeddedDebugger.View.UserControls.ObjectDisplayers;
using EmbeddedDebugger.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace EmbeddedDebugger.View.UserControls
{
    /// <summary>
    /// Interaction logic for ReadWriteRegistersUserControl.xaml
    /// </summary>
    public partial class ReadWriteRegistersUserControl : UserControl
    {
        private SystemViewModel systemViewModel;

        private IList<Register> registers;

        public ReadWriteRegistersUserControl()
        {
            InitializeComponent();
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
            //if (startLength == tb.Text.Length)
            //RegisterDataGrid.SearchValue = SearchTextBox.Text;
        }

        private void ResetTimeButton_Click(object sender, RoutedEventArgs e)
        {
            int decimation_ms = 10;
            if (!int.TryParse(Decimation.Text, out decimation_ms))
                decimation_ms = 10;

            Decimation.Text = decimation_ms.ToString();

        }


        private void RemoveAllChannels_Click(object sender, RoutedEventArgs e)
        {
            foreach (Register r in registers.First().CpuNode.DebugChannelRegisters)
            {
                r.ChannelMode = DebugProtocol.Enums.ChannelMode.Off;
            }
        }

        private void ExpandAll_Click(object sender, RoutedEventArgs e)
        {
            // RegisterDataGrid.ExpandAll();
        }

        private void CollapseAll_Click(object sender, RoutedEventArgs e)
        {
            //    RegisterDataGrid.CollapseAll();
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
                // if (r != null && r.IsWritable &&
                //         RegisterDataGrid.CurrentColumn != null &&
                //          RegisterDataGrid.CurrentColumn.Header != null && RegisterDataGrid.CurrentColumn.Header.Equals("Value"))
                {
                    r.EnableValueUpdates = false;
                }
            }
        }

        private void ClearFilter_MouseDown(object sender, MouseButtonEventArgs e)
        {
            SearchTextBox.Clear();
        }

        public void Refresh()
        {
            if (this.registers == null || !this.registers.SequenceEqual(this.systemViewModel.GetRegisters()))
            {
                this.registers = this.systemViewModel.GetRegisters();
                if (this.registers != null)
                {
                    this.Dispatcher.Invoke(() => { this.RegistersStackPanel.Children.Clear(); });
                    foreach (Register r in this.registers)
                    {
                        this.Dispatcher.Invoke(() => { this.RegistersStackPanel.Children.Add(new RegisterDisplayerUserControl() { Register = r }); });
                    }
                }
            }
        }

        public void Update(object o, EventArgs e)
        {
            this.Refresh();
        }

        private void ReadWriteRegistersUserControl_OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
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

        private void ReadOnceChannels_OnClick(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }
    }
}
