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
    /// Interaction logic for RegsiterUserControl.xaml
    /// </summary>
    public partial class RegisterUserControl : UserControl
    {
        public List<CpuNode> Nodes { get => CpuNodeChooserUserControl.Nodes; set => CpuNodeChooserUserControl.Nodes = value; }

        #region EventHandlers
        public delegate void ResetTimeHandler(int decimation_ms);
        public event ResetTimeHandler ResetTime = delegate { };
        public event EventHandler<int> RequestOnce = delegate { };
        #endregion

        public RegisterUserControl()
        {
            InitializeComponent();
            CpuNodeChooserUserControl.SelectedCPUChanged += CpuNodeChooserUserControl_SelectedCPUChanged;
            ReadWriteRegisterUserControl.RegisterPlottingChanged += ReadWriteRegisterUserControl_RegisterPlottingChanged;
            ReadWriteRegisterUserControl.ResetTime += ReadWriteRegisterUserControl_ResetTime;
            ReadWriteRegisterUserControl.RequestOnce += ReadWriteRegisterUserControl_RequestOnce;
        }

        public void ConfigurationCompletelySend(object sender, EventArgs e)
        {
            ReadWriteRegisterUserControl.Registers = CpuNodeChooserUserControl.Registers;
            ReadWriteRegisterUserControl.NewRegisterAdded();
        }

        private void ReadWriteRegisterUserControl_RequestOnce(object sender, EventArgs e)
        {
            foreach(CpuNode cpu in CpuNodeChooserUserControl.Nodes)
            {
                RequestOnce(this, cpu.ID);
            }
        }

        private void ReadWriteRegisterUserControl_ResetTime(int decimation_ms)
        {
            ResetTime(decimation_ms);
            foreach (Register r in new List<Register>(PlotUserControl.PlotRegisters))
            {
                if (!ReadWriteRegisterUserControl.Registers.Contains(r))
                    PlotUserControl.RemoveRegisterFromPlot(r);
            }
        }

        private void ReadWriteRegisterUserControl_RegisterPlottingChanged(object sender, Register e)
        {
            if (e.Plot)
            {
                PlotUserControl.AddRegisterToPlot(e);
            }
            else
            {
                PlotUserControl.RemoveRegisterFromPlot(e);
            }
        }

        private void CpuNodeChooserUserControl_SelectedCPUChanged(object sender, EventArgs e)
        {
            ReadWriteRegisterUserControl.Registers = CpuNodeChooserUserControl.Registers;
        }

        public void NewCPUNodeFound(object sender, EventArgs e)
        {
            CpuNodeChooserUserControl.RefreshCPUNodeList(sender, e);
        }
    }
}
