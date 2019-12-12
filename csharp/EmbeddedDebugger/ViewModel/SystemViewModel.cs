using EmbeddedDebugger.Model;
using EmbeddedDebugger.Properties;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EmbeddedDebugger.Connectors.Interfaces;

namespace EmbeddedDebugger.ViewModel
{
    public class SystemViewModel
    {
        private readonly ModelManager modelManager;
        private readonly Model.DebugProtocol debugProtocol;


        public SystemViewModel(ModelManager modelManager)
        {
            this.modelManager = modelManager;
            this.debugProtocol = modelManager.DebugProtocol;
        }

        public void ConnectConnector(IConnector connector)
        {
            this.debugProtocol.Connector = connector;
            this.debugProtocol.Connect();
        }

        public void DisconnectConnector(IConnector connector)
        {
            this.debugProtocol.Disconnect();
        }

        public void ShowConnectorSettings(IConnector connector)
        {
            // TODO Add connector to showsettings
            this.debugProtocol.ShowSettings();
        }


        public void ResetTime(CpuNode cpuNode = null)
        {
            modelManager.ResetTime();
        }
    }
}
