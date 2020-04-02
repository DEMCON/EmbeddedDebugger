using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EmbeddedDebugger.DebugProtocol.Enums;

namespace EmbeddedDebugger.DebugProtocol.Messages
{
    public class DecimationMessage : ApplicationMessage
    {
        public byte? Decimation { get; set; }

        public DecimationMessage()
        {
        }

        public DecimationMessage(ProtocolMessage msg) : this(msg.CommandData)
        {
        }

        public DecimationMessage(byte[] payload)
        {
            if (payload.Length > 0)
            {
                Decimation = payload[0];
            }
        }

        public override Command Command => Command.Decimation;

        public override byte[] ToBytes()
        {
            List<byte> data = new List<byte>();
            if (Decimation.HasValue)
            {
                data.Add(Decimation.Value);
            }
            return data.ToArray();
        }
    }
}
