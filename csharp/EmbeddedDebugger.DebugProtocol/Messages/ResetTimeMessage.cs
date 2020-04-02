using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EmbeddedDebugger.DebugProtocol.Enums;

namespace EmbeddedDebugger.DebugProtocol.Messages
{
    public class ResetTimeMessage : ApplicationMessage
    {
        public override Command Command => throw new NotImplementedException();

        public ResetTimeMessage()
        {
        }

        public ResetTimeMessage(ProtocolMessage msg) : this(msg.CommandData)
        {
        }

        public ResetTimeMessage(byte[] payload)
        {
            throw new NotImplementedException();
        }

        public override byte[] ToBytes()
        {
            throw new NotImplementedException();
        }
    }
}
