/*
Embedded Debugger PC Application which can be used to debug embedded systems at a high level.
Copyright (C) 2019 DEMCON advanced mechatronics B.V.

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/
using EmbeddedDebugger.DebugProtocol.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EmbeddedDebugger.DebugProtocol.Messages;

namespace EmbeddedDebugger.DebugProtocol.Messages
{
    public class TraceMessage : ApplicationMessage
    {
        #region fields
        private TraceLevel traceLevel;
        private string message;
        private byte nodeID;
        private readonly DateTime dateTime;
        #endregion

        #region Properties
        public TraceLevel TraceLevel { get => traceLevel; set => traceLevel = value; }
        public string Message { get => message; set => message = value; }
        public byte NodeID { get => nodeID; set => nodeID = value; }
        public DateTime DateTime { get => dateTime; }
        #endregion

        public override Command Command => Command.Tracing;
        
        public TraceMessage()
        {
            dateTime = DateTime.Now;
        }

        public TraceMessage(byte[] byteArray, byte nodeID = 0x00)
        {
            if (byteArray.Length < 2) throw new ArgumentException("ByteArray too small for a tracemessage");
            traceLevel = (TraceLevel)byteArray[0];
            if (byteArray.Length > 1)
            {
                message = Encoding.UTF8.GetString(byteArray.Skip(1).ToArray());
            }
            else
            {
                message = "";
            }
            this.nodeID = nodeID;
            dateTime = DateTime.Now;
        }

        public override byte[] ToBytes()
        {
            List<byte> returnable = new List<byte>()
            {
                (byte)traceLevel,
            };
            returnable.AddRange(Encoding.UTF8.GetBytes(message));
            return returnable.ToArray();
        }
    }
}
