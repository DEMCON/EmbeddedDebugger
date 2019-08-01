using EmbeddedDebugger.Model;
using EmbeddedDebugger.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmbeddedDebugger.ViewModel
{
    public class SystemViewModel
    {
        private readonly ModelManager modelManager;


        public SystemViewModel(ModelManager modelManager)
        {
            this.modelManager = modelManager;
            
        }



        public void ResetTime(CpuNode cpuNode = null)
        {

            modelManager.ResetTime();
        }
    }
}
