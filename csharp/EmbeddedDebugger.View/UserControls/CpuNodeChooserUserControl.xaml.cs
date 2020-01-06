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
    public partial class CpuNodeChooserUserControl : UserControl
    {
        private SystemViewModel systemViewModel;
        private List<CpuNode> nodes;

        public CpuNodeChooserUserControl()
        {
            InitializeComponent();
        }

        public void RefreshCPUNodeList(object sender, EventArgs e)
        {
            if (!CpuNodeComboBox.Dispatcher.CheckAccess())
            {
                CpuNodeComboBox.Dispatcher.Invoke(delegate { RefreshCPUNodeList(sender, e); });
            }
            else
            {
                // TODO Add functionality to keep the existing item "selected"
                /*
                object selected = CpuNodeComboBox.SelectedItem;
                int selectedIndex = CpuNodeComboBox.SelectedIndex;
                CpuNodeComboBox.Items.Clear();
                foreach (CpuNode node in nodes)
                {
                    CpuNodeComboBox.Items.Add(node);
                }
                if (selected != null && CpuNodeComboBox.Items.Contains(selected))
                {
                    CpuNodeComboBox.SelectedItem = selected;
                }
                else if (CpuNodeComboBox.Items.Count > selectedIndex && selectedIndex >= 0)
                {
                    CpuNodeComboBox.SelectedIndex = selectedIndex;
                }
                else if (CpuNodeComboBox.Items.Count > 0)
                {
                    CpuNodeComboBox.SelectedIndex = 0;
                }*/
            }
        }

        private void CpuNodeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.ShowAllCheckBox.IsChecked == false)
            {
                if (this.CpuNodeComboBox.SelectedItem is CpuNode cpu)
                {
                    this.systemViewModel.SelectedCpuNode = cpu;
                }
            }
        }

        private void ShowAllCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            this.systemViewModel.SelectedCpuNode = null;
        }

        private void ShowAllCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            if (this.CpuNodeComboBox.SelectedItem is CpuNode cpu)
            {
                this.systemViewModel.SelectedCpuNode = cpu;
            }
        }

        private void CpuNodeChooserUserControl_OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue is ViewModelManager oldVmm)
            {
                oldVmm.RefreshLow -= this.Vmm_RefreshLow;
            }
            if (e.NewValue is ViewModelManager vmm)
            {
                this.systemViewModel = vmm.SystemViewModel;
                vmm.RefreshLow += this.Vmm_RefreshLow;
            }
        }
        public void Refresh()
        {
            if (!this.Dispatcher.CheckAccess())
            {
                this.Dispatcher.Invoke(this.Refresh);
                return;
            }
            if (this.nodes == null || !this.nodes.SequenceEqual(this.systemViewModel.GetCpuNodes()))
            {
                int selectedIndex = this.CpuNodeComboBox.SelectedIndex;
                this.CpuNodeComboBox.ItemsSource = null;
                this.nodes = this.systemViewModel.GetCpuNodes().ToList();
                this.CpuNodeComboBox.ItemsSource = this.nodes;
                if (this.nodes.Count > 0) this.CpuNodeComboBox.SelectedIndex = selectedIndex >= 0 ? selectedIndex : 0;
            }
        }

        private void Vmm_RefreshLow(object sender, EventArgs ea)
        {
            this.Refresh();
        }
    }
}
