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
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmbeddedDebugger.DebugProtocol.Messages
{
    /// <summary>
    /// This class is a message which is used in the debug protocol
    /// </summary>
    public class ProtocolMessage
    {
        #region Properties
        private readonly byte controllerID;
        public byte ControllerID { get => controllerID; }

        private readonly byte msgID;
        public byte MsgID { get => msgID; }

        private readonly Command command;
        public Command Command { get => command; }

        private readonly byte[] commandData;
        public byte[] CommandData { get => commandData; }
        public string CommandDataString { get => BitConverter.ToString(commandData ?? new byte[0]); }

        public bool Valid { get => string.IsNullOrEmpty(invalidReason) || isAckMessage; }

        private string invalidReason;
        public string InvalidReason { get => invalidReason; set => invalidReason = value; }

        private int ticksSinceLastSend;
        public int TicksSinceLastSend { get => ticksSinceLastSend; set => ticksSinceLastSend = value; }

        private bool isAckMessage;
        public bool IsAckMessage { get => isAckMessage; set => isAckMessage = value; }
        #endregion

        #region Constructors
        public ProtocolMessage(byte controllerID, byte msgID, Command command, byte[] commandData = null)
        {
            this.controllerID = controllerID;
            this.msgID = msgID;
            this.command = command;
            this.commandData = commandData ?? new byte[0];
        }

        public ProtocolMessage(byte[] msg)
        {
            if (!string.IsNullOrEmpty(MessageCodec.ValidateMessageBytes(msg)))
            {
                throw new ArgumentException($"Invalid Message! Reason: {MessageCodec.ValidateMessageBytes(msg)} Message was: {BitConverter.ToString(msg)}");
            }
            controllerID = msg[1];
            msgID = msg[2];
            command = (Command)msg[3];
            commandData = new byte[msg.Length - 6];
            Array.Copy(msg, 4, commandData, 0, msg.Length - 6);
        }

        public ProtocolMessage(string invalidReason, byte controllerID = 0x00)
        {
            this.invalidReason = invalidReason;
            this.controllerID = controllerID;
        }
        #endregion

        public override string ToString()
        {
            return $"uC[{controllerID}] \t| msgID[{msgID}] \t| invalid[{invalidReason}] \t| command[{command.ToString()}] \t| msg[{BitConverter.ToString(commandData ?? new byte[] { 0x00 })}]";
        }
    }
}
