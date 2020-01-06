using EmbeddedDebugger.Connectors.Interfaces;
using EmbeddedDebugger.DebugProtocol.Enums;
using EmbeddedDebugger.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EmbeddedDebugger.ViewModel
{
    public class SystemViewModel
    {
        private readonly ModelManager modelManager;
        private readonly Model.DebugProtocol debugProtocol;

        public CpuNode SelectedCpuNode { get; set; }


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

        public IList<Register> GetRegisters()
        {
            if (SelectedCpuNode == null)
            {
                return null; this.GetCpuNodes().SelectMany(x => x.Registers).ToList();
            }
            else
            {
                Console.WriteLine(this.SelectedCpuNode);
                return SelectedCpuNode.Registers;
            }
        }

        public void ResetTime(CpuNode cpuNode = null)
        {
            this.modelManager.ResetTime();
        }

        public void RequestNewValue(Register register)
        {
            this.modelManager.DebugProtocol.QueryRegister(register.CpuID, register.CpuNode, register);
        }

        public bool UpdateChannelMode(Register register, ChannelMode channelMode)
        {
            bool result = false;
            if (!register.CpuNode.DebugChannels.ContainsValue(register))
            {
                for (byte i = 0; i < register.CpuNode.MaxNumberOfDebugChannels; i++)
                {
                    if (!register.CpuNode.DebugChannels.ContainsKey(i))
                    {
                        register.CpuNode.DebugChannels.Add(i, register);
                        this.modelManager.DebugProtocol.ConfigChannel(register.CpuID, i, channelMode, register);
                        result = true;
                        break;
                    }
                }
            }
            else
            {
                int channelId = register.CpuNode.DebugChannels.FirstOrDefault(x => x.Value == register).Key;
                this.modelManager.DebugProtocol.ConfigChannel(register.CpuID, (byte)channelId, channelMode, register);
                if (channelMode == ChannelMode.Off) this.modelManager.DebugChannels.Remove((byte)channelId);
            }
            return result;
        }
    }
}
