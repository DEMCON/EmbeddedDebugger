using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EmbeddedDebugger.DebugProtocol.Enums;

namespace EmbeddedDebugger.DebugProtocol.Messages
{
    public class WriteRegisterMessage : ApplicationMessage
    {
        public uint Offset { get; set; }

        public byte Control { get; set; }

        public byte[] Value { get; set; }

        public override Command Command => Command.WriteRegister;

        public WriteRegisterMessage()
        {

        }

        public WriteRegisterMessage(ProtocolMessage msg) : this(msg.CommandData)
        {
        }

        public WriteRegisterMessage(byte[] payload)
        {
            if (payload.Length < 6)
            {
                throw new ArgumentException("Too few data for write register message");
            }

            Offset = BitConverter.ToUInt32(payload, 0);
            Control = payload[4];
            int size = payload[5];
            Value = payload.Skip(6).ToArray();

            if (Value.Length != size)
            {
                throw new ArgumentException("Value size does not match size field.");
            }
        }

        public override byte[] ToBytes()
        {
            List<byte> data = new List<byte>();
            data.AddRange(BitConverter.GetBytes(Offset));
            data.Add(Control);
            data.Add((byte)Value.Length);
            data.AddRange(Value);
            return data.ToArray();
        }
    }
}
