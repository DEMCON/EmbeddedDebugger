using EmbeddedDebugger.Connectors.Settings;
using EmbeddedDebugger.Connectors.ZeroMQ;
using EmbeddedDebugger.DebugProtocol;
using EmbeddedDebugger.DebugProtocol.CustomEventArgs;
using EmbeddedDebugger.DebugProtocol.Enums;
using EmbeddedDebugger.DebugProtocol.RegisterValues;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml.Serialization;
using EmbeddedDebugger.DebugProtocol.EmbeddedConfiguration;

namespace EmbeddedDebugger.Connectors.BaseClasses
{
    [XmlInclude(typeof(BaseEmbeddedDebugProtocolConnection))]
    [XmlInclude(typeof(ZeroMqConnector))]
    [Serializable]
    public abstract class DebugConnection
    {
        public abstract string Name { get; }
        [XmlIgnore]
        public abstract bool IsConnected { get; }
        [XmlIgnore]
        public abstract List<ConnectionSetting> ConnectionSettings { get; }
        [XmlIgnore]
        // TODO REMOVE
        public List<CpuNode> Nodes { get; set; }

        public abstract event EventHandler UnexpectedlyDisconnected;
        public abstract event EventHandler HasConnected;
        public abstract event EventHandler<CpuNode> NewNodeFound;
        public abstract event EventHandler<ValueReceivedEventArgs> NewValueReceived;
        public abstract event EventHandler<TraceMessageReceivedEventArgs> NewTraceMessageReceived;
        public abstract event EventHandler<string> NewTerminalMessageReceived;

        public abstract void SetConnectionSettings(List<ConnectionSetting> settings);
        public abstract bool Connect();
        public abstract void Disconnect();
        public abstract void SearchForNodes();
        public abstract void QueryValue(CpuNode cpuNode, Register register);
        public abstract void WriteValue(CpuNode cpuNode, Register register, RegisterValue registerValue);
        public abstract void WriteConsole(CpuNode cpuNode, string message);
        public abstract void SetupSignalTracing(CpuNode cpuNode, Register register, ChannelMode channelMode);
        public abstract void ResetTime(CpuNode cpuNode);
    }
}
