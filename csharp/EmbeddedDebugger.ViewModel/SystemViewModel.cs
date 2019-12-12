using EmbeddedDebugger.Connectors.Interfaces;
using EmbeddedDebugger.Model;
using System.Collections.Generic;

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

        #region Nodes

        public List<CpuNode> GetCpuNodes()
        {
            return this.modelManager.Nodes;
        }

        #endregion

        #region Connector
        public List<IConnector> GetConnectors()
        {
            return this.modelManager.Connectors;
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
            this.debugProtocol.ShowSettings(connector);
        }

        public bool ConnectorConnected()
        {
            // TODO: Add possibility to do this on connector level?
            return this.debugProtocol.IsConnected;
        }
        #endregion

        public void ResetTime(CpuNode cpuNode = null)
        {
            modelManager.ResetTime();
        }
    }
}
