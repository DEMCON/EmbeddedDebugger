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
using EmbeddedDebugger.Connectors.Interfaces;
using EmbeddedDebugger.Model.Logging;
using EmbeddedDebugger.Model.RPC;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EmbeddedDebugger.Model
{
    /// <summary>
    /// As the name of the class explains this is the core of the program
    /// Everything is to be decided by this class
    /// </summary>
    public class ModelManager
    {
        private readonly DebugProtocol dp;
        private readonly ValueLogger logger;
        private RpcInterface rpcInterface;

        #region Properties
        private readonly List<CpuNode> nodes;
        public List<CpuNode> Nodes => nodes;
        // By making this a list of objects, the view never has to have the assembly information of the connectors
        public List<Connector> Connectors => dp.Connectors;
        public bool IsConnected => dp.IsConnected;
        public object Connector => dp.Connector;
        public byte Decimation { get; set; }
        public ValueLogger Logger { get; private set; }
        public RpcInterface RpcInterface { get; private set; }
        public DebugProtocol DebugProtocol => dp;
        public Dictionary<byte, Register> DebugChannels;
        #endregion

        #region Eventhandlers
        public event EventHandler HasConnected = delegate { };
        public event EventHandler HasDisconnected = delegate { };
        public event EventHandler ConfigCompletelyLoaded = delegate { };
        public event EventHandler<Register> RegisterQueried = delegate { };
        #endregion

        public ModelManager()
        {
            this.logger = new ValueLogger();
            this.dp = new DebugProtocol(this, logger);
            this.nodes = new List<CpuNode>();
            this.dp.HasConnected += CoreConnected;
            this.dp.HasDisconnected += CoreDisconnected;
            this.dp.RegisterQueried += Dp_RegisterQueried;
            this.dp.ConfigLoaded += Dp_ConfigLoaded;
            this.rpcInterface = new RpcInterface(this, dp);
            this.DebugChannels = new Dictionary<byte, Register>();
        }

        private void Dp_ConfigLoaded(object sender, EventArgs e)
        {
            ConfigCompletelyLoaded(this, new EventArgs());
        }

        public void RpcChanged(bool enabled)
        {
            if (enabled)
            {
                rpcInterface.Start();
            }
            else
            {
                rpcInterface.Stop();
            }
        }

        public void RequestOnce(byte nodeId)
        {
            dp.ReadChannelData(nodeId, 0x02);
        }

        private void Dp_RegisterQueried(object sender, Register reg)
        {
            RegisterQueried(sender, reg);
        }

        public void CoreConnected(object sender, EventArgs e)
        {
            nodes.Clear();
            HasConnected(sender, e);
        }

        public void CoreDisconnected(object sender, EventArgs e)
        {
            HasDisconnected(sender, e);
        }

        /// <summary>
        /// Request the current connector to connect to the embedded system
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">Arguments</param>
        public void ConnectRequest(object sender, EventArgs e)
        {
            dp.Connect();
        }

        /// <summary>
        /// Request the current connector to disconnect to the embedded system
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">Arguments</param>
        public void DisconnectRequest(object sender, EventArgs e)
        {
            if (dp.Connector != null)
            {
                dp.Disconnect();
            }
        }

        public void NewDebugMessageToEmbedded(object sender, string e)
        {
            dp.SendDebugString(((CpuNode)sender).ID, e);
        }

        public void ResetTime(int decimation_ms = 0)
        {
            if (decimation_ms > 0)
            {
                foreach (CpuNode n in Nodes)
                {
                    int decimation = decimation_ms * 1000 / n.Sizes[EmbeddedDebugger.DebugProtocol.Enums.VariableType.TimeStamp];
                    if (decimation < 1)
                        decimation = 1;
                    if (decimation > 255)
                        decimation = 255;
                    dp.Decimation(n.ID, (byte)decimation);
                }
            }

            dp.ResetTime();
        }
    }
}
