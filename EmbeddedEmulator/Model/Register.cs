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
using EmbeddedDebugger.DebugProtocol;
using EmbeddedDebugger.DebugProtocol.Enums;
using EmbeddedDebugger.DebugProtocol.RegisterValues;
using System;
using System.Collections.Generic;
using System.Text;

namespace EmbeddedEmulator.Model
{
    public class Register
    {
        #region Fields
        private RegisterValue value;
        private uint id;
        private string name;
        private string fullName;
        private ReadWrite readWrite;
        private VariableType variableType;
        private int size;
        private uint offset;
        private int derefDepth;
        private Source source;
        private int timeStampUnits = 1;
        private byte? debugChannel;
        private ChannelMode channelMode;
        #endregion

        #region Properties
        public RegisterValue Value { get => value; set => this.value = value; }
        public uint ID { get => id; set => id = value; }
        public string Name { get => name ?? fullName; set => name = value; }
        public string FullName { get => fullName ?? name; set => fullName = value; }
        public ReadWrite ReadWrite { get => readWrite; set => readWrite = value; }
        public VariableType VariableType { get => variableType; set => variableType = value; }
        public int Size { get => size; set => size = value; }
        public uint Offset { get => offset; set => offset = value; }
        public int DerefDepth { get => derefDepth; set => derefDepth = value; }
        public Source Source { get => source; set => source = value; }
        public int TimeStampUnits { get => timeStampUnits; set => timeStampUnits = value; }
        public byte? DebugChannel { get => debugChannel; set => debugChannel = value; }
        public bool IsDebugChannel { get => debugChannel.HasValue; }
        public ChannelMode ChannelMode { get => channelMode; set => channelMode = value; }
        #endregion

        #region Constructors
        public Register(uint id, string name, ReadWrite readWrite, VariableType variableType, Source source, int derefDepth, uint offset, int size)
        {
            this.id = id;
            this.name = name;
            this.readWrite = readWrite;
            this.variableType = variableType;
            this.source = source;
            this.derefDepth = derefDepth;
            this.offset = offset;
            this.size = size;
        }
        #endregion

        public byte[] GetBytes(Version protocolVersion)
        {
            List<byte> data = new List<byte>();
            data.AddRange(BitConverter.GetBytes(id));
            data.Add((byte)name.Length);
            data.AddRange(Encoding.ASCII.GetBytes(name));
            data.AddRange(BitConverter.GetBytes(offset));
            data.Add((byte)size);
            data.Add((byte)variableType);
            data.Add(MessageCodec.GetControlByte(protocolVersion, readWrite, Source, derefDepth));
            return data.ToArray();
        }
    }
}
