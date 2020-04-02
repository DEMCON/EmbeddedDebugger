using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EmbeddedDebugger.DebugProtocol.Enums;

namespace EmbeddedDebugger.DebugProtocol.Messages
{
    public class DebugStringMessage : ApplicationMessage
    {
        public override Command Command => Command.DebugString;

        public string Message { get; set; }

        public DebugStringMessage()
        {
        }

        public DebugStringMessage(ProtocolMessage msg)
        {
            Message = Encoding.UTF8.GetString(msg.CommandData);
        }

        public override byte[] ToBytes()
        {
            return Encoding.UTF8.GetBytes(Message);
        }
    }
}
