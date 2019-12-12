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
using EmbeddedDebugger.Connectors.CustomEventArgs;
using EmbeddedDebugger.Connectors.Interfaces;
using EmbeddedDebugger.Connectors.ProjectConnectors.Interfaces;
using EmbeddedDebugger.DebugProtocol;
using EmbeddedDebugger.DebugProtocol.Enums;
using EmbeddedDebugger.DebugProtocol.Messages;
using EmbeddedDebugger.DebugProtocol.RegisterValues;
using EmbeddedDebugger.Model.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Timers;

namespace EmbeddedDebugger.Model
{
    /// <summary>
    /// This class is used to communicate properly with an embedded system.
    /// Ensuring messages are correctly endoded and decoded 
    /// </summary>
    public class DebugProtocol
    {
        public readonly static Version CurrentVersion = new Version(0, 0, 2);
        private const int AckTimoutTicks = 20;
        private const byte REC_SEP = 0x33;

        private readonly ConcurrentQueue<byte[]> incomingBytes;
        private readonly Thread myThread;
        private readonly ModelManager core;
        private readonly Dictionary<byte, byte> msgIDs;
        private readonly List<ProtocolMessage> messagesWaitingAck;
        private readonly System.Timers.Timer resendsTimer;
        private readonly ValueLogger logger;

        #region Properties
        private IConnector connector;
        public IConnector Connector { get => connector; set => connector = value; }

        public object NewConnector { get => connector; set => connector = value as IConnector; }

        // This list could be dynamic, but since the assembly will never(!) change during runtime it is useless to reassemble this list
        private readonly List<IConnector> connectors;
        public List<IConnector> Connectors { get => connectors; }

        private bool isConnected;
        public bool IsConnected { get => isConnected; }
        #endregion

        #region EventHandlers
        public event EventHandler HasConnected = delegate { };
        public event EventHandler HasDisconnected = delegate { };
        public event EventHandler<Register> RegisterQueried = delegate { };
        public event EventHandler<long> NewReadChannelData = delegate { };
        public event EventHandler ConfigLoaded = delegate { };
        #endregion

        public DebugProtocol(ModelManager core, ValueLogger logger)
        {
            this.logger = logger;
            logger.StartLogging += Logger_StartLogging;
            logger.StopLogging += Logger_StopLogging;
            connectors = GetConnectorTypes().ToList();
            incomingBytes = new ConcurrentQueue<byte[]>();
            msgIDs = new Dictionary<byte, byte>();
            messagesWaitingAck = new List<ProtocolMessage>();
            this.core = core;
            myThread = new Thread(DoWork)
            {
                IsBackground = true
            };
            myThread.Start();
            // Create a timer for resending the messages
            resendsTimer = new System.Timers.Timer(100)
            {
                AutoReset = true
            };
            resendsTimer.Elapsed += DoResends;
            resendsTimer.Start();
        }

        /// <summary>
        /// Make sure the eventhandlers are added whenever logging is started
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Logger_StartLogging(object sender, EventArgs e)
        {
            NewReadChannelData += logger.NewValuesReceived;
        }

        /// <summary>
        /// Make sure the eventhandlers are removed whenever logging is stopped
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Logger_StopLogging(object sender, EventArgs e)
        {
            NewReadChannelData -= logger.NewValuesReceived;
        }

        /// <summary>
        /// The destructor to make sure the thread is stopped upon destruction
        /// </summary>
        ~DebugProtocol()
        {
            myThread.Abort();
            myThread.Join();
        }

        #region Connector Manipulation
        /// <summary>
        /// Method used to connect a connector to the embedded system and setting up the correct events
        /// </summary>
        /// <returns>If the connect worked or not</returns>
        public bool Connect()
        {
            if (connector.IsConnected) return true;
            connector.HasConnected += ConnectorConnected;
            connector.UnexpectedDisconnect += ConnectorDisconnected;
            bool returnable = connector.Connect();
            if (!returnable)
            {
                connector.HasConnected -= ConnectorConnected;
                connector.UnexpectedDisconnect -= ConnectorDisconnected;
            }
            return returnable;
        }

        /// <summary>
        /// Method used to disconnect the connector from the embedded system
        /// </summary>
        public void Disconnect()
        {
            connector.Disconnect();
            msgIDs.Clear();
            logger.LogRegisters.Clear();
            HasDisconnected(this, new EventArgs());
            isConnected = false;
        }

        /// <summary>
        /// Show a form with the settings of the connector
        /// </summary>
        public void ShowSettings()
        {
            connector.ShowDialog();
        }

        #region Delegates
        /// <summary>
        /// When the connector actually connects (servers might take a while to have a client connect to it) tell the world and search for CPU nodes
        /// </summary>
        /// <param name="sender">The sender</param>
        /// <param name="e">Possible EventArgs</param>
        public void ConnectorConnected(object sender, EventArgs e)
        {
            isConnected = true;
            HasConnected(sender, e);
            connector.MessageReceived += NewBytes;
            SearchForNodes();
        }
        /// <summary>
        /// When the connector actually disconnects tell the world
        /// </summary>
        /// <param name="sender">The sender</param>
        /// <param name="e">Possible EventArgs</param>
        public void ConnectorDisconnected(object sender, EventArgs e)
        {
            isConnected = false;
            msgIDs.Clear();
            logger.LogRegisters.Clear();
            HasDisconnected(sender, e);
            connector.HasConnected -= ConnectorConnected;
            connector.UnexpectedDisconnect -= ConnectorDisconnected;
            connector.MessageReceived -= NewBytes;
        }
        #endregion
        #endregion

        /// <summary>
        /// This method gathers all classes extending either the IConnector or the IProjectConnector.
        /// </summary>
        /// <returns>The list of all connectors</returns>
        private IEnumerable<IConnector> GetConnectorTypes()
        {
            foreach (Type typeString in AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(p => typeof(IProjectConnector).IsAssignableFrom(p) && !p.IsInterface).ToList())
            {
                yield return (IConnector)Activator.CreateInstance(typeString);
            }
            foreach (Type typeString in AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(p => typeof(IConnector).IsAssignableFrom(p) && !p.IsInterface).ToList())
            {
                yield return (IConnector)Activator.CreateInstance(typeString);
            }
        }

        /// <summary>
        /// This method is constantly run by the thread running in this object
        /// It contains all information about the debugprotocol and makes sure all messages are read and dispatched correctly
        /// It is also responsible for resending messages which have not been ACKed by the uC
        /// </summary>
        private void DoWork()
        {
            byte[] remainder = new byte[0];
            while (true)
            {
                if (incomingBytes.TryDequeue(out byte[] inputByte))
                {
                    // Turn those bytes into messages, making sure we don't forget the remainder of the previous array
                    MessageCodec.DecodeMessages(inputByte, out List<ProtocolMessage> msgs, out remainder, remainder);
                    foreach (ProtocolMessage msg in msgs)
                    {
                        // Make sure the message is valid, otherwise only add it to the list of messages
                        if (msg.Valid)
                        {
                            lock (messagesWaitingAck)
                            {
                                if (messagesWaitingAck.Any(x => x.ControllerID == msg.ControllerID && x.MsgID == msg.MsgID && x.Command == msg.Command))
                                {
                                    messagesWaitingAck.Remove(messagesWaitingAck.First(x => x.ControllerID == msg.ControllerID && x.MsgID == msg.MsgID && x.Command == msg.Command));
                                    msg.IsAckMessage = true;
                                }
                            }
                            switch (msg.Command)
                            {
                                case Command.GetVersion:
                                    msg.InvalidReason = DispatchVersionMessage(msg);
                                    break;
                                case Command.GetInfo:
                                    msg.InvalidReason = DispatchInfoMessage(msg);
                                    break;
                                case Command.WriteRegister:
                                    msg.InvalidReason = DispatchWriteRegisterMessage(msg);
                                    break;
                                case Command.QueryRegister:
                                    msg.InvalidReason = DispatchQueryRegisterMessage(msg);
                                    break;
                                case Command.ConfigChannel:
                                    msg.InvalidReason = DispatchConfigChannelMessage(msg);
                                    break;
                                case Command.Decimation:
                                    msg.InvalidReason = DispatchDecimationMessage(msg);
                                    break;
                                case Command.ResetTime:
                                    msg.InvalidReason = DispatchResetTimeMessage(msg);
                                    break;
                                case Command.ReadChannelData:
                                    msg.InvalidReason = DispatchReadChannelDataMessage(msg);
                                    break;
                                case Command.DebugString:
                                    msg.InvalidReason = DispatchDebugStringMessage(msg);
                                    break;
                            }
                        }
                        // Get the node this message came from and add it to the message list if it hasn't already been set
                        CpuNode node = core.Nodes.FirstOrDefault(x => x.ID == msg.ControllerID);
                        if (node != null)
                        {
                            node.MessageCounter++;
                            if (!msg.Valid)
                            {
#if DEBUG
                                Console.WriteLine(msg);
#endif
                                node.InvalidMessageCounter++;
                            }
                        }
                    }
                }
                else
                {
                    Thread.Sleep(1);
                }
            }
        }

        /// <summary>
        /// Resend all non-ACKed messages again
        /// </summary>
        private void DoResends(Object source, ElapsedEventArgs e)
        {
            if (!isConnected) return;
            lock (messagesWaitingAck)
            {
                foreach (ProtocolMessage msg in messagesWaitingAck)
                {
                    if (msg.TicksSinceLastSend++ > AckTimoutTicks)
                    {
                        msg.TicksSinceLastSend = 0;
                        connector.SendMessage(MessageCodec.EncodeMessage(msg));
#if DEBUG
                        Console.WriteLine($"Resending message, due to no ACK: {msg}");
#endif
                    }
                }
            }
        }

        /// <summary>
        /// This method is called whenever a CpuNode adds a debugchannel
        /// Whenever the event is called, the channel is configured through these steps
        /// </summary>
        /// <param name="sender">The CpuNode</param>
        /// <param name="r">The register for which the debugchannel is to be set up</param>
        public void AddNewDebugChannel(object sender, Register r)
        {
            if (!r.IsDebugChannel) return;
            CpuNode node = (CpuNode)sender;
            ConfigChannel(node.ID, (byte)r.DebugChannel, ChannelMode.LowSpeed, r);
        }

        /// <summary>
        /// This method is called whenever a CpuNode wants to remove a debugchannel
        /// The CpuNode (object) is responsible for removing the debugchannel for itself, this is only for the microcontroller
        /// </summary>
        /// <param name="sender">The CpuNode</param>
        /// <param name="r">The register for which the debugchannel is to be removed</param>
        public void RemoveDebugChannel(object sender, Register r)
        {
            CpuNode node = (CpuNode)sender;
            ConfigChannel(node.ID, (byte)r.DebugChannel, ChannelMode.Off);
        }

        /// <summary>
        /// This method is called whenever a CpuNode wants to update a debugchannel
        /// </summary>
        /// <param name="sender">The CpuNode</param>
        /// <param name="r">The register for which the debugchannel is to be updated</param>
        public void UpdateDebugChannel(object sender, Register r)
        {
            CpuNode node = (CpuNode)sender;
            ConfigChannel(node.ID, (byte)r.DebugChannel, r.ChannelMode, r);
        }

        /// <summary>
        /// This method is used to query the current value for a register
        /// </summary>
        /// <param name="sender">The CpuNode</param>
        /// <param name="r">The register for which the value is queried</param>
        public void Node_RegisterQueriesValue(object sender, Register r)
        {
            CpuNode node = (CpuNode)sender;
            QueryRegister(node.ID, node, r);
        }

        /// <summary>
        /// This method is used to change the value for a register
        /// </summary>
        /// <param name="sender">The CpuNode</param>
        /// <param name="r">The register for which the value is to be changed</param>
        private void Node_RegisterValueChanged(object sender, object[] e)
        {
            if (!(sender is CpuNode) && !(e[0] is Register) && !(e[1] is RegisterValue)) return;
            CpuNode node = (CpuNode)sender;
            Register r = (Register)e[0];
            RegisterValue regValue = (RegisterValue)e[1];
            WriteToRegister(node.ID, regValue.ValueByteArray, r);
        }

        #region Outgoing Messages
        /// <summary>
        /// Ask all nodes if there is anyone there
        /// </summary>
        public void SearchForNodes()
        {
            GetVersion(0xFF, 0x00);
        }

        /// <summary>
        /// This method sends the node a message that it wants to have the version details
        /// </summary>
        /// <param name="nodeID">The ID of the node in question</param>
        /// <param name="msgID">The msgID if ACK is required</param>
        public void GetVersion(byte nodeID, byte? msgID = null)
        {
            SendMessage(new ProtocolMessage(nodeID, msgID == null ? GetMsgID(nodeID) : (byte)msgID, Command.GetVersion));
        }

        /// <summary>
        /// Request the information from the CPU node
        /// </summary>
        /// <param name="nodeID">The ID of the node</param>
        private void GetNodeInfo(byte nodeID)
        {
            SendMessage(new ProtocolMessage(nodeID, GetMsgID(nodeID), Command.GetInfo));
        }

        /// <summary>
        /// Write a value to a Register
        /// </summary>
        /// <param name="nodeID">The CPU node ID</param>
        /// <param name="value">The value to write to the register</param>
        /// <param name="r">The register to write to</param>
        public void WriteToRegister(byte nodeID, byte[] value, Register r)
        {
            List<byte> data = new List<byte>();
            data.AddRange(BitConverter.GetBytes(r.Offset));
            data.Add(MessageCodec.GetControlByte(core.Nodes.First(x => x.ID == nodeID).ProtocolVersion, r.ReadWrite, r.Source, r.DerefDepth));
            data.Add((byte)value.Length);
            data.AddRange(value);
            SendMessage(new ProtocolMessage(nodeID, GetMsgID(nodeID), Command.WriteRegister, data.ToArray()));
        }

        /// <summary>
        /// This method gets the value for a register once
        /// </summary>
        /// <param name="nodeID"></param>
        /// <param name="node"></param>
        /// <param name="r"></param>
        public void QueryRegister(byte nodeID, CpuNode node, Register r)
        {
            if (r.VariableType == VariableType.Unknown) return;
            List<byte> data = new List<byte>();
            data.AddRange(BitConverter.GetBytes(r.Offset));
            data.Add(MessageCodec.GetControlByte(core.Nodes.First(x => x.ID == nodeID).ProtocolVersion, r.ReadWrite, r.Source, r.DerefDepth));
            data.Add((byte)r.Size);
            SendMessage(new ProtocolMessage(nodeID, GetMsgID(nodeID), Command.QueryRegister, data.ToArray()));
        }

        /// <summary>
        /// This method is used to request the configuration of a channel
        /// </summary>
        /// <param name="nodeID">The ID of the node the channel is setup for</param>
        /// <param name="channelID">The ID of the channel</param>
        public void ConfigChannel(byte nodeID, byte channelID)
        {
            List<byte> data = new List<byte>
            {
                channelID
            };
            SendMessage(new ProtocolMessage(nodeID, GetMsgID(nodeID), Command.ConfigChannel, data.ToArray()));
        }

        /// <summary>
        /// This method is used to change the mode of a channel
        /// </summary>
        /// <param name="nodeID">The ID of the node the channel is setup for</param>
        /// <param name="channelID">The ID of the channel</param>
        /// <param name="mode">The new mode</param>
        public void ConfigChannel(byte nodeID, byte channelID, ChannelMode mode)
        {
            List<byte> data = new List<byte>
            {
                channelID,
                (byte)mode
            };
            SendMessage(new ProtocolMessage(nodeID, GetMsgID(nodeID), Command.ConfigChannel, data.ToArray()));
        }

        /// <summary>
        /// This method is used to setup a channel
        /// </summary>
        /// <param name="nodeID">The ID of the node the channel is setup for</param>
        /// <param name="channelID">The ID of the channel</param>
        /// <param name="mode">The mode</param>
        /// <param name="r">The register the channel is bound to</param>
        public void ConfigChannel(byte nodeID, byte channelID, ChannelMode mode, Register r)
        {
            List<byte> data = new List<byte>
            {
                channelID,
                (byte)mode
            };
            data.AddRange(BitConverter.GetBytes(r.Offset));
            data.Add(MessageCodec.GetControlByte(core.Nodes.First(x => x.ID == nodeID).ProtocolVersion, r.ReadWrite, r.Source, r.DerefDepth));
            data.Add((byte)r.Size);
            SendMessage(new ProtocolMessage(nodeID, GetMsgID(nodeID), Command.ConfigChannel, data.ToArray()));
        }

        /// <summary>
        /// This method is used to query the current decimation or to set the new decimation
        /// </summary>
        /// <param name="nodeID">The node to write to</param>
        /// <param name="dec">The new decimation</param>
        public void Decimation(byte nodeID, byte? dec = null)
        {
            if (dec.HasValue)
            {
                SendMessage(new ProtocolMessage(nodeID, GetMsgID(nodeID), Command.Decimation, new byte[] { (byte)dec }));
            }
            else
            {
                SendMessage(new ProtocolMessage(nodeID, GetMsgID(nodeID), Command.Decimation));
            }
        }

        /// <summary>
        /// This method is used to reset the time on a CPU node, can be used on all if no nodeID is given
        /// </summary>
        /// <param name="nodeID">The node to write to</param>
        public void ResetTime(byte? nodeID = null)
        {
            if (nodeID.HasValue)
            {
                SendMessage(new ProtocolMessage((byte)nodeID, GetMsgID((byte)nodeID), Command.ResetTime));
            }
            else
            {
                SendMessage(new ProtocolMessage(0xFF, 0x00, Command.ResetTime));
            }
        }

        /// <summary>
        /// This method is used to configure all channels for a CPUnode
        /// </summary>
        /// <param name="nodeID">The ID of the node</param>
        /// <param name="trace">The mode</param>
        public void ReadChannelData(byte nodeID, byte trace)
        {
            SendMessage(new ProtocolMessage(nodeID, GetMsgID(nodeID), Command.ReadChannelData, new byte[] { trace }));
        }

        /// <summary>
        /// This method is used to send a debug string (terminal like) to a CPU node
        /// </summary>
        /// <param name="nodeID">The node to write the debugstring to</param>
        /// <param name="debugString">The debug string</param>
        public void SendDebugString(byte nodeID, string debugString)
        {
            SendMessage(new ProtocolMessage(nodeID, GetMsgID(nodeID), Command.DebugString, Encoding.UTF8.GetBytes(debugString)));
        }

        /// <summary>
        /// This method returns a proper ID for a message, making sure it never gets to 0
        /// If it would be 0x00, the uC would never send a ACK msg
        /// </summary>
        /// <param name="nodeID">The ID of the node</param>
        /// <returns>The new msg ID</returns>
        private byte GetMsgID(byte nodeID)
        {
            if (!msgIDs.ContainsKey(nodeID)) return 0x00;
            msgIDs[nodeID]++;
            if (msgIDs[nodeID] == 0)
            {
                msgIDs[nodeID]++;
            }
            return msgIDs[nodeID];
        }

        /// <summary>
        /// This method is used to actucally send a msg to the embedded system
        /// It makes sure that if a msg needs an ACK, it is added to the list
        /// </summary>
        /// <param name="msg">The msg to be send</param>
        private void SendMessage(ProtocolMessage msg)
        {
            if (!isConnected) return;
            if (msg.MsgID != 0x00)
            {
                lock (messagesWaitingAck)
                {
                    messagesWaitingAck.Add(msg);
                }
            }
            connector.SendMessage(MessageCodec.EncodeMessage(msg));
        }
        #endregion

        #region Incoming Messages
        #region Delegates
        /// <summary>
        /// This method is called whenever the connector has a new message
        /// It adds the message to the list of new messages
        /// </summary>
        /// <param name="sender">The connector</param>
        /// <param name="e">The received message</param>
        public void NewBytes(object sender, BytesReceivedEventArgs e)
        {
            incomingBytes.Enqueue(e.Message);
        }
        #endregion

        #region DispatchMethods
        /// <summary>
        /// This method is used to dispatch a Version message
        /// Adding the new CPU node to the list of nodes
        /// </summary>
        /// <param name="msg">The message</param>
        /// <returns>In case the dispatching failes, a string with explanation is returned</returns>
        private string DispatchVersionMessage(ProtocolMessage msg)
        {
            // Gather all information from the message
            byte id = msg.ControllerID;
            VersionMessage version_message;
            try
            {
                version_message = new VersionMessage(msg);
            }
            catch (ArgumentException ex)
            {
                return ex.Message;
            }

            // If the node is already known, don't re-add it 
            if (core.Nodes.Any(x => x.ID == msg.ControllerID)) return "CPUNode already known!";

            // Create a new node with the information that was gathered
            CpuNode node = new CpuNode(id, version_message.ProtocolVersion, version_message.ApplicationVersion, version_message.Name, version_message.SerialNumber);
            // Create a msgID, to be used whenever a msg is send
            msgIDs.Add(node.ID, 0);
            // Add the node to the list of nodes in the modelmanager
            core.Nodes.Add(node);
            core.NewCPUFound();
            // Ask the node to send over its information
            GetNodeInfo(node.ID);
            // Turn off all channels
            for (int i = 0; i < node.MaxNumberOfDebugChannels; i++)
            {
                ConfigChannel(node.ID, (byte)i, ChannelMode.Off);
            }
            // Add the event handlers of the node
            node.RegisterQueriesValue += Node_RegisterQueriesValue;
            node.NewDebugChannel += AddNewDebugChannel;
            node.RemoveDebugChannel += RemoveDebugChannel;
            node.UpdateDebugChannel += UpdateDebugChannel;
            node.RegisterValueChanged += Node_RegisterValueChanged;
            node.RegisterLoggingChanged += logger.RegisterLoggingChanged;
            // TODO: Add loading of config again
            // Try to load the configuration from .xml file/
            /*
            if (
                
            node.TryToLoadConfiguration(
                $"{Properties.Settings.Default.ConfigurationPath}" +
                $"{connector.ToString()}" +
                $"/{node.Name.Trim()}" +
                $"/cpu{id.ToString("D2")}" +
                $"-V{node.ApplicationVersion.Major.ToString("D2")}" +
                $"_{node.ApplicationVersion.Minor.ToString("D2")}" +
                $"_{node.ApplicationVersion.Build.ToString("D4")}.xml"))
            {
                ConfigLoaded(this, new EventArgs());
            } else
            {
                // TODO: Implement something to let the user know that no config file was found
            }*/
            return null;
        }


        /// <summary>
        /// This method is used to dispatch an Info message
        /// Updating the nodes sizes
        /// </summary>
        /// <param name="msg">The received message</param>
        /// <returns>In case the dispatching failes, a string with explanation is returned</returns>
        private string DispatchInfoMessage(ProtocolMessage msg)
        {
            // Check if message is too small
            if (msg.CommandData.Length < 2) return "Message too short for Info Message";

            List<byte> records = new List<byte>();
            for (int i = 0; i < msg.CommandData.Length; i++)
            {
                byte b = msg.CommandData.ElementAt(i);
                if (b == REC_SEP || i == msg.CommandData.Length - 1)
                {
                    if (i == msg.CommandData.Length - 1) { records.Add(b); }
                    // For the timestamp, 4 bytes are used 
                    if (records[0] == (byte)VariableType.TimeStamp)
                    {
                        core.Nodes.First(x => x.ID == msg.ControllerID).Sizes[(VariableType)records[0]] = records[4] << 24 | records[3] << 16 | records[2] << 8 | records[1];
                    }
                    else
                    {
                        core.Nodes.First(x => x.ID == msg.ControllerID).Sizes[(VariableType)records[0]] = records[1];
                    }
                    records.Clear();
                }
                else
                {
                    records.Add(b);
                }
            }
            return null;
        }

        /// <summary>
        /// This method is used to dispatch a Write Register message
        /// Checking the result of the earlier request
        /// </summary>
        /// <param name="msg">The received message</param>
        /// <returns>In case the dispatching failes, a string with explanation is returned</returns>
        private string DispatchWriteRegisterMessage(ProtocolMessage msg)
        {
            if (msg.CommandData.Length == 0) return "Message too short for Write Register Message";
            byte result = msg.CommandData[0];

            return null;
        }

        /// <summary>
        /// This method is used to dispatch a Query Register message
        /// Setting the value of the Register, since that was requested
        /// </summary>
        /// <param name="msg">The received message</param>
        /// <returns>In case the dispatching failes, a string with explanation is returned</returns>
        private string DispatchQueryRegisterMessage(ProtocolMessage msg)
        {
            if (msg.CommandData.Length < 7) return "Message too short for Query Register Message";
            uint offset = BitConverter.ToUInt32(msg.CommandData, 0);
            byte ctrl = msg.CommandData[4];

            byte size = msg.CommandData[5];
            if (size == 0x00 && msg.CommandData.Length <= 6)
            {
                // Error reading occured!
                return "Error reading occured";
            }

            byte[] value;
            if (size == 0x00)
            {
                value = msg.CommandData.Skip(6).ToArray();
            }
            else
            {
                value = msg.CommandData.Skip(6).Take(size).ToArray();
            }
            if (!core.Nodes.Any(x => x.ID == msg.ControllerID))
            {
                return "No node found for the msg";
            }
            CpuNode node = core.Nodes.First(x => x.ID == msg.ControllerID);
            MessageCodec.GetControlByteValues(node.ProtocolVersion, ctrl, out ReadWrite readWrite, out Source source, out int derefDepth);
            Register r = node.EmbeddedConfig.GetRegister(offset, readWrite);
            if (r == null)
            {
                return "No register found with this offset or readwrite";
            }
            r.AddValue(RegisterValue.GetRegisterValueByVariableType(r.VariableType, value));
            RegisterQueried(this, r);
            return null;
        }

        /// <summary>
        /// This method is used to dispatch a Config Channel message
        /// Check if the setting up of the channel was done correctly]
        /// Currently Microcontroller -> Embedded Debugger is not yet supported
        /// </summary>
        /// <param name="msg">The received message</param>
        /// <returns>In case the dispatching failes, a string with explanation is returned</returns>
        private string DispatchConfigChannelMessage(ProtocolMessage msg)
        {
            if (msg.CommandData.Length < 2)
            {
                return "Message too short for Config Channel Message";
            }
            byte channel = msg.CommandData[0];
            byte mode = msg.CommandData[1];
            if (msg.CommandData.Length > 2)
            {
                uint offset = BitConverter.ToUInt32(msg.CommandData.Skip(2).Take(4).ToArray(), 0);
                byte ctrl = msg.CommandData[6];
                byte size = msg.CommandData[7];
            }
            // TODO add the correct implementation

            return null;
        }

        /// <summary>
        /// This method is used to dispatch a Decimation message
        /// Setting the new Decimation
        /// </summary>
        /// <param name="msg">The received message</param>
        /// <returns>In case the dispatching failes, a string with explanation is returned</returns>
        private string DispatchDecimationMessage(ProtocolMessage msg)
        {
            if (msg.CommandData.Length < 1)
            {
                return "Message too short for Decimation Message";
            }
            byte dec = msg.CommandData[0];
            core.Decimation = dec;
            return null;
        }


        /// <summary>
        /// This method is used to dispatch a Reset Time message
        /// Nothing has to be done (yet???)
        /// </summary>
        /// <param name="msg">The received message</param>
        /// <returns>In case the dispatching failes, a string with explanation is returned</returns>
        private string DispatchResetTimeMessage(ProtocolMessage msg)
        {
            // Method can be used in time if the protocol is ever upgraded
            return null;
        }


        /// <summary>
        /// This method is used to dispatch a Read Channel Message message
        /// Setting the value of the Register
        /// </summary>
        /// <param name="msg">The received message</param>
        /// <returns>In case the dispatching failes, a string with explanation is returned</returns>
        private string DispatchReadChannelDataMessage(ProtocolMessage msg)
        {
            if (msg.CommandData.Length == 0)
            {
                return "Message too short for Read Channel Data Message";
            }
            byte id = msg.ControllerID;
            CpuNode node = core.Nodes.FirstOrDefault(x => x.ID == id);
            if (node == null) return "No node found";
            byte[] timeArray = msg.CommandData.Take(3).ToArray();
            uint time = (uint)((0x00 << 24) | (timeArray[2] << 16) | (timeArray[1] << 8) | timeArray[0]);
            ushort mask = BitConverter.ToUInt16(msg.CommandData.Skip(3).Take(2).ToArray(), 0);
            List<byte> value = msg.CommandData.Skip(5).ToList();
            for (int i = node.MaxNumberOfDebugChannels; i >= 0; i--)
            {
                if ((mask >> i & 1) == 1 && node.DebugChannels.ContainsKey(i))
                {
                    byte[] myValue = value.Take(node.DebugChannels[i].Size).ToArray();
                    value.RemoveRange(0, node.DebugChannels[i].Size);
                    RegisterValue regVal = new RegisterValue
                    {
                        TimeStamp = time,
                        ValueByteArray = myValue
                    };
                    node.DebugChannels[i].AddValue(regVal);
                }
            }
            NewReadChannelData(this, time);
            return null;
        }


        /// <summary>
        /// This method is used to dispatch a Debug String message
        /// Terminal like this string contains debug information, make sure it gets to the top
        /// </summary>
        /// <param name="msg">The received message</param>
        /// <returns>In case the dispatching failes, a string with explanation is returned</returns>
        private string DispatchDebugStringMessage(ProtocolMessage msg)
        {
            if (!core.Nodes.Any(x => x.ID == msg.ControllerID) || msg.CommandData.Length == 0) return "Empty string";
            CpuNode node = core.Nodes.First(x => x.ID == msg.ControllerID);
            node.NewTerminalData = Encoding.UTF8.GetString(msg.CommandData);
            return null;
        }
        #endregion
        #endregion
    }
}
