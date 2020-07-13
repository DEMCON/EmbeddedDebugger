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
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EmbeddedDebugger.DebugProtocol;
using EmbeddedDebugger.Model;

namespace EmbeddedDebugger.ViewModel
{
    public class PlottingViewModel
    {
        private readonly ModelManager modelManager;
        private readonly Model.ConnectionManager debugProtocol;
        private readonly PlottingBtreeManager plottingBtreeManager;

        public List<Register> RegistersToPlot { get; }

        public PlottingViewModel(ModelManager modelManager)
        {
            this.modelManager = modelManager;
            this.debugProtocol = modelManager.DebugProtocol;
            this.plottingBtreeManager = modelManager.BTreeManager;
            this.RegistersToPlot = new List<Register>();
        }

        public void AddPlottingRegister(Register register)
        {
            this.RegistersToPlot.Add(register);
            this.plottingBtreeManager.AddBTree(register);
        }

        public void RemovePlottingRegister(Register register)
        {
            this.RegistersToPlot.Remove(register);
            this.plottingBtreeManager.RemoveBTree(register);
        }
    }
}
