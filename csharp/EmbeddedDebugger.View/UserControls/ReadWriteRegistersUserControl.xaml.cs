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
using EmbeddedDebugger.View.DataContext;
using EmbeddedDebugger.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
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
        private RefreshViewModel refreshViewModel;
        private PlottingViewModel plottingViewModel;

        public IList<Register> Registers { get; set; }


        public ReadWriteRegistersUserControl()
        {
            InitializeComponent();
        }


        private void RefreshAllButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.Registers == null) return;
            foreach (Register register in Registers)
            {
                this.systemViewModel.RequestNewValue(register);
                //register.RequestNewValue();
            }
        }

        private async void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox tb = (TextBox)sender;
            int startLength = tb.Text.Length;

            await Task.Delay(300);
            // Check if the user stopped typing 
            if (startLength == tb.Text.Length)
            {
                string searchText = this.SearchTextBox.Text;
                this.RegistersStackPanel.ItemsSource = this.Registers
                    .Where(x => this.ParentOrChildMatches(x, searchText))
                    .Select(x => new RegisterDataContext()
                    {
                        Register = x,
                        RefreshViewModel = this.refreshViewModel,
                        SystemViewModel = this.systemViewModel,
                        PlottingViewModel = this.plottingViewModel
                    });
            }
        }

        private bool ParentOrChildMatches(Register register, string searchText)
        {
            return Regex.IsMatch(register.Name, searchText) || register.ChildRegisters?.Any(x => this.ParentOrChildMatches(x, searchText)) == true;
        }

        private void ResetTimeButton_Click(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(this.Decimation.Text, out int decimationMs))
                decimationMs = 10;

            this.Decimation.Text = decimationMs.ToString();

        }


        private void RemoveAllChannels_Click(object sender, RoutedEventArgs e)
        {
            foreach (Register r in Registers.First().CpuNode.DebugChannelRegisters)
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

        private void ClearFilter_MouseDown(object sender, MouseButtonEventArgs e)
        {
            this.SearchTextBox.Clear();
        }

        public void Refresh()
        {
            this.Dispatcher.Invoke(() =>
            {
                if (this.Registers == null || !this.Registers.SequenceEqual(this.systemViewModel.GetRegisters()))
                {
                    this.Registers = this.systemViewModel.GetRegisters();
                    if (this.Registers == null) return;
                    this.RegistersStackPanel.ItemsSource = this.Registers.
                        Select(x => new RegisterDataContext()
                        {
                            Register = x,
                            RefreshViewModel = this.refreshViewModel,
                            SystemViewModel = this.systemViewModel,
                            PlottingViewModel = this.plottingViewModel
                        });
                }
            });
        }

        public void Update(object o, EventArgs e)
        {
            this.Refresh();
        }

        private void ReadWriteRegistersUserControl_OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue is ViewModelManager vmmOld)
            {
                vmmOld.RefreshViewModel.RefreshLow -= this.Update;
            }
            if (e.NewValue is ViewModelManager vmm)
            {
                this.systemViewModel = vmm.SystemViewModel;
                this.refreshViewModel = vmm.RefreshViewModel;
                this.plottingViewModel = vmm.PlottingViewModel;
                vmm.RefreshViewModel.RefreshLow += this.Update;
            }
        }

        private void ReadOnceChannels_OnClick(object sender, RoutedEventArgs e)
        {
            this.systemViewModel.ReadOnceOfChannels();
        }
    }
}
