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
using EmbeddedDebugger.DebugProtocol.Enums;
using EmbeddedDebugger.DebugProtocol.RegisterValues;
using EmbeddedDebugger.Model.EmbeddedConfiguration;
using EmbeddedDebugger.Model.Messages;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace EmbeddedDebugger.Model
{
    /// <summary>
    /// This class represents a CPU node with which we can communicate using the connector
    /// </summary>
    public class CpuNode
    {
        #region Properties
        public byte Id { get; }
        // Is used in XAML!
        public string InvalidMessageCount => $"{this.InvalidMessageCounter}/{this.MessageCounter}";
        public int MessageCounter { get; set; } = 0;
        public int InvalidMessageCounter { get; set; } = 0;
        public string SerialNumber { get; }
        public string Name { get; }
        public string ProtocolVersionString => $"V {this.ProtocolVersion.Major}.{this.ProtocolVersion.Minor}.{this.ProtocolVersion.Build}";
        public Version ProtocolVersion { get; }
        public string ApplicationVersionString => $"V {this.ApplicationVersion.Major}.{this.ApplicationVersion.Minor}.{this.ApplicationVersion.Build}";
        public Version ApplicationVersion { get; }

        public IList<Register> Registers => this.EmbeddedConfig.Registers;
        public EmbeddedConfig EmbeddedConfig { get; set; }
        public List<TraceMessage> TraceMessages { get; }
        public Dictionary<VariableType, int> Sizes { get; }
        public Dictionary<int, Register> DebugChannels { get; }
        public int MaxNumberOfDebugChannels { get; set; } = 16;
        public List<Register> DebugChannelRegisters => this.DebugChannels.Values.ToList();
        #endregion



        public CpuNode(byte id, Version protocolVersion, Version applicationVersion, string name, string serialNumber)
        {
            this.Id = id;
            this.Name = name;
            this.ProtocolVersion = protocolVersion;
            this.ApplicationVersion = applicationVersion;
            this.SerialNumber = serialNumber;
            this.TraceMessages = new List<TraceMessage>();
            this.DebugChannels = new Dictionary<int, Register>();
            this.Sizes = new Dictionary<VariableType, int>
            {
                { VariableType.MemoryAlignment, 1 },
                { VariableType.Pointer, 4 },
                { VariableType.Char, 1 },
                { VariableType.Short, 2 },
                { VariableType.Int, 4 },
                { VariableType.Long, 8 },
                { VariableType.Float, 4 },
                { VariableType.Double, 8 },
                { VariableType.LongDouble, 8 },
                { VariableType.SChar, 1 },
                { VariableType.UChar, 1 },
                { VariableType.UShort, 2 },
                { VariableType.UInt, 4 },
                { VariableType.ULong, 8 },
                { VariableType.String, 0 },
                { VariableType.Blob, 0 },
                { VariableType.TimeStamp, 1000 }
            };
        }


        /// <summary>
        /// This method is used to get the embedded configuration for this cpunode
        /// </summary>
        /// <param name="path">The path where the .xml file is supposed to be</param>
        /// <returns>If the configuration is loaded</returns>
        public bool TryToLoadConfiguration(string path)
        {
            XmlConfigurationParser parser = new XmlConfigurationParser();
            EmbeddedConfig ec;
            try
            {
                ec = parser.FromFile(path, this);
            }
            catch (Exception e)
            {
#if DEBUG
                Console.WriteLine(e);
#endif
                return false;
            }
            this.EmbeddedConfig = ec;
            return true;
        }

        /// <summary>
        /// Define how this object should be displayed as
        /// </summary>
        /// <returns>A string containing this objects information</returns>
        public override string ToString()
        {
            return $"{this.Name} (Serial number: {this.SerialNumber}) [Cpu id: {this.Id}]";
        }
    }
}
