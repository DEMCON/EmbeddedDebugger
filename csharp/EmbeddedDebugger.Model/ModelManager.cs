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
using System.Collections.Generic;
using EmbeddedDebugger.Connectors.BaseClasses;
using EmbeddedDebugger.DebugProtocol;

namespace EmbeddedDebugger.Model
{
    /// <summary>
    /// As the name of the class explains this is the core of the program
    /// Everything is to be decided by this class
    /// </summary>
    public class ModelManager
    {
        #region Properties
        public List<CpuNode> Nodes { get; }
        public List<DebugConnection> Connectors => this.DebugProtocol.Connections;
        public byte Decimation { get; set; }
        public ValueLogger Logger { get; }
        public RpcInterface RpcInterface { get; }
        public ConnectionManager DebugProtocol { get; }
        public Dictionary<byte, Register> DebugChannels { get; }
        #endregion


        public ModelManager()
        {
            this.Logger = new ValueLogger();
            this.DebugProtocol = new ConnectionManager(this);
            this.Nodes = new List<CpuNode>();
            this.RpcInterface = new RpcInterface(this, this.DebugProtocol);
            this.DebugChannels = new Dictionary<byte, Register>();
        }

        public void RequestOnce(byte nodeId)
        {
            this.DebugProtocol.ReadChannelData(nodeId, 0x02);
        }

        public void NewDebugMessageToEmbedded(object sender, string e)
        {
            this.DebugProtocol.SendDebugString(((CpuNode)sender).Id, e);
        }

        public void ResetTime(int decimationMs = 0)
        {
            this.DebugProtocol.ResetTime();
        }
    }
}
