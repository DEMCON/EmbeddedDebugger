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
using EmbeddedDebugger.DebugProtocol;

namespace EmbeddedDebugger.View.UserControls
{
    /// <summary>
    /// Interaction logic for TerminalUserControl.xaml
    /// </summary>
    public partial class TerminalUserControl : UserControl
    {
        private List<CpuNode> registeredNodes;
        private bool enabled;

        #region Events
        public event EventHandler<string> NewMessage = delegate { };
        #endregion

        public TerminalUserControl()
        {
            InitializeComponent();
            registeredNodes = new List<CpuNode>();
            enabled = true;
        }



        private void Node_NewTerminalDataAdded(CpuNode node, string s)
        {
            if (enabled == true)
            {
                if (registeredNodes.Count == 1)
                {
                    EmbeddedTerminal.AddResponse(s);
                }
                else
                {
                    EmbeddedTerminal.AddResponse($"({node}){s}");
                }
            }

        }


        public void NewCPUNodeFound(object sender, EventArgs e)
        {
            CpuNodeChooser.RefreshCPUNodeList(sender, e);
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            enabled = true;            
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            enabled = false;
        }
    }
}
