using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EmbeddedDebugger.DebugProtocol.Enums;

namespace EmbeddedDebugger.DebugProtocol.Messages
{
    public abstract class ApplicationMessage
    {
        public abstract Command Command { get; }

        public abstract byte[] ToBytes();

        public ProtocolMessage ToProtocolMessage(byte nodeId, byte msgId)
        {
            return new ProtocolMessage(nodeId, msgId, Command, ToBytes());
        }
    }
}
