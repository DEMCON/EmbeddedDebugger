using EmbeddedDebugger.DebugProtocol;
using EmbeddedDebugger.DebugProtocol.RegisterValues;
using System;

namespace EmbeddedDebugger.DebugProtocol.CustomEventArgs
{
    public class ValueReceivedEventArgs : EventArgs
    {
        public RegisterValue Value { get; set; }
        public Register Register { get; set; }
        public CpuNode CpuNode { get; set; }

        public ValueReceivedEventArgs() { }

        public ValueReceivedEventArgs(RegisterValue registerValue, Register register, CpuNode cpuNode)
        {
            this.Value = registerValue;
            this.Register = register;
            this.CpuNode = cpuNode;
        }
    }
}
