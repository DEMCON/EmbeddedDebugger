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
using EmbeddedDebugger.Connectors.BaseClasses;
using EmbeddedDebugger.Connectors.ProjectConnectors.Connectors;
using EmbeddedDebugger.Connectors.Settings;
using EmbeddedDebugger.DebugProtocol;
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
        private static readonly Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly ModelManager modelManager;
        private readonly ConnectionManager debugProtocol;

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
        public List<DebugConnection> GetConnectors()
        {
            return this.modelManager.Connectors;
        }

        public void ConnectConnector(DebugConnection connector)
        {
            this.debugProtocol.Connection = connector;
            // If connect was successful, save this configuration to be opened on next launch
            if (this.debugProtocol.Connect())
            {
                XmlSerializer writer = new XmlSerializer(typeof(DebugConnection), new[] { typeof(ExampleProjectConnector) });

                string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "EmbeddedDebugger", "Config", "connection.xml");
                Directory.CreateDirectory(Path.GetDirectoryName(path) ?? throw new InvalidOperationException());
                FileStream file = File.Create(path);

                writer.Serialize(file, this.debugProtocol.Connection);
                file.Close();
            }
        }

        public void DisconnectConnector(DebugConnection connector)
        {
            this.debugProtocol.Disconnect();
        }


        public void SetConnectorSettings(DebugConnection connector, List<ConnectionSetting> settings)
        {
            this.debugProtocol.SetConnectorSettings(connector, settings);
        }

        public bool ConnectorConnected()
        {
            // TODO: Add possibility to do this on connector level?
            return this.debugProtocol.IsConnected;
        }
        #endregion

        public IList<Register> GetRegisters()
        {
            return this.SelectedCpuNode == null
                ? this.GetCpuNodes().SelectMany(x => x.Registers).ToList()
                : this.SelectedCpuNode.Registers;
        }

        public void ResetTime(CpuNode cpuNode = null)
        {
            this.modelManager.ResetTime();
        }

        public void RequestNewValue(Register register)
        {
            this.modelManager.DebugProtocol.QueryRegister(register.CpuNode, register);
        }

        public void WriteNewValue(Register register)
        {
            this.modelManager.DebugProtocol.WriteToRegister(register.CpuNode, register, register.RegisterValue);
        }

        public void UpdateChannelMode(Register register, ChannelMode channelMode)
        {
            this.modelManager.DebugProtocol.SetupSignalTracing(register.CpuNode, channelMode, register);
        }

        public DebugConnection FindPreviousConnector()
        {
            DebugConnection returnable = null;
            string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "EmbeddedDebugger", "Config", "connection.xml");
            if (File.Exists(path))
            {
                try
                {
                    XmlSerializer serializer =
                        new XmlSerializer(typeof(DebugConnection));
                    using (Stream reader = new FileStream(path, FileMode.Open))
                    {
                        // Call the Deserialize method to restore the object's state.
                        returnable = (DebugConnection)serializer.Deserialize(reader);
                    }
                }
                catch (Exception e)
                {
                    Logger.Error(e);
                }
            }
            return returnable;
        }

        public void ReadOnceOfChannels()
        {
            this.modelManager.RequestOnce(SelectedCpuNode.Id);
        }
    }
}
