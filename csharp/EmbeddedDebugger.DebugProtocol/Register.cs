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
using System;
using System.Collections.Generic;
using System.Linq;

namespace EmbeddedDebugger.DebugProtocol
{
    public class Register
    {
        #region Fields
        private string name;
        private string fullName;
        private string variableTypeName;
        #endregion

        #region Properties
        public RegisterValue RegisterValue { get; set; }
        public List<RegisterValue> MyValues { get; }
        public string Value
        {
            get
            {
                string s = RegisterValue != null ? RegisterValue.ValueAsFormattedString(this.ValueDisplayFormat) : "";
                if (!EnableValueUpdates)
                    s = s.Trim();
                return s;
            }
            set
            {
                RegisterValue regValue = RegisterValue.GetRegisterValueByVariableType(VariableType);
                if (regValue.ValueByteArrayFromString(value, out byte[] output, this.ValueDisplayFormat))
                {
                    regValue.ValueByteArray = output;
                    RegisterValue = regValue;
                }
            }
        }
        public List<Register> ChildRegisters { get; set; }
        public bool HasChildren => this.ChildRegisters != null && this.ChildRegisters.Count > 0;
        public Register Parent { get; set; }
        public uint Id { get; set; }
        public string Name { get => this.name ?? this.fullName; set => this.name = value; }
        public string FullName { get => this.fullName ?? this.name; set => this.fullName = value; }
        public ReadWrite ReadWrite { get; set; }
        public bool EnableValueUpdates { get; set; } = true;
        public VariableType VariableType { get; set; }

        public string VariableTypeName
        {
            get => this.VariableType == VariableType.Unknown ? this.variableTypeName : this.VariableType.ToString().ToLower();
            set => this.variableTypeName = value;
        }
        public string VariableTypeString => this.VariableType == VariableType.Unknown ? $"\"{this.variableTypeName}\"" : this.VariableType.ToString().ToLower();
        public bool Plot { get; set; }

        public int MaxNumberOfValues { get; set; }
        public double NumberOfSeconds { get; set; }
        public int Size { get; set; }
        public uint Offset { get; set; }
        public int DerefDepth { get; set; }
        public bool? Show { get; set; }
        public Source Source { get; set; }
        public int TimeStampUnits { get; set; } = 1;
        public bool IsDebugChannel => DebugChannel.HasValue;
        public byte? DebugChannel { get; set; }
        public ChannelMode ChannelMode { get; set; }
        public CpuNode CpuNode { get; set; }
        public byte CpuId => this.CpuNode.Id;
        public bool IsReadable => ((byte)this.ReadWrite & 0b0000_0010) >> 1 == 1;
        public bool IsWritable => ((byte)this.ReadWrite & 0b0000_0001) == 1;
        public ValueDisplayFormat ValueDisplayFormat { get; set; }

        public bool IsIntegralValue
        {
            get
            {
                switch (this.VariableType)
                {
                    case VariableType.Bool:
                    case VariableType.Char:
                    case VariableType.Short:
                    case VariableType.Int:
                    case VariableType.Long:
                    case VariableType.Float:
                    case VariableType.Double:
                    case VariableType.LongDouble:
                    case VariableType.SChar:
                    case VariableType.UChar:
                    case VariableType.UShort:
                    case VariableType.UInt:
                    case VariableType.ULong:
                        return true;
                    default:
                        return false;
                }
            }
        }

        public bool IsVariableSize
        {
            get
            {
                switch (this.VariableType)
                {
                    case VariableType.String:
                    case VariableType.Blob:
                        return true;
                }
                return false;
            }
        }

        #endregion

        #region Constructors
        public Register()
        {
            this.ChildRegisters = new List<Register>();
            this.MyValues = new List<RegisterValue>();
        }
        #endregion

        /// <summary>
        /// Add a value to the register, also adds it to the plotmodel, this is used for plotting
        /// TODO: check if there is a more elegant way.
        /// </summary>
        /// <param name="regValue">The value which is to be added to the Register</param>
        public void AddValue(RegisterValue regValue)
        {
            if (regValue != null)
            {
                Console.WriteLine($"Register {this.name} got value {regValue.Value}");
                regValue = RegisterValue.GetRegisterValueByVariableType(this.VariableType, regValue.ValueByteArray, regValue.TimeStamp);
                this.RegisterValue = regValue;
                if (this.Plot && regValue.TimeStamp != null)
                {
                    this.MyValues.Add(regValue);
                }
            }
        }


        public string GetCPPVariableTypes(VariableType varType)
        {
            switch (varType)
            {
                case VariableType.Bool:
                    return "boolean";
                case VariableType.SChar:
                    return "int8_t";
                case VariableType.Short:
                    return "int16_t";
                case VariableType.Int:
                    return "int32_t";
                case VariableType.Long:
                    return "int64_t";
                case VariableType.UChar:
                    return "uint8_t";
                case VariableType.UShort:
                    return "uint16_t";
                case VariableType.UInt:
                    return "uint32_t";
                case VariableType.ULong:
                    return "uint64_t";
            }
            return varType.ToString().ToLower();
        }

        /// <summary>
        /// This method copies all values of the old register to a new and standalone register
        /// </summary>
        /// <param name="old">The Register to be copied/cloned</param>
        /// <returns>The new Register</returns>
        public static Register GetCopy(Register old)
        {
            return new Register()
            {
                ChildRegisters = old.ChildRegisters.ToList(),
                Parent = old.Parent == null ? null : GetCopy(old.Parent),
                Id = Convert.ToUInt32(old.Id),
                Name = old.Name.ToString(),
                FullName = old.FullName.ToString(),
                ReadWrite = (ReadWrite)Enum.Parse(typeof(ReadWrite), old.ReadWrite.ToString()),
                VariableType = (VariableType)Enum.Parse(typeof(VariableType), old.VariableType.ToString()),
                VariableTypeName = old.VariableTypeName.ToString(),
                MaxNumberOfValues = Convert.ToInt32(old.MaxNumberOfValues),
                NumberOfSeconds = Convert.ToDouble(old.NumberOfSeconds),
                Size = Convert.ToInt32(old.Size),
                Offset = Convert.ToUInt32(old.Offset),
                DerefDepth = Convert.ToInt32(old.DerefDepth),
                Show = Convert.ToBoolean(old.Show),
                Source = (Source)Enum.Parse(typeof(Source), old.Source.ToString()),
                TimeStampUnits = Convert.ToInt32(old.TimeStampUnits),
                DebugChannel = null,
                CpuNode = old.CpuNode,
            };
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Register)) return false;
            Register reg = (Register)obj;
            return
                reg.Id == this.Id &&
                reg.FullName == this.FullName &&
                reg.ReadWrite == this.ReadWrite &&
                reg.VariableType == this.VariableType &&
                reg.Size == this.Size &&
                reg.Offset == this.Offset &&
                reg.DerefDepth == this.DerefDepth &&
                reg.Source == this.Source &&
                reg.CpuNode == this.CpuNode;
        }

        public override int GetHashCode()
        {
            int hashCode = 1671609247;
            hashCode = hashCode * -1521134295 + EqualityComparer<List<Register>>.Default.GetHashCode(ChildRegisters);
            hashCode = hashCode * -1521134295 + EqualityComparer<Register>.Default.GetHashCode(Parent);
            hashCode = hashCode * -1521134295 + this.Id.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(name);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(fullName);
            hashCode = hashCode * -1521134295 + this.ReadWrite.GetHashCode();
            hashCode = hashCode * -1521134295 + VariableType.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(variableTypeName);
            hashCode = hashCode * -1521134295 + Plot.GetHashCode();
            hashCode = hashCode * -1521134295 + MaxNumberOfValues.GetHashCode();
            hashCode = hashCode * -1521134295 + NumberOfSeconds.GetHashCode();
            hashCode = hashCode * -1521134295 + Size.GetHashCode();
            hashCode = hashCode * -1521134295 + Offset.GetHashCode();
            hashCode = hashCode * -1521134295 + DerefDepth.GetHashCode();
            hashCode = hashCode * -1521134295 + Show.GetHashCode();
            hashCode = hashCode * -1521134295 + Source.GetHashCode();
            hashCode = hashCode * -1521134295 + TimeStampUnits.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<byte?>.Default.GetHashCode(DebugChannel);
            hashCode = hashCode * -1521134295 + this.ChannelMode.GetHashCode();
            return hashCode;
        }
    }
}
