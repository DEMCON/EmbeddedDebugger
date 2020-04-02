using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EmbeddedDebugger.DebugProtocol.Enums;

namespace EmbeddedDebugger.DebugProtocol.Messages
{
    public class QueryRegisterMessage : ApplicationMessage
    {
        public override Command Command => Command.QueryRegister;

        public uint Offset { get; set; }

        public byte Control { get; set; }

        public int Size { get; set; }

        public byte[] Value { get; set; }

        public QueryRegisterMessage()
        {
        }

        public QueryRegisterMessage(ProtocolMessage msg) : this(msg.CommandData)
        {
        }

        public QueryRegisterMessage(byte[] payload)
        {
            if (payload.Length < 6)
            {
                throw new ArgumentException("Message too short for Query Register Message");
            }

            Offset = BitConverter.ToUInt32(payload, 0);
            Control = payload[4];
            Size = payload[5];

            // This is a response message
            if (Size > 0 && payload.Length > 6)
            {
                Value = payload.Skip(6).Take(Size).ToArray();
            }
        }

        public override byte[] ToBytes()
        {
            List<byte> data = new List<byte>();

            data.AddRange(BitConverter.GetBytes(Offset));
            data.Add(Control);
            if (Value == null)
            {
                // This is a request
                data.Add((byte)Size);
            }
            else
            {
                // This is a response
                data.Add((byte)Value.Length);
                data.AddRange(Value);
            }
            return data.ToArray();
        }
    }
}
