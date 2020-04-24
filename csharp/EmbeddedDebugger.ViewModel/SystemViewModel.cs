using EmbeddedDebugger.Connectors.Interfaces;
using EmbeddedDebugger.DebugProtocol.Enums;
using EmbeddedDebugger.Model;
using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;

namespace EmbeddedDebugger.ViewModel
{
    public class SystemViewModel
    {
        /// <summary>
        /// The logger for this class
        /// </summary>
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

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
        public List<Connector> GetConnectors()
        {
            return this.modelManager.Connectors;
        }

        public void ConnectConnector(Connector connector)
        {
            this.debugProtocol.Connector = connector;
            // If connect was successful, save this configuration to be opened on next launch
            if (this.debugProtocol.Connect())
            {
                XmlSerializer writer = new XmlSerializer(typeof(Connector));

                string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "EmbeddedDebugger", "Config", "connection.xml");
                Directory.CreateDirectory(Path.GetDirectoryName(path));
                FileStream file = File.Create(path);

                writer.Serialize(file, this.debugProtocol.Connector);
                file.Close();
            }
        }

        public void DisconnectConnector(Connector connector)
        {
            this.debugProtocol.Disconnect();
        }

        public void ShowConnectorSettings(Connector connector)
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

        public Connector FindPreviousConnector()
        {
            Connector returnable = null;
            string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "EmbeddedDebugger", "Config", "connection.xml");
            if (File.Exists(path))
            {
                try
                {
                    XmlSerializer serializer =
                        new XmlSerializer(typeof(Connector));
                    using (Stream reader = new FileStream(path, FileMode.Open))
                    {
                        // Call the Deserialize method to restore the object's state.
                        returnable = (Connector)serializer.Deserialize(reader);
                    }
                }
                catch (Exception e)
                {
                    Logger.Error(e);
                }
            }
            return returnable;
        }
    }
}
