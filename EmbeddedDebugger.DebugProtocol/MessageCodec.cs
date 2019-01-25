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
using System.Text;
using System.Threading.Tasks;

namespace EmbeddedDebugger.DebugProtocol
{
    /// <summary>
    /// This class is used to encode and decode messages used with the Protocol
    /// The protocol is not explained here, see the .pdf file
    /// </summary>
    public static class MessageCodec
    {
        private const byte STX = 0x55;
        private const byte ETX = 0xAA;
        private const byte ESC = 0x66;

        private static readonly byte[] crcTable =
        {
             0,  94, 188, 226,  97,  63, 221, 131, 194, 156, 126,  32, 163, 253,  31,  65,
           157, 195,  33, 127, 252, 162,  64,  30,  95,   1, 227, 189,  62,  96, 130, 220,
            35, 125, 159, 193,  66,  28, 254, 160, 225, 191,  93,   3, 128, 222,  60,  98,
           190, 224,   2,  92, 223, 129,  99,  61, 124,  34, 192, 158,  29,  67, 161, 255,
            70,  24, 250, 164,  39, 121, 155, 197, 132, 218,  56, 102, 229, 187,  89,   7,
           219, 133, 103,  57, 186, 228,   6,  88,  25,  71, 165, 251, 120,  38, 196, 154,
           101,  59, 217, 135,   4,  90, 184, 230, 167, 249,  27,  69, 198, 152, 122,  36,
           248, 166,  68,  26, 153, 199,  37, 123,  58, 100, 134, 216,  91,   5, 231, 185,
           140, 210,  48, 110, 237, 179,  81,  15,  78,  16, 242, 172,  47, 113, 147, 205,
            17,  79, 173, 243, 112,  46, 204, 146, 211, 141, 111,  49, 178, 236,  14,  80,
           175, 241,  19,  77, 206, 144, 114,  44, 109,  51, 209, 143,  12,  82, 176, 238,
            50, 108, 142, 208,  83,  13, 239, 177, 240, 174,  76,  18, 145, 207,  45, 115,
           202, 148, 118,  40, 171, 245,  23,  73,   8,  86, 180, 234, 105,  55, 213, 139,
            87,   9, 235, 181,  54, 104, 138, 212, 149, 203,  41, 119, 244, 170,  72,  22,
           233, 183,  85,  11, 136, 214,  52, 106,  43, 117, 151, 201,  74,  20, 246, 168,
           116,  42, 200, 150,  21,  75, 169, 247, 182, 232,  10,  84, 215, 137, 107,  53
        };

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
                0x00,
                message.ControllerID,
                message.MsgID,
                (byte)message.Command
            };
            data.AddRange(message.CommandData);
            data.Add(0x00);
            data.Add(0x00);
            data.Add(CalculateCRC(data.ToArray()));
            data.RemoveAt(0);
            data.RemoveAt(data.Count - 2);
            data.RemoveAt(data.Count - 2);
            data = AddEscapeCharacters(data);
            data.Insert(0, STX);
            data.Add(ETX);
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

            int STXindex = -1;
            int ETXindex = -1;
            messages = new List<ProtocolMessage>();
            remainderOutput = new byte[0];
            ProtocolMessage msg;
            byte[] msgBytes;

            while (true)
            {
                for (int i = 0; i < data.Length; i++)
                {
                    if (data[i] == STX)
                    {
                        STXindex = i;
                    }
                    else if (data[i] == ETX && STXindex != -1)
                    {
                        ETXindex = i;
                        break;
                    }
                }
                if (STXindex >= 0 && ETXindex >= 0 && STXindex < data.Length && ETXindex < data.Length && STXindex + 4 < ETXindex)
                {
                    msgBytes = ReplaceEscapeCharacters(data.Skip(STXindex).Take(ETXindex - STXindex + 1).ToList()).ToArray();
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
                else if (ETXindex == -1)
                {
                    remainderOutput = data;
                    break;
                }

                if (data.Length == ETXindex + 1)
                {
                    break;
                }
                data = data.Skip(ETXindex + 1).ToArray();
                STXindex = -1;
                ETXindex = -1;
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
            if (msg[0] != STX)
            {
                return "Message didn't start with STX";
            }
            if (msg[msg.Length - 1] != ETX)
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
        /// Calculate the CRC (Cyclic Redundancy Check)
        /// </summary>
        /// <param name="data">Data to add to the crc</param>
        /// <returns>CRC value</returns>
        public static byte CalculateCRC(byte[] data)
        {
            byte returnable = 0x00;
            for (int i = 1; i < data.Length - 2; i++)
            {
                returnable = crcTable[returnable ^ data[i]];
            }
            return returnable;
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
            } else if(version == new Version(0, 8, 0))
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
                if (input.ElementAt(i) == ETX || input.ElementAt(i) == STX || input.ElementAt(i) == ESC)
                {
                    byte value = input.ElementAt(i);
                    input.RemoveAt(i);
                    input.Insert(i, (byte)(ESC ^ value));
                    input.Insert(i, ESC);
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
                if (input.ElementAt(i) == ESC)
                {
                    input.RemoveAt(i);
                    byte value = (byte)(ESC ^ input.ElementAt(i));
                    input.RemoveAt(i);
                    input.Insert(i, value);
                }
            }
            return input;
        }
    }
}
