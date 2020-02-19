using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EmbeddedDebugger.Model;

namespace EmbeddedDebugger.ViewModel
{
    public class PlottingViewModel
    {
        private readonly ModelManager modelManager;
        private readonly Model.DebugProtocol debugProtocol;

        public List<Register> RegistersToPlot;
        
        public PlottingViewModel(ModelManager modelManager)
        {
            this.modelManager = modelManager;
            this.debugProtocol = modelManager.DebugProtocol;
            this.RegistersToPlot = new List<Register>();
        }

        
    }
}
