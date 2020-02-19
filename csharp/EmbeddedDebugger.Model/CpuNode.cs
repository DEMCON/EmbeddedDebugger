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
    public class CpuNode : INotifyPropertyChanged
    {
        #region Fields
        private readonly byte id;
        private int messageCounter = 0;
        private int invalidMessageCounter = 0;
        private readonly string serialNumber;
        private readonly string name;
        private readonly Version protocolVersion;
        private readonly Version applicationVersion;
        private EmbeddedConfig embeddedConfig;
        private readonly Dictionary<VariableType, int> sizes;
        private readonly Dictionary<int, Register> debugChannels;
        private int maxNumberOfDebugChannels = 16;
        private readonly List<TraceMessage> traceMessages;
        #endregion

        #region Properties
        [DisplayName("ID")]
        public byte ID { get => id; }
        public int MessageCounter
        {
            get => messageCounter;
            set
            {
                messageCounter = value;
                PropertyChanged(this, new PropertyChangedEventArgs("InvalidMessageCount"));
            }
        }
        public int InvalidMessageCounter
        {
            get => invalidMessageCounter; set
            {
                invalidMessageCounter = value;
                PropertyChanged(this, new PropertyChangedEventArgs("InvalidMessageCount"));
            }
        }
        [DisplayName("Serial number")]
        public string SerialNumber { get => serialNumber; }
        [DisplayName("Name")]
        public string Name { get => name; }
        [DisplayName("Protocol version")]
        public string ProtocolVersionString { get => $"V {protocolVersion.Major}.{protocolVersion.Minor}.{protocolVersion.Build}"; }
        public Version ProtocolVersion { get => protocolVersion; }
        [DisplayName("Application version")]
        public string ApplicationVersionString { get => $"V {applicationVersion.Major}.{applicationVersion.Minor}.{applicationVersion.Build}"; }
        public Version ApplicationVersion { get => applicationVersion; }
        [DisplayName("Invalid Message Count")]
        public string InvalidMessageCount { get => $"{invalidMessageCounter}/{messageCounter}"; }
        public IList<Register> Registers { get => EmbeddedConfig.Registers; }
        public EmbeddedConfig EmbeddedConfig { get => embeddedConfig ?? new EmbeddedConfig(); set => embeddedConfig = value; }
        public string NewTerminalData
        {
            set
            {
                NewTerminalDataAdded(this, value);
            }
        }
        public TraceMessage TraceMessage
        {
            get => traceMessages.Last() ?? new TraceMessage(new byte[1]);
            set
            {
                lock (traceMessages)
                {
                    traceMessages.Add(value);
                }
                NewTraceMessageAdded(this, value);
            }
        }
        public List<TraceMessage> TraceMessages
        {
            get => traceMessages;
        }
        public Dictionary<VariableType, int> Sizes { get => sizes; }
        public Dictionary<int, Register> DebugChannels { get => debugChannels; }
        public int MaxNumberOfDebugChannels { get => maxNumberOfDebugChannels; set => maxNumberOfDebugChannels = value; }
        public List<Register> DebugChannelRegisters { get => debugChannels.Values.ToList(); }
        #endregion

        #region EventHandlers
        public event EventHandler NewReadRegisterAdded = delegate { };
        public event EventHandler NewWriteRegisterAdded = delegate { };
        public delegate void NewTerminalDataAddedDelegate(CpuNode node, string s);
        public event NewTerminalDataAddedDelegate NewTerminalDataAdded = delegate { };
        public event EventHandler<Register> NewDebugChannel = delegate { };
        public event EventHandler<Register> RemoveDebugChannel = delegate { };
        public event EventHandler<Register> UpdateDebugChannel = delegate { };
        public event EventHandler<Register> RegisterQueriesValue = delegate { };
        public event EventHandler<Register> RegisterLoggingChanged = delegate { };
        public event EventHandler<object[]> RegisterValueChanged = delegate { };
        public event PropertyChangedEventHandler PropertyChanged = delegate { };
        public event EventHandler<TraceMessage> NewTraceMessageAdded = delegate { };
        #endregion

        public CpuNode(byte id, Version protocolVersion, Version applicationVersion, string name, string serialNumber)
        {
            this.id = id;
            this.name = name;
            this.protocolVersion = protocolVersion;
            this.applicationVersion = applicationVersion;
            this.serialNumber = serialNumber;
            traceMessages = new List<TraceMessage>();
            debugChannels = new Dictionary<int, Register>();
            sizes = new Dictionary<VariableType, int>
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

        #region Add registers
        /// <summary>
        /// This method is used whenever an additional Register is to be added to the list
        /// Should not be used, since it doesn't really add value.
        /// To be destroyed
        /// </summary>
        /// <param name="register">The register to be added</param>
        public void AddReadRegister(Register register)
        {
            if (register.ReadWrite != ReadWrite.Read)
            {
                throw new ArgumentException("You cannot add a none read regiser to this list!");
            }
            AddRegister(register, ReadWrite.Read);
        }

        /// <summary>
        /// This method is used whenever an additional Register is to be added to the list
        /// Should not be used, since it doesn't really add value.
        /// To be destroyed
        /// </summary>
        /// <param name="register">The register to be added</param>
        public void AddWriteRegister(Register register)
        {
            if (register.ReadWrite != ReadWrite.Write)
            {
                throw new ArgumentException("You cannot add a none write regiser to this list!");
            }
            AddRegister(register, ReadWrite.Write);
        }

        /// <summary>
        /// Add a register to the correct list, depending on the value of readWrite
        /// </summary>
        /// <param name="register">The register to add to the list</param>
        /// <param name="readWrite">Determine which list to add to</param>
        public void AddRegister(Register register, ReadWrite readWrite)
        {
            register.TimeStampUnits = sizes[VariableType.TimeStamp];
        }
        #endregion



        /// <summary>
        /// This method goes through the available debugchannels and registers the register as debugchannel on the first available one
        /// If none can be found, the register is not set as debugchannel
        /// </summary>
        /// <param name="r">The register to set as debugchannel</param>
        /// <returns>If the register is set as debugchannel</returns>
        public bool SetRegisterAsDebugChannel(Register r)
        {
            if (debugChannels.Any(x => x.Value == r)) return false;
            for (int i = 0; i < maxNumberOfDebugChannels; i++)
            {
                if (!debugChannels.ContainsKey(i))
                {
                    r.DebugChannel = (byte)i;
                    if (r.ChannelMode == ChannelMode.Off)
                    {
                        r.ChannelMode = ChannelMode.LowSpeed;
                    }
                    debugChannels.Add(i, r);
                    NewDebugChannel(this, r);
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// This method is used to remove a register as a debugchannel
        /// </summary>
        /// <param name="r">The register to remove</param>
        public void RemoveRegisterAsDebugChannel(Register r)
        {
            if (!debugChannels.Any(x => x.Value == r)) return;
            debugChannels.Remove(debugChannels.First(x => x.Value == r).Key);
            RemoveDebugChannel(this, r);
            r.DebugChannel = null;
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
            IList<Register> registers = ec.Registers;
            ec.SetRegisters(new List<Register>());
            embeddedConfig = ec;
            foreach (Register r in registers)
            {
                if (r.HasChildren)
                {
                    foreach (Register c in r.ChildRegisters)
                    {
                        AddRegister(c, c.ReadWrite);
                    };
                }
                else
                {
                    AddRegister(r, r.ReadWrite);
                }
                embeddedConfig.AddRegister(r);
            }
            //SetupChannels();
            NewReadRegisterAdded(this, new EventArgs());
            NewWriteRegisterAdded(this, new EventArgs());
            return true;
        }

        /// <summary>
        /// Set up as many channels as possible
        /// </summary>
        public void SetupChannels()
        {
            foreach (Register r in embeddedConfig.Registers.Where(x => x.Show == true && x.IsReadable && !x.IsVariableSize))
            {
                SetRegisterAsDebugChannel(r);
            }
        }

        /// <summary>
        /// Define how this object should be displayed as
        /// </summary>
        /// <returns>A string containing this objects information</returns>
        public override string ToString()
        {
            return $"{name} (Serialnumber: {serialNumber}) [CPUID: {id}]";
        }

        #region RegisterEvents
        /// <summary>
        /// This method is used as a gatway to let the ValueLogger know that this register is to be added
        /// </summary>
        /// <param name="sender">The Register</param>
        /// <param name="e">Empty eventargs</param>
        private void Register_LoggingChanged(object sender, EventArgs e)
        {
            RegisterLoggingChanged(this, (Register)sender);
        }

        /// <summary>
        /// This method is used as a gateway to the debugprotocol to make sure a changed value is actually send to the platform
        /// </summary>
        /// <param name="sender">The register</param>
        /// <param name="e">The registervalue</param>
        private void Register_ValueChanged(object sender, RegisterValue e)
        {
            RegisterValueChanged(this, new object[] { sender, e });
        }

        /// <summary>
        /// This method is used to setup/change/remove a register as debugchannel
        /// Thereby making sure that the ChannelMode.Off really means that it is removed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Register_ChannelModeUpdated(object sender, EventArgs e)
        {
            Register reg = (Register)sender;
            ChannelMode cm = reg.ChannelMode;
            if (!reg.DebugChannel.HasValue && !SetRegisterAsDebugChannel(reg))
            {
                throw new ArgumentException("Out of debug channels.");
            }
            if (reg.ChannelMode == ChannelMode.Off)
            {
                RemoveRegisterAsDebugChannel(reg);
            }
            else
            {
                UpdateDebugChannel(this, reg);
            }
        }

        /// <summary>
        /// This method is used as a gateway for a register to query its value
        /// </summary>
        /// <param name="sender">The register</param>
        /// <param name="e">Empty eventargs</param>
        private void CpuNode_RegisterQueriesValue(object sender, EventArgs e)
        {
            RegisterQueriesValue(this, (Register)sender);
        }
        #endregion
    }
}
