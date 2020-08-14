using EmbeddedDebugger.Connectors.Serial;
using EmbeddedDebugger.Connectors.TCP;
using EmbeddedDebugger.DebugProtocol;
using EmbeddedDebugger.DebugProtocol.CustomEventArgs;
using EmbeddedDebugger.DebugProtocol.Enums;
using EmbeddedDebugger.DebugProtocol.Messages;
using EmbeddedDebugger.DebugProtocol.RegisterValues;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace EmbeddedDebugger.Connectors.BaseClasses
{
    [XmlInclude(typeof(SerialConnector))]
    [XmlInclude(typeof(TcpConnector))]
    [Serializable]
    public abstract class BaseEmbeddedDebugProtocolConnection : DebugConnection
    {
        private byte[] remainderBytes;
        private Dictionary<CpuNode, Dictionary<byte, Register>> DebugChannels;

        protected BaseEmbeddedDebugProtocolConnection()
        {
            this.remainderBytes = new byte[0];
            DebugChannels = new Dictionary<CpuNode, Dictionary<byte, Register>>();
        }

        public override event EventHandler<CpuNode> NewNodeFound;
        public override event EventHandler<ValueReceivedEventArgs> NewValueReceived;
        public override event EventHandler<TraceMessageReceivedEventArgs> NewTraceMessageReceived;
        public override event EventHandler<string> NewTerminalMessageReceived;

        public abstract override bool Connect();
        public abstract override void Disconnect();

        public override void SearchForNodes()
        {
            // ControllerID 0xFF means broadcast
            this.SendMessage(MessageCodec.EncodeMessage(new ProtocolMessage(0xFF, 0x00, Command.GetVersion)));
        }

        public override void QueryValue(CpuNode cpuNode, Register register)
        {
            Version protocolVersion = cpuNode.ProtocolVersion;
            byte control = MessageCodec.GetControlByte(protocolVersion, register.ReadWrite, register.Source, register.DerefDepth);
            QueryRegisterMessage msg = new QueryRegisterMessage()
            {
                Offset = register.Offset,
                Control = control,
            };
            // TODO add the msgID
            this.SendMessage(MessageCodec.EncodeMessage(msg.ToProtocolMessage(cpuNode.Id, 0x01)));
        }

        public override void WriteValue(CpuNode cpuNode, Register register, RegisterValue registerValue)
        {
            Version protocolVersion = cpuNode.ProtocolVersion;
            byte control = MessageCodec.GetControlByte(protocolVersion, register.ReadWrite, register.Source, register.DerefDepth);
            WriteRegisterMessage msg = new WriteRegisterMessage()
            {
                Offset = register.Offset,
                Control = control,
                Value = registerValue.ValueByteArray,
            };
            // TODO add the msgID
            this.SendMessage(MessageCodec.EncodeMessage(msg.ToProtocolMessage(cpuNode.Id, 0x01)));
        }

        public override void WriteConsole(CpuNode cpuNode, string message)
        {
            DebugStringMessage msg = new DebugStringMessage()
            {
                Message = message
            };
            // TODO add the msgID
            this.SendMessage(MessageCodec.EncodeMessage(msg.ToProtocolMessage(cpuNode.Id, 0x01)));
        }

        public override void SetupSignalTracing(CpuNode cpuNode, Register register, ChannelMode channelMode)
        {
            // Make sure the CpuNode is present in our dictionary
            if (DebugChannels.ContainsKey(cpuNode))
            {
                if (!this.DebugChannels[cpuNode].ContainsValue(register) && channelMode != ChannelMode.Off)
                {
                    for (byte i = 0; i < 255; i++)
                    {
                        if (!register.CpuNode.DebugChannels.ContainsKey(i))
                        {
                            this.DebugChannels[cpuNode].Add(i, register);
                            break;
                        }
                    }
                }
                if (this.DebugChannels[cpuNode].ContainsValue(register))
                {
                    Version protocolVersion = cpuNode.ProtocolVersion;
                    ConfigChannelMessage msg = new ConfigChannelMessage()
                    {
                        ChannelId = this.DebugChannels[cpuNode].First(x => x.Value.Id == register.Id).Key,
                        Mode = channelMode,
                        Offset = register.Offset,
                        Control = MessageCodec.GetControlByte(protocolVersion, register.ReadWrite, register.Source, register.DerefDepth),
                        Size = register.Size,
                    };
                    SendMessage(cpuNode.Id, msg);
                    if (this.DebugChannels[cpuNode].ContainsValue(register) && channelMode == ChannelMode.Off)
                    {
                        this.DebugChannels[cpuNode].Remove(this.DebugChannels[cpuNode].First(x => x.Value.Id == register.Id).Key);
                    }
                }
            }
        }

        private void SendMessage(byte nodeID, ApplicationMessage msg)
        {
            ProtocolMessage protocol_message = msg.ToProtocolMessage(nodeID, 0x00);
            SendMessage(protocol_message);
        }

        private void SendMessage(ProtocolMessage msg)
        {
            SendMessage(MessageCodec.EncodeMessage(msg));
        }

        public override void ResetTime(CpuNode cpuNode)
        {
            // TODO Add correct message id
            this.SendMessage(MessageCodec.EncodeMessage(new ProtocolMessage(cpuNode.Id, 0x02, Command.ResetTime)));
        }

        public abstract void SendMessage(byte[] message);

        protected void ReceiveMessage(BytesReceivedEventArgs e)
        {
            MessageCodec.DecodeMessages(e.Message, out List<ProtocolMessage> messages, out this.remainderBytes, this.remainderBytes);
            foreach (ProtocolMessage protocolMessage in messages)
            {
                switch (protocolMessage.Command)
                {
                    case Command.GetVersion:
                        this.DispatchVersionMessage(protocolMessage);
                        break;
                    case Command.GetInfo:
                        break;
                    case Command.WriteRegister:
                        break;
                    case Command.QueryRegister:
                        this.DispatchQueryRegisterMessage(protocolMessage);
                        break;
                    case Command.ConfigChannel:
                        break;
                    case Command.Decimation:
                        break;
                    case Command.ResetTime:
                        break;
                    case Command.ReadChannelData:
                        DispatchReadChannelDataMessage(protocolMessage);
                        break;
                    case Command.DebugString:
                        break;
                    case Command.EmbeddedConfiguration:
                        break;
                    case Command.Tracing:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        private void DispatchVersionMessage(ProtocolMessage protocolMessage)
        {
            VersionMessage versionMessage = new VersionMessage(protocolMessage);
            byte id = protocolMessage.ControllerID;
            // Create a new node with the information that was gathered
            CpuNode node = new CpuNode(id, versionMessage.ProtocolVersion, versionMessage.ApplicationVersion, versionMessage.Name, versionMessage.SerialNumber);
            if (
                // TODO, make the base path globally configurable
                node.TryToLoadConfiguration(
                    Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                        "EmbeddedDebugger",
                        "Configurations",
                        this.Name,
                        node.Name.Trim(),
                        $"cpu{id:D2}-V{node.ApplicationVersion.Major:D2}_{node.ApplicationVersion.Minor:D2}_{node.ApplicationVersion.Build:D4}.xml")))
            {
                this.DebugChannels.Add(node, new Dictionary<byte, Register>());
                this.NewNodeFound?.Invoke(this, node);
            }
            else
            {
                // TODO: Implement something to let the user know that no config file was found
            }
        }

        private void DispatchQueryRegisterMessage(ProtocolMessage protocolMessage)
        {
            QueryRegisterMessage response = new QueryRegisterMessage(protocolMessage);

            if (response.Value == null)
            {
                // Error reading occured!
                throw new ArgumentException("Error reading occured");
            }

            if (this.Nodes.All(x => x.Id != protocolMessage.ControllerID))
            {
                throw new ArgumentException("No node found for the msg");
            }
            CpuNode node = this.Nodes.First(x => x.Id == protocolMessage.ControllerID);
            MessageCodec.GetControlByteValues(node.ProtocolVersion, response.Control, out ReadWrite readWrite, out Source source, out int derefDepth);
            Register r = node.EmbeddedConfig.GetRegister(response.Offset, readWrite);
            if (r == null)
            {
                throw new ArgumentException("No register found with this offset or readwrite");
            }
            NewValueReceived(this, new ValueReceivedEventArgs(RegisterValue.GetRegisterValueByVariableType(r.VariableType, response.Value), r, node));
        }

        private void DispatchReadChannelDataMessage(ProtocolMessage protocolMessage)
        {
            byte id = protocolMessage.ControllerID;
            CpuNode node = this.Nodes.First(x => x.Id == protocolMessage.ControllerID);
            if (node == null)
            {
                throw new ArgumentException("No node found");
            }

            ReadChannelDataMessage readChannelDataMessage = new ReadChannelDataMessage(protocolMessage);
            List<byte> value = readChannelDataMessage.Data.ToList();

            for (int i = 255; i >= 0; i--)
            {
                if ((readChannelDataMessage.Mask >> i & 1) == 1 && this.DebugChannels[node].ContainsKey((byte)i))
                {
                    byte[] myValue = value.Take(this.DebugChannels[node][(byte)i].Size).ToArray();
                    value.RemoveRange(0, this.DebugChannels[node][(byte)i].Size);
                    RegisterValue regVal = new RegisterValue
                    {
                        TimeStamp = readChannelDataMessage.TimeStamp,
                        ValueByteArray = myValue
                    };
                    NewValueReceived(this, new ValueReceivedEventArgs(regVal, this.DebugChannels[node][(byte)i], node));
                }
            }
        }
    }
}
