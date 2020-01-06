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
using EmbeddedDebugger.ViewModel;

namespace EmbeddedDebugger.View.UserControls
{
    /// <summary>
    /// Interaction logic for ConnectUserControl.xaml
    /// </summary>
    public partial class ConnectUserControl : UserControl
    {
        private SystemViewModel systemViewModel;

        public ConnectUserControl()
        {
            InitializeComponent();
            
        }

        public void Refresh()
        {
            if (!this.Dispatcher.CheckAccess())
            {
                this.Dispatcher.Invoke(this.Refresh);
                return;
            }
            this.NodesDataGrid.Items.Refresh();
        }

        public void Update(object o, EventArgs e)
        {
            this.Refresh();
        }

        private void ConnectUserControl_OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue is ViewModelManager vmmOld)
            {
                vmmOld.RefreshLow -= this.Update;
            }
            if (e.NewValue is ViewModelManager vmm)
            {
                this.systemViewModel = vmm.SystemViewModel;
                vmm.RefreshLow += this.Update;
                this.NodesDataGrid.ItemsSource = this.systemViewModel.GetCpuNodes();
            }
        }
    }
}
