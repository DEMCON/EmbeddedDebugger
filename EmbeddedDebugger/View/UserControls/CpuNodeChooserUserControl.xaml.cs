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
    /// Interaction logic for CpuNodeChooserUserControl.xaml
    /// </summary>
    public partial class CpuNodeChooserUserControl : UserControl
    {
        private CpuNode SelectedNode;
        private List<CpuNode> nodes;
        private List<Register> registers;

        #region Properties
        /// <summary>
        /// The list of nodes, if the checkbox is checked, return all, otherwise return a list with only the selected cpunode
        /// </summary>
        public List<CpuNode> Nodes
        {
            get
            {
                if (ShowAllCheckBox.IsChecked.HasValue && (bool)ShowAllCheckBox.IsChecked)
                {
                    return nodes;
                }
                else
                {
                    if (SelectedNode == null) return new List<CpuNode>();
                    List<CpuNode> returnable = new List<CpuNode>() { SelectedNode };
                    return returnable;
                }
            }
            set
            {
                nodes = value;
            }
        }
        /// <summary>
        /// The registers of the selected cpunodes, keeping in mind that not all have to be returned when showall is not checked
        /// </summary>
        public List<Register> Registers
        {
            get
            {
                if (!Dispatcher.CheckAccess())
                {
                    Dispatcher.Invoke(delegate
                    {
                        if ((bool)ShowAllCheckBox.IsChecked)
                        {
                            registers = SelectedNode.Registers.ToList();
                        }
                        else
                        {
                            registers.Clear();
                            foreach (CpuNode node in nodes)
                            {
                                registers.AddRange(node.Registers);
                            }
                        }
                    });
                }
                else
                {
                    if ((bool)ShowAllCheckBox.IsChecked)
                    {
                        registers = SelectedNode.Registers.ToList();
                    }
                    else
                    {
                        registers.Clear();
                        foreach (CpuNode node in nodes)
                        {
                            registers.AddRange(node.Registers);
                        }
                    }
                }
                return registers;
            }
        }
        #endregion

        #region EventHandlers
        public event EventHandler SelectedCPUChanged = delegate { };
        #endregion

        public CpuNodeChooserUserControl()
        {
            InitializeComponent();
            registers = new List<Register>();
        }

        public void RefreshCPUNodeList(object sender, EventArgs e)
        {
            if (!CpuNodeComboBox.Dispatcher.CheckAccess())
            {
                CpuNodeComboBox.Dispatcher.Invoke(delegate { RefreshCPUNodeList(sender, e); });
            }
            else
            {
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
                }
            }
        }

        private void CpuNodeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Tell the rest of the world a different CPU was selected
            SelectedNode = (CpuNode)CpuNodeComboBox.SelectedItem;
            SelectedCPUChanged(this, new EventArgs());
        }

        private void ShowAllCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            registers.Clear();
            foreach (CpuNode node in nodes)
            {
                registers.AddRange(node.Registers);
            }
            // Since this usercontrol does not discriminate between one or multiple cpus selected
            // Just let the world know a different cpud was selected
            SelectedCPUChanged(this, new EventArgs());
        }

        private void ShowAllCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            registers = SelectedNode.Registers.ToList();
            // Since this usercontrol does not discriminate between one or multiple cpus selected
            // Just let the world know a different cpud was selected
            SelectedCPUChanged(this, new EventArgs());
        }
    }
}
