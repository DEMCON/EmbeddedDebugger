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
using EmbeddedDebugger.DebugProtocol.Enums;
using EmbeddedDebugger.Model;
using EmbeddedDebugger.Model.Messages;
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
    /// Interaction logic for TraceUserControl.xaml
    /// </summary>
    public partial class TraceUserControl : UserControl
    {
        private List<CpuNode> registeredNodes;

        public TraceUserControl()
        {
            InitializeComponent();
            registeredNodes = new List<CpuNode>();
        }

        public void NewCPUNodeFound(object sender, EventArgs e)
        {
            CpuNodeChooser.RefreshCPUNodeList(sender, e);
        }

        private void CpuNodeChooser_SelectedCPUChanged(object sender, EventArgs e)
        {
        }

        private void Node_NewTraceMessageAdded(object sender, TraceMessage e)
        {
            if (TraceLevelActive(e.TraceLevel) == false) return;
            TraceTerminal.AddMessage(e);
        }

        private bool? TraceLevelActive(TraceLevel level)
        {
            bool? returnable = null;
            Dispatcher.Invoke(delegate
            {
                switch (level)
                {
                    case TraceLevel.Debug:
                        returnable= DebugCheckBox.IsChecked;
                        break;
                    case TraceLevel.Error:
                        returnable= ErrorCheckBox.IsChecked;
                        break;
                    case TraceLevel.Fatal:
                        returnable= FatalCheckBox.IsChecked;
                        break;
                    case TraceLevel.Info:
                        returnable= InfoCheckBox.IsChecked;
                        break;
                    case TraceLevel.Trace:
                        returnable= TraceCheckBox.IsChecked;
                        break;
                    case TraceLevel.Warning:
                        returnable= WarningCheckBox.IsChecked;
                        break;
                }
            });
            return returnable;
        }

        private void RefreshMessages()
        {
            if (registeredNodes == null) return;
            TraceTerminal.SetMessages(registeredNodes.SelectMany(x => x.TraceMessages)
                .Where(x=> 
                    TraceCheckBox.IsChecked == true && x.TraceLevel == TraceLevel.Trace ||
                    DebugCheckBox.IsChecked == true && x.TraceLevel == TraceLevel.Debug ||
                    InfoCheckBox.IsChecked == true && x.TraceLevel == TraceLevel.Info ||
                    WarningCheckBox.IsChecked == true && x.TraceLevel == TraceLevel.Warning ||
                    ErrorCheckBox.IsChecked == true && x.TraceLevel == TraceLevel.Error || 
                    FatalCheckBox.IsChecked == true && x.TraceLevel == TraceLevel.Fatal
                    )
                .OrderBy(x => x.DateTime).ToList());
        }

        private void TraceCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            RefreshMessages();
        }

        private void TraceCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            RefreshMessages();
        }

        private void DebugCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            RefreshMessages();
        }

        private void DebugCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            RefreshMessages();
        }

        private void InfoCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            RefreshMessages();
        }

        private void InfoCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            RefreshMessages();
        }

        private void WarningCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            RefreshMessages();
        }

        private void WarningCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            RefreshMessages();
        }

        private void ErrorCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            RefreshMessages();
        }

        private void ErrorCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            RefreshMessages();
        }

        private void FatalCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            RefreshMessages();
        }

        private void FatalCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            RefreshMessages();
        }
    }
}
