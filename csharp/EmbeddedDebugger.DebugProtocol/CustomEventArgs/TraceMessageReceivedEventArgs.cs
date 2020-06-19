using System;
using EmbeddedDebugger.DebugProtocol.Messages;

namespace EmbeddedDebugger.DebugProtocol.CustomEventArgs
{
    public class TraceMessageReceivedEventArgs : EventArgs
    {
        public CpuNode CpuNode { get; set; }
        public TraceMessage Message { get; set; }
    }
}
