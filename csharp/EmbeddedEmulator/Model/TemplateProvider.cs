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
using EmbeddedDebugger.DebugProtocol.Messages;
using EmbeddedDebugger.Model.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EmbeddedEmulator.Model
{
    public static class TemplateProvider
    {
        private static readonly byte[] versionData = { 0x00, 0x05, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x05, 0x46, 0x4F, 0x49, 0x54, 0x4E, 0x04, 0x30, 0x30, 0x31, 0x0A };
        private static readonly byte[] infoData = {
            0x00, 0x01, 0x33,
            0x01, 0x04, 0x33,
            0x02, 0x01, 0x33,
            0x03, 0x02, 0x33,
            0x04, 0x02, 0x33,
            0x05, 0x04, 0x33,
            0x06, 0x08, 0x33,
            0x07, 0x04, 0x33,
            0x08, 0x08, 0x33,
            0x09, 0x08, 0x33,
            0x0A, 0xE8, 0x03, 0x00, 0x00
        };
        private static readonly byte[][] configurationData =
        {
            new byte[]{ 0x0E, 0x6d, 0x79, 0x52, 0x65, 0x61, 0x64, 0x56, 0x61, 0x72, 0x69, 0x61, 0x62, 0x6c, 0x65, 0x01, 0x00, 0x00, 0x00, 0x04, 0x05, 0b0111_0000, 0x01, 0x00, 0x00, 0x00},
            new byte[]{ 0x0F, 0x6d, 0x79, 0x57, 0x72, 0x69, 0x74, 0x65, 0x56, 0x61, 0x72, 0x69, 0x61, 0x62, 0x6c, 0x65, 0x00, 0x00, 0x00, 0x01, 0x04, 0x05, 0b1111_0000, 0x00, 0x0F, 0x00, 0x03},
            new byte[]{ 0x10, 0x52, 0x65, 0x64, 0x20, 0x47, 0x72, 0x65, 0x65, 0x6e, 0x20, 0x42, 0x75, 0x74, 0x74, 0x6f, 0x6e, 0x00, 0x00, 0x00, 0x00, 0x01, 0x02, 0b1111_0000, 0x00, 0x00, 0x00, 0x00 },
        };

        private static byte controllerID = 0x01;
        public static byte ControllerID { get => controllerID; set => controllerID = value; }

        public static ProtocolMessage GetVersionMessage(byte msgID, EmbeddedConfig ec, byte localControllerID)
        {
            var versionMessage = new VersionMessage()
            {
                ProtocolVersion = ec.ProtocolVersion,
                ApplicationVersion = ec.ApplicationVersion,
                Name = ec.CpuName,
                SerialNumber = ec.SerialNumber,
            };

            return new ProtocolMessage(localControllerID, msgID, Command.GetVersion, versionMessage.ToBytes());
        }

        public static ProtocolMessage GetInfoMessage(byte msgID, byte controllerID)
        {
            return new ProtocolMessage(controllerID, msgID, Command.GetInfo, infoData);
        }

        public static ProtocolMessage GetAccMsg(ProtocolMessage msg)
        {
            return new ProtocolMessage(msg.ControllerID, msg.MsgID, msg.Command);
        }

        public static IEnumerable<ProtocolMessage> GetConfiguration(byte msgID, EmbeddedConfig embeddedConfig, byte controllerID, uint? id = null)
        {
            if (id.HasValue)
            {
                if (embeddedConfig.ReadRegisters.Any(x => x.ID == id) || embeddedConfig.WriteRegisters.Any(x => x.ID == id))
                {
                    foreach (Register reg in embeddedConfig.ReadRegisters.Where(x => x.ID == id))
                    {
                        yield return new ProtocolMessage(controllerID, msgID, Command.EmbeddedConfiguration, reg.GetBytes(embeddedConfig.ProtocolVersion));
                    }
                    foreach (Register reg in embeddedConfig.WriteRegisters.Where(x => x.ID == id))
                    {
                        yield return new ProtocolMessage(controllerID, msgID, Command.EmbeddedConfiguration, reg.GetBytes(embeddedConfig.ProtocolVersion));
                    }
                }
                else
                {
                    List<byte> data = new List<byte>();
                    data.AddRange(BitConverter.GetBytes((uint)id));
                    data.Add(0x00);
                    yield return new ProtocolMessage(controllerID, msgID, Command.EmbeddedConfiguration, data.ToArray());
                }
            }
            else
            {
                foreach (Register reg in embeddedConfig.ReadRegisters)
                {
                    yield return new ProtocolMessage(controllerID, msgID, Command.EmbeddedConfiguration, reg.GetBytes(embeddedConfig.ProtocolVersion));
                }
                foreach (Register reg in embeddedConfig.WriteRegisters)
                {
                    yield return new ProtocolMessage(controllerID, msgID, Command.EmbeddedConfiguration, reg.GetBytes(embeddedConfig.ProtocolVersion));
                }
                yield return new ProtocolMessage(controllerID, msgID, Command.EmbeddedConfiguration, BitConverter.GetBytes(((uint)embeddedConfig.ReadRegisters.Count + (uint)embeddedConfig.WriteRegisters.Count)));
            }
        }

        public static ProtocolMessage GetChannelDataMessage(int value, long timeStamp)
        {
            // EC-61-53-01-00-EA-61-53-00-FD
            byte[] time = BitConverter.GetBytes(timeStamp).Take(3).ToArray();
            byte[] data = new byte[9];
            Array.Copy(time, 0, data, 0, 3);
            data[3] = 0x01;
            data[4] = 0x00;
            data[5] = (byte)value;
            data[6] = 0x00;
            data[7] = 0x00;
            data[8] = 0x00;
            return new ProtocolMessage(controllerID, 0x00, Command.ReadChannelData, data);
        }

        public static ProtocolMessage GetDebugStringMessage(ProtocolMessage incomingMessage)
        {
            string returnable = $"You send me: {Encoding.ASCII.GetString(incomingMessage.CommandData)}";
            return new ProtocolMessage(controllerID, incomingMessage.MsgID, Command.DebugString, Encoding.ASCII.GetBytes(returnable));
        }

        public static ProtocolMessage GetDebugStringMessage(string toSend)
        {
            return new ProtocolMessage(controllerID, 0x00, Command.DebugString, Encoding.ASCII.GetBytes(toSend));
        }

        public static ProtocolMessage GetQueryRegisterMessage(ProtocolMessage incomingMessage, byte[] value)
        {
            List<byte> data = new List<byte>();
            data.AddRange(incomingMessage.CommandData);
            data.AddRange(value);
            return new ProtocolMessage(controllerID, incomingMessage.MsgID, Command.QueryRegister, data.ToArray());
        }

        public static ProtocolMessage GetReadChannelDataMessage(byte[] data, byte controllerID)
        {
            //Console.WriteLine(new ProtocolMessage(controllerID, 0x00, Command.ReadChannelData, data));
            return new ProtocolMessage(controllerID, 0x00, Command.ReadChannelData, data);
        }

        public static ProtocolMessage GetTraceMessage(TraceMessage message)
        {
            Console.WriteLine($"ControllerID: {controllerID}");
            return new ProtocolMessage(message.NodeID, 0x00, Command.Tracing, message.ToBytes());
        }
    }
}
