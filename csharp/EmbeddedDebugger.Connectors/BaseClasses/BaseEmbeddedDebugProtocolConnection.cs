using EmbeddedDebugger.Connectors.Serial;
using EmbeddedDebugger.Connectors.TCP;
using EmbeddedDebugger.DebugProtocol;
using EmbeddedDebugger.DebugProtocol.CustomEventArgs;
using EmbeddedDebugger.DebugProtocol.Enums;
using EmbeddedDebugger.DebugProtocol.Messages;
using EmbeddedDebugger.DebugProtocol.RegisterValues;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;

namespace EmbeddedDebugger.Connectors.BaseClasses
{
    [XmlInclude(typeof(SerialConnector))]
    [XmlInclude(typeof(TcpConnector))]
    [Serializable]
    public abstract class BaseEmbeddedDebugProtocolConnection : DebugConnection
    {
        private byte[] remainderBytes;

        protected BaseEmbeddedDebugProtocolConnection()
        {
            this.remainderBytes = new byte[0];
        }

        public override event EventHandler<CpuNode> NewNodeFound;
        public override event EventHandler<ValueReceivedEventArgs> NewValueReceived;
        public override event EventHandler<TraceMessageReceivedEventArgs> NewTraceMessageReceived;


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
            throw new NotImplementedException();
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
            r.AddValue(RegisterValue.GetRegisterValueByVariableType(r.VariableType, response.Value));
        }
    }
}
