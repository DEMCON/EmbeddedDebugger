using EmbeddedDebugger.Connectors.BaseClasses;
using EmbeddedDebugger.Connectors.Settings;
using EmbeddedDebugger.DebugProtocol;
using EmbeddedDebugger.DebugProtocol.CustomEventArgs;
using EmbeddedDebugger.DebugProtocol.EmbeddedConfiguration;
using EmbeddedDebugger.DebugProtocol.Enums;
using EmbeddedDebugger.DebugProtocol.RegisterValues;
using NetMQ;
using NetMQ.Sockets;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EmbeddedDebugger.Connectors.ZeroMQ
{
    public class ZeroMqConnector : DebugConnection
    {
        private RequestSocket socket;
        private const byte FlagSigned = 0x8;
        private const byte FlagInt = 0x10;
        private const byte FlagFixed = 0x20;
        private const byte FlagFunction = 0x40;
        private const byte Void = 0;
        private const byte Blob = 1;
        private const byte String = 2;
        private object lockObject = new object();

        public override string Name { get; } = "ZeroMQ";
        public override bool IsConnected { get; }
        public string HostName { get; set; }
        public int Port { get; set; }

        public override List<ConnectionSetting> ConnectionSettings { get; } = new List<ConnectionSetting> {
            new ConnectionSetting{Name = "HostName", Value = "127.0.0.1"},
            new ConnectionSetting{Name = "Port",Value = 19026}
        };
        public override event EventHandler UnexpectedlyDisconnected;
        public override event EventHandler HasConnected;
        public override event EventHandler<CpuNode> NewNodeFound;
        public override event EventHandler<ValueReceivedEventArgs> NewValueReceived;
        public override event EventHandler<TraceMessageReceivedEventArgs> NewTraceMessageReceived;
        public override event EventHandler<string> NewTerminalMessageReceived;

        public override void SetConnectionSettings(List<ConnectionSetting> settings)
        {
            //Add some testing for the values
            this.HostName = (string)(settings.First(x => x.Type == typeof(string) && x.Name == "HostName")
                                         ?.Value ?? throw new ArgumentException("HostName not included in settings"));
            this.Port = (int)(settings.First(x => x.Type == typeof(int) && x.Name == "Port")
                                      .Value ?? throw new ArgumentException("Port not included in settings"));
        }

        public override bool Connect()
        {
            // If the hostname is not set, show the settings form
            if (this.HostName == null)
            {
                return false;
            }

            this.socket = new RequestSocket();
            this.socket.Connect($"tcp://{this.HostName}:{Port}");
            this.HasConnected?.Invoke(this, null);
            return true;
        }

        public override void Disconnect()
        {
            throw new NotImplementedException();
        }

        public override async void SearchForNodes()
        {
            if (this.socket?.IsDisposed != true)
            {
                string response = await this.SendMessage("i");
                CpuNode node = new CpuNode(0x00, new Version(2, 0, 0), null, response, "");
                this.NewNodeFound?.Invoke(this.Name, node);

                response = await this.SendMessage("l");
                EmbeddedConfig ed = new EmbeddedConfig();
                uint registerID = 0;
                foreach (string register in response.Split('\n').Where(x => x != string.Empty))
                {
                    string[] split = register.Split('/');
                    string regType = split[0].Substring(0, 2);
                    string regSize = split[0].Substring(2);

                    Register r = new Register
                    {
                        Id = registerID++,
                        Name = $"/{string.Join("/", split.Skip(1))}".Replace("(", ""),
                        Size = int.Parse(regSize, NumberStyles.HexNumber),
                        VariableType = this.GetVariableType(regType),
                        CpuNode = node,
                        ReadWrite = this.GetReadWrite(regType),
                    };

                    ed.AddRegister(r);
                }
                node.EmbeddedConfig = ed;
            }
        }

        public override async void QueryValue(CpuNode cpuNode, Register register)
        {
            string returnable = string.Empty;
            string message = $"r{register.Name}";
            returnable = await this.SendMessage(message);
            this.NewValueReceived?.Invoke(this, new ValueReceivedEventArgs(RegisterValue.GetRegisterValueByVariableType(register.VariableType, StringToByteArray(returnable), timeStamp: 0), register, cpuNode));
        }

        public override void WriteValue(CpuNode cpuNode, Register register, RegisterValue registerValue)
        {
            throw new NotImplementedException();
        }

        public override async void WriteConsole(CpuNode cpuNode, string message)
        {
            if (this.socket?.IsDisposed != true)
            {
                string response = await this.SendMessage(message);
                this.NewTerminalMessageReceived?.Invoke(this, response);
            }
        }

        public override void SetupSignalTracing(CpuNode cpuNode, Register register, ChannelMode channelMode)
        {
            new Task(() =>
            {
                while (true)
                {
                    this.QueryValue(cpuNode, register);
                    Thread.Sleep(10);
                }
            }).Start();
            //throw new NotImplementedException();
        }

        public override void ResetTime(CpuNode cpuNode)
        {
            throw new NotImplementedException();
        }

        private static byte[] StringToByteArray(string hex)
        {
            if (hex.Length == 0)
            {
                return new byte[]{};
            }
            hex = hex.Length % 2 == 1 ? $"0{hex}" : hex;
            return Enumerable.Range(0, hex.Length)
                .Where(x => x % 2 == 0)
                .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                .ToArray();
        }

        private async Task<string> SendMessage(string message)
        {
            string returnable = string.Empty;
            lock (this.lockObject)
            {
                this.socket.SendFrame(message);
                returnable = string.Join(string.Empty, this.socket.ReceiveMultipartStrings());
            }

            return returnable;
        }

        private ReadWrite GetReadWrite(string input)
        {
            ReadWrite result = ReadWrite.None;
            if (this.GetVariableType(input) != VariableType.Unknown)
            {
                result = (int.Parse(input, NumberStyles.HexNumber) >> 4 & 0x01) == 1
                    ? ReadWrite.Read
                    : ReadWrite.ReadWrite;
            }
            return result;
        }

        private VariableType GetVariableType(string input)
        {
            switch (int.Parse(input, NumberStyles.HexNumber) & 0x3F)
            {
                case (FlagFixed | FlagInt | FlagSigned | 0):
                    return VariableType.Char;
                case (FlagFixed | FlagInt | 0):
                    return VariableType.UChar;
                case (FlagFixed | FlagInt | FlagSigned | 1):
                    return VariableType.Short;
                case (FlagFixed | FlagInt | 1):
                    return VariableType.UShort;
                case (FlagFixed | FlagInt | FlagSigned | 3):
                    return VariableType.Int;
                case (FlagFixed | FlagInt | 3):
                    return VariableType.UInt;
                case (FlagFixed | FlagInt | FlagSigned | 7):
                    return VariableType.Long;
                case (FlagFixed | FlagInt | 7):
                    return VariableType.ULong;
                case (FlagFixed | FlagSigned | 3):
                    return VariableType.Float;
                case (FlagFixed | FlagSigned | 7):
                    return VariableType.Double;
                case (FlagFixed | 0):
                    return VariableType.Bool;
                case (FlagFixed | 3):
                case (FlagFixed | 7):
                    return VariableType.Pointer;
                case (Void):
                    return VariableType.Void;
                case (Blob):
                    return VariableType.Blob;
                case (String):
                    return VariableType.String;
                default:
                    return VariableType.Unknown;
            }
        }
    }
}
