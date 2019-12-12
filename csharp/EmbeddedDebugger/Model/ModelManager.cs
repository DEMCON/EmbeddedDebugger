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
using EmbeddedDebugger.Model.Logging;
using EmbeddedDebugger.Model.RPC;
using EmbeddedDebugger.View.UserControls;
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
        private byte decimation;
        private RpcInterface rpcInterface;

        #region Properties
        private readonly List<CpuNode> nodes;
        public List<CpuNode> Nodes { get => nodes; }
        // By making this a list of objects, the view never has to have the assembly information of the connectors
        public List<object> Connectors { get => dp.Connectors.Cast<object>().ToList(); }
        public bool IsConnected { get => dp.IsConnected; }
        public object Connector { get => dp.Connector; }
        public byte Decimation { get => decimation; set => decimation = value; }
        public ValueLogger Logger { get => logger; }
        public RpcInterface RpcInterface { get => rpcInterface; }
        public DebugProtocol DebugProtocol { get => dp; }
        #endregion

        #region Eventhandlers
        public event EventHandler NewCPUNodeFound = delegate { };
        public event EventHandler HasConnected = delegate { };
        public event EventHandler HasDisconnected = delegate { };
        public event EventHandler ConfigCompletelyLoaded = delegate { };
        public event EventHandler<Register> RegisterQueried = delegate { };
        #endregion

        public ModelManager()
        {
            logger = new ValueLogger();
            dp = new DebugProtocol(this, logger);
            nodes = new List<CpuNode>();
            dp.HasConnected += CoreConnected;
            dp.HasDisconnected += CoreDisconnected;
            dp.RegisterQueried += Dp_RegisterQueried;
            dp.ConfigLoaded += Dp_ConfigLoaded;
            rpcInterface = new RpcInterface(this, dp);
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

        public void RequestOnce(object sender, int e)
        {
            dp.ReadChannelData((byte)e, 0x02);
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

        public void NewCPUFound()
        {
            NewCPUNodeFound(this, new EventArgs());
        }

        public void ConnectorChanged(object sender, EventArgs e)
        {
            if (sender is ConnectUserControl)
            {
                ConnectUserControl cuc = sender as ConnectUserControl;
                dp.NewConnector = cuc.SelectedConnector;
            }
        }

        public void ShowSettings(object sender, EventArgs e)
        {
            dp.ShowSettings();
        }

        public void NewDebugMessageToEmbedded(object sender, string e)
        {
            dp.SendDebugString(((CpuNode)sender).ID, e);
        }

        public void WriteRegisterValueUpdated(object sender, Register reg)
        {
            dp.WriteToRegister(0x01, reg.RegisterValue.ValueByteArray, reg);
        }

        // TODO: Add the correct CPU number!!!
        public void RefreshRegister(object sender, Register reg)
        {
            dp.QueryRegister(0x01, nodes.First(x => x.ID == 0x01), reg);
        }

        public void DebugChannelModeUpdated(object sender, Register reg)
        {
            dp.ConfigChannel(0x01, (byte)reg.DebugChannel, reg.ChannelMode);
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
