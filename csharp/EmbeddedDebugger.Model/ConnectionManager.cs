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
using EmbeddedDebugger.Connectors.ProjectConnectors.Interfaces;
using EmbeddedDebugger.Connectors.Settings;
using EmbeddedDebugger.DebugProtocol;
using EmbeddedDebugger.DebugProtocol.Enums;
using EmbeddedDebugger.DebugProtocol.Messages;
using EmbeddedDebugger.DebugProtocol.RegisterValues;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EmbeddedDebugger.Model
{
    /// <summary>
    /// This class is used to communicate properly with an embedded system.
    /// Ensuring messages are correctly encoded and decoded 
    /// </summary>
    public class ConnectionManager
    {
        private readonly ModelManager core;

        #region Properties
        public DebugConnection Connection { get; set; }
        public List<DebugConnection> Connections { get; }
        // TODO DESTROY!!!
        public bool IsConnected { get; private set; }
        #endregion

        public ConnectionManager(ModelManager core)
        {
            this.Connections = GetConnectorTypes().ToList();
            this.core = core;
        }

        #region Connector manipulation
        /// <summary>
        /// Method used to connect a connector to the embedded system and setting up the correct events
        /// </summary>
        /// <returns>If the connect worked or not</returns>
        public bool Connect()
        {
            // Check if the connector is connected
            if (this.Connection.IsConnected) return true;
            this.Connection.HasConnected += this.ConnectorConnected;
            this.Connection.UnexpectedlyDisconnected += this.ConnectorDisconnected;
            // Try to connect
            bool returnable = this.Connection.Connect();
            if (!returnable)
            {
                // If connection failed, remove the event handlers
                this.Connection.HasConnected -= this.ConnectorConnected;
                this.Connection.UnexpectedlyDisconnected -= this.ConnectorDisconnected;
            }
            return returnable;
        }

        /// <summary>
        /// Method used to disconnect the connector from the embedded system
        /// </summary>
        public void Disconnect()
        {
            this.Connection.Disconnect();
            this.IsConnected = false;
        }

        /// <summary>
        /// Method used to set the setting for a connection
        /// </summary>
        /// <param name="connection">The connection for which the settings are set</param>
        /// <param name="settings">The settings for the connection</param>
        public void SetConnectorSettings(DebugConnection connection, List<ConnectionSetting> settings)
        {
            connection.SetConnectionSettings(settings);
        }

        #endregion


        #region Send messages
        /// <summary>
        /// Ask all nodes if there is anyone there
        /// </summary>
        public void SearchForNodes(DebugConnection connection)
        {
            // TODO NEEDS REMOVAL
            connection.Nodes = this.core.Nodes;
            connection.SearchForNodes();
        }

        /// <summary>
        /// Write a value to a Register
        /// </summary>
        /// <param name="cpuNode">The CPU node ID</param>
        /// <param name="register">The value to write to the register</param>
        /// <param name="registerValue">The register to write to</param>
        public void WriteToRegister(CpuNode cpuNode, Register register, RegisterValue registerValue)
        {
            this.Connection.WriteValue(cpuNode, register, registerValue);
        }

        /// <summary>
        /// This method gets the value for a register once
        /// </summary>
        /// <param name="nodeID"></param>
        /// <param name="node"></param>
        /// <param name="r"></param>
        public void QueryRegister(CpuNode node, Register r)
        {
            this.Connection.QueryValue(node, r);
        }


        /// <summary>
        /// This method is used to setup a channel
        /// </summary>
        /// <param name="node">The cpu node</param>
        /// <param name="mode">The mode</param>
        /// <param name="register">The register the channel is bound to</param>
        public void SetupSignalTracing(CpuNode node, ChannelMode mode, Register register)
        {
            this.Connection.SetupSignalTracing(node, register, mode);
        }

        /// <summary>
        /// This method is used to reset the time on a CPU node, can be used on all if no nodeID is given
        /// </summary>
        /// <param name="node">The node to write to</param>
        public void ResetTime(CpuNode node = null)
        {
            if (node == null)
            {
                // Reset time for all nodes
                this.core.Nodes.ForEach(x => this.Connection.ResetTime(x));
            }
            else
            {
                this.Connection.ResetTime(node);
            }
        }

        /// <summary>
        /// This method is used to configure all channels for a CPUnode
        /// </summary>
        /// <param name="nodeID">The ID of the node</param>
        /// <param name="trace">The mode</param>
        public void ReadChannelData(byte nodeID, byte trace)
        {
            //SendMessage(new ProtocolMessage(nodeID, GetMsgID(nodeID), Command.ReadChannelData, new byte[] { trace }));
        }

        /// <summary>
        /// This method is used to send a debug string (terminal like) to a CPU node
        /// </summary>
        /// <param name="nodeID">The node to write the debugstring to</param>
        /// <param name="debugString">The debug string</param>
        public void SendDebugString(byte nodeID, string debugString)
        {
            DebugStringMessage debugStringMessage = new DebugStringMessage()
            {
                Message = debugString
            };
            //SendMessage(nodeID, debugStringMessage);
        }
        #endregion


        #region EventHandlers
        /// <summary>
        /// When the connector actually connects (servers might take a while to have a client connect to it) tell the world and search for CPU nodes
        /// </summary>
        /// <param name="sender">The sender</param>
        /// <param name="e">Possible EventArgs</param>
        public void ConnectorConnected(object sender, EventArgs e)
        {
            this.IsConnected = true;
            // Add event handlers for when new nodes, values, etc. are found
            this.Connection.NewNodeFound += this.Connector_NewNodeFound;
            this.Connection.NewValueReceived += this.Connector_NewValueReceived;
            this.Connection.NewTraceMessageReceived += this.Connector_NewTraceMessageReceived;

            //  Search for nodes on this connector
            this.SearchForNodes(this.Connection);

            // Remove this event handler
            this.Connection.HasConnected -= this.ConnectorConnected;
        }

        /// <summary>
        /// When the connector actually disconnects tell the world
        /// </summary>
        /// <param name="sender">The sender</param>
        /// <param name="e">Possible EventArgs</param>
        private void ConnectorDisconnected(object sender, EventArgs e)
        {
            this.IsConnected = false;
            this.Connection.UnexpectedlyDisconnected -= this.ConnectorDisconnected;
        }

        /// <summary>
        /// Method called whenever a connector discovers a new node
        /// </summary>
        /// <param name="sender">The connector</param>
        /// <param name="e">The new Node</param>
        private void Connector_NewNodeFound(object sender, CpuNode e)
        {
            // Add the node to list
            // TODO, when adding multi connector support, make sure to create mapping for these
            this.core.Nodes.Add(e);
        }

        /// <summary>
        /// This method is called whenever a connector has received a new value
        /// </summary>
        /// <param name="sender">The connector</param>
        /// <param name="e">The new value</param>
        private void Connector_NewValueReceived(object sender, EmbeddedDebugger.DebugProtocol.CustomEventArgs.ValueReceivedEventArgs e)
        {
            e.Register?.AddValue(e.Value);
        }

        /// <summary>
        /// Method called whenever a new trace message is received
        /// </summary>
        /// <param name="sender">The connector</param>
        /// <param name="e"></param>
        private void Connector_NewTraceMessageReceived(object sender, DebugProtocol.CustomEventArgs.TraceMessageReceivedEventArgs e)
        {
            e.CpuNode.TraceMessages.Add(e.Message);
        }
        #endregion

        /// <summary>
        /// This method gathers all classes extending either the IConnector or the IProjectConnector.
        /// </summary>
        /// <returns>The list of all connectors</returns>
        private static IEnumerable<DebugConnection> GetConnectorTypes()
        {
            foreach (Type typeString in AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(p => typeof(IProjectConnector).IsAssignableFrom(p) && !p.IsInterface).ToList())
            {
                yield return (DebugConnection)Activator.CreateInstance(typeString);
            }
            foreach (Type typeString in AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(p => typeof(DebugConnection).IsAssignableFrom(p) && !p.IsInterface && !p.IsAbstract).ToList())
            {
                yield return (DebugConnection)Activator.CreateInstance(typeString);
            }
        }
    }
}
