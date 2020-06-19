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
using System;
using System.Collections.Generic;
using System.Linq;

namespace EmbeddedDebugger.DebugProtocol
{
    /// <summary>
    /// This class is used to encode and decode messages used with the Protocol
    /// The protocol is not explained here, see the .pdf file
    /// </summary>
    public static class MessageCodec
    {
        private const byte Stx = 0x55;
        private const byte Etx = 0xAA;
        private const byte Esc = 0x66;

        /// <summary>
        /// Takes a message and turns it into a byte array
        /// Can be seen as serialization
        /// </summary>
        /// <param name="message">The message to be serialized</param>
        /// <returns>The byte array</returns>
        public static byte[] EncodeMessage(ProtocolMessage message)
        {
            List<byte> data = new List<byte>
            {
                message.ControllerID,
                message.MsgID,
                (byte)message.Command
            };
            data.AddRange(message.CommandData);
            data.Add(Crc.Calculate(data.ToArray()));

            data = AddEscapeCharacters(data);
            data.Insert(0, Stx);
            data.Add(Etx);
            return data.ToArray();
        }

        /// <summary>
        /// Takes a buffer and reads all available messages inside
        /// Can be seen as deserialization
        /// Outputs a remainder, for whenever a message spans over multiple buffers
        /// </summary>
        /// <param name="inputData">The buffer</param>
        /// <param name="messages">Output messages</param>
        /// <param name="remainderOutput">The remainder of the buffer</param>
        /// <param name="remainder">The input remainder, which is inserted at the front of the buffer</param>
        public static void DecodeMessages(byte[] inputData, out List<ProtocolMessage> messages, out byte[] remainderOutput, byte[] remainder = null)
        {
            remainder = remainder ?? new byte[0];
            byte[] data = new byte[remainder.Length + inputData.Length];
            Array.Copy(remainder, 0, data, 0, remainder.Length);
            Array.Copy(inputData, 0, data, remainder.Length, inputData.Length);

            int stxIndex = -1;
            int etxIndex = -1;
            messages = new List<ProtocolMessage>();
            remainderOutput = new byte[0];

            while (true)
            {
                for (int i = 0; i < data.Length; i++)
                {
                    if (data[i] == Stx)
                    {
                        stxIndex = i;
                    }
                    else if (data[i] == Etx && stxIndex != -1)
                    {
                        etxIndex = i;
                        break;
                    }
                }

                if (stxIndex >= 0 && etxIndex >= 0 && stxIndex < data.Length && etxIndex < data.Length && stxIndex + 4 < etxIndex)
                {
                    byte[] msgBytes = ReplaceEscapeCharacters(data.Skip(stxIndex).Take(etxIndex - stxIndex + 1).ToList()).ToArray();
                    ProtocolMessage msg;
                    try
                    {
                        msg = new ProtocolMessage(msgBytes);
                    }
                    catch (Exception e)
                    {
#if DEBUG
                        Console.WriteLine(e);
#endif
                        msg = new ProtocolMessage(e.Message, msgBytes[1]);
                    }
                    messages.Add(msg);
                }
                else if (etxIndex == -1)
                {
                    remainderOutput = data;
                    break;
                }

                if (data.Length == etxIndex + 1)
                {
                    break;
                }

                data = data.Skip(etxIndex + 1).ToArray();
                stxIndex = -1;
                etxIndex = -1;
            }
        }

        /// <summary>
        /// Check if a message buffer is valid, to be turned into a message object
        /// </summary>
        /// <param name="msg">The buffer</param>
        /// <returns>Whether the message is valid or not</returns>
        public static string ValidateMessageBytes(byte[] msg)
        {
            if (msg.Length < 6)
            {
                return "Message too short";
            }

            if (msg[0] != Stx)
            {
                return "Message didn't start with STX";
            }

            if (msg[msg.Length - 1] != Etx)
            {
                return "Message didn't end with ETX";
            }

            if (!Enum.IsDefined(typeof(Command), msg[3]))
            {
                return "Unknown command";
            }

            if (msg[msg.Length - 2] != CalculateCRC(msg))
            {
                return "CRC failed";
            }

            return null;
        }

        /// <summary>
        /// Calculate the CRC of a message, skipping start and end markers, and the actual CRC in the message.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static byte CalculateCRC(byte[] data)
        {
            if (data.Length < 3)
            {
                return 0;
            }
            else
            {
                ArraySegment<byte> payloadData = new ArraySegment<byte>(data, 1, data.Length - 3);
                return Crc.Calculate(payloadData.ToArray());
            }
        }

        /// <summary>
        /// This method is used to determine what variable type is used
        /// </summary>
        /// <param name="type">The variable type</param>
        /// <returns>The found VariableType</returns>
        public static VariableType FindVariableType(string type)
        {
            // Remove some keywords, which are not used to determine the type
            type = type.Replace("volatile ", "").Replace("pointer to ", "").Replace("const ", "").Trim();
            switch (type)
            {
                case "BIT":
                case "BOOL":
                    return VariableType.Bool;
                case "U8":
                case "uint8_t":
                case "BYTE":
                case "UINT8":
                case "unsigned char":
                    return VariableType.UChar;
                case "U16":
                case "uint16_t":
                case "UINT16":
                case "unsigned short":
                case "unsigned short int":
                    return VariableType.UShort;
                case "U32":
                case "uint32_t":
                case "UINT32":
                case "UINT":
                case "unsigned":
                case "unsigned int":
                    return VariableType.UInt;
                case "U64":
                case "uint64_t":
                case "UINT64":
                case "unsigned long":
                case "unsigned long int":
                    return VariableType.ULong;
                case "unsigned long long":
                case "unsigned long long int":
                    return VariableType.ULong;
                case "S8":
                case "signed char":
                    return VariableType.SChar;
                case "S16":
                case "short":
                case "short int":
                case "signed short":
                case "signed short int":
                    return VariableType.Short;
                case "S32":
                case "int":
                case "INT":
                case "signed":
                case "signed int":
                    return VariableType.Int;
                case "S64":
                case "long":
                case "long int ":
                case "signed long":
                case "signed long int":
                    return VariableType.Long;
                case "long long ":
                case "long long int ":
                case "signed long long":
                case "signed long long int":
                    return VariableType.Long;
                case "CHAR":
                case "char":
                    return VariableType.Char;
                case "float":
                    return VariableType.Float;
            }
            // If we cannot find a suitable variable type, set it to unknown
            return VariableType.Unknown;
        }

        /// <summary>
        /// This method takes a couple of values and produces one byte of ctrl information
        /// For more information, see the Embedded Debugger Protocol
        /// </summary>
        /// <param name="readWrite">Whether this is read or write</param>
        /// <param name="source">The source of the register</param>
        /// <param name="derefDepth">The depth to dereference</param>
        /// <returns>The ctrl byte</returns>
        public static byte GetControlByte(Version version, ReadWrite readWrite, Source source, int derefDepth)
        {
            if (version < new Version(0, 7, 0))
            {
                return GetControlByteV07(readWrite, source, derefDepth);
            }
            else if (version == new Version(0, 8, 0))
            {
                return GetControlByteV08(readWrite, derefDepth);
            }
            else if (version == new Version(1, 0, 0))
            {
                return GetControlByteV10(readWrite, derefDepth);
            }
            else
            {
                throw new NotSupportedException("Version not supported for embedded configuration");
            }
        }

        private static byte GetControlByteV07(ReadWrite readWrite, Source source, int derefDepth)
        {
            byte returnable = 0x00;
            returnable |= (byte)(readWrite == ReadWrite.Write ? 0b1000_0000 : 0b0000_0000);
            returnable |= (byte)source;
            returnable |= (byte)((byte)derefDepth & 0x0F);
            return returnable;
        }

        private static byte GetControlByteV08(ReadWrite readWrite, int derefDepth)
        {
            byte returnable = 0x00;
            returnable |= (byte)((byte)derefDepth & 0x0F);
            returnable |= (byte)((int)readWrite << 6);
            return returnable;
        }

        private static byte GetControlByteV10(ReadWrite readWrite, int derefDepth)
        {
            byte returnable = 0x00;
            returnable |= (byte)((byte)derefDepth & 0x0F);
            returnable |= (byte)((int)readWrite << 6);
            return returnable;
        }


        public static void GetControlByteValues(Version version, byte controlByte, out ReadWrite readWrite, out Source source, out int derefDepth)
        {
            if (version < new Version(0, 7, 0))
            {
                GetControlByteValuesV07(controlByte, out readWrite, out source, out derefDepth);
            }
            else if (version == new Version(0, 8, 0))
            {
                GetControlByteValuesV08(controlByte, out readWrite, out derefDepth);
                source = Source.ElfParsed;
            }
            else if (version == new Version(1, 0, 0))
            {
                GetControlByteValuesV10(controlByte, out readWrite, out derefDepth);
                source = Source.ElfParsed;
            }
            else
            {
                throw new NotSupportedException("Version not supported for embedded configuration");
            }
        }

        private static void GetControlByteValuesV07(byte controlByte, out ReadWrite readWrite, out Source source, out int derefDepth)
        {
            derefDepth = 0x0F & controlByte;
            readWrite = (controlByte >> 7) == 1 ? ReadWrite.Write : ReadWrite.Read;
            source = (Source)(0b0111_0000 & controlByte);
        }

        private static void GetControlByteValuesV08(byte controlByte, out ReadWrite readWrite, out int derefdepth)
        {
            derefdepth = 0x0F & controlByte;
            readWrite = (ReadWrite)((controlByte >> 6) & 0b0000_0011);
        }

        private static void GetControlByteValuesV10(byte controlByte, out ReadWrite readWrite, out int derefdepth)
        {
            derefdepth = 0x0F & controlByte;
            readWrite = (ReadWrite)((controlByte >> 6) & 0b0000_0011);
        }

        /// <summary>
        /// Add escape characters to a buffer
        /// </summary>
        /// <param name="input">The buffer to be escaped</param>
        /// <returns>The escaped buffer</returns>
        private static List<byte> AddEscapeCharacters(List<byte> input)
        {
            for (int i = input.Count - 1; i >= 0; i--)
            {
                if (input.ElementAt(i) == Etx || input.ElementAt(i) == Stx || input.ElementAt(i) == Esc)
                {
                    byte value = input.ElementAt(i);
                    input.RemoveAt(i);
                    input.Insert(i, (byte)(Esc ^ value));
                    input.Insert(i, Esc);
                }
            }
            return input;
        }

        /// <summary>
        /// Remove the escaped characters and replace them with the original ones
        /// </summary>
        /// <param name="input">The escaped buffer</param>
        /// <returns>The buffer without escape characters</returns>
        private static List<byte> ReplaceEscapeCharacters(List<byte> input)
        {
            for (int i = 1; i < input.Count; i++)
            {
                if (input.ElementAt(i) == Esc)
                {
                    input.RemoveAt(i);
                    byte value = (byte)(Esc ^ input.ElementAt(i));
                    input.RemoveAt(i);
                    input.Insert(i, value);
                }
            }
            return input;
        }
    }
}
