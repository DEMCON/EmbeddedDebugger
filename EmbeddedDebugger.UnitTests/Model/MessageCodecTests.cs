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
using EmbeddedDebugger.DebugProtocol;
using EmbeddedDebugger.DebugProtocol.Enums;
using EmbeddedDebugger.DebugProtocol.Messages;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EmbeddedDebugger.UnitTests.Model
{
    [TestClass]
    public class MessageCodecTests
    {
        private const byte STX = 0x55;
        private const byte ETX = 0xAA;
        private const byte ESC = 0x66;

        [TestMethod]
        public void EncodeMessage_CommandDataValidFalse_ByteArray()
        {
            ProtocolMessage msg = new ProtocolMessage(0x01, 0x84, Command.QueryRegister, new byte[] { 0x03, 0x05, 0x07 });

            byte[] output = MessageCodec.EncodeMessage(msg);
            byte crc = output[output.Length - 2];

            Assert.AreEqual(output[0], STX);
            Assert.AreEqual(output[1], msg.ControllerID);
            Assert.AreEqual(output[2], msg.MsgID);
            Assert.AreEqual(output[3], (byte)msg.Command);
            CollectionAssert.AreEqual(output.Skip(4).Take(msg.CommandData.Length).ToArray(), msg.CommandData);
            Assert.AreEqual(output[output.Length - 1], ETX);
            Assert.IsTrue(string.IsNullOrEmpty(MessageCodec.ValidateMessageBytes(output)));

            output[0] = 0x00;
            output[output.Length - 2] = 0x00;
            output[output.Length - 1] = 0x00;
            Assert.AreEqual(crc, MessageCodec.CalculateCRC(output));
        }

        [TestMethod]
        public void EncodeMessage_NoCommandDataValidFalse_ByteArray()
        {
            ProtocolMessage msg = new ProtocolMessage(0x01, 0x84, Command.QueryRegister);

            byte[] output = MessageCodec.EncodeMessage(msg);
            byte crc = output[output.Length - 2];

            Assert.AreEqual(output[0], STX);
            Assert.AreEqual(output[1], msg.ControllerID);
            Assert.AreEqual(output[2], msg.MsgID);
            Assert.AreEqual(output[3], (byte)msg.Command);
            Assert.AreEqual(output[output.Length - 1], ETX);
            Assert.IsTrue(string.IsNullOrEmpty(MessageCodec.ValidateMessageBytes(output)));

            output[0] = 0x00;
            output[output.Length - 2] = 0x00;
            output[output.Length - 1] = 0x00;
            Assert.AreEqual(crc, MessageCodec.CalculateCRC(output));
        }

        [TestMethod]
        public void EncodeMessage_CommandDataValidTrue_ByteArray()
        {
            ProtocolMessage msg = new ProtocolMessage(0x01, 0x84, Command.QueryRegister, new byte[] { 0x03, 0x05, 0x07 });

            byte[] output = MessageCodec.EncodeMessage(msg);
            byte crc = output[output.Length - 2];

            Assert.AreEqual(output[0], STX);
            Assert.AreEqual(output[1], msg.ControllerID);
            Assert.AreEqual(output[2], msg.MsgID);
            Assert.AreEqual(output[3], (byte)msg.Command);
            CollectionAssert.AreEqual(output.Skip(4).Take(msg.CommandData.Length).ToArray(), msg.CommandData);
            Assert.AreEqual(output[output.Length - 1], ETX);
            Assert.IsTrue(string.IsNullOrEmpty(MessageCodec.ValidateMessageBytes(output)));

            output[0] = 0x00;
            output[output.Length - 2] = 0x00;
            output[output.Length - 1] = 0x00;
            Assert.AreEqual(crc, MessageCodec.CalculateCRC(output));
        }

        [TestMethod]
        public void EncodeMessage_NoCommandDataValidTrue_ByteArray()
        {
            ProtocolMessage msg = new ProtocolMessage(0x01, 0x84, Command.QueryRegister, new byte[0]);

            byte[] output = MessageCodec.EncodeMessage(msg);
            byte crc = output[output.Length - 2];

            Assert.AreEqual(output[0], STX);
            Assert.AreEqual(output[1], msg.ControllerID);
            Assert.AreEqual(output[2], msg.MsgID);
            Assert.AreEqual(output[3], (byte)msg.Command);
            Assert.AreEqual(output[output.Length - 1], ETX);
            Assert.IsTrue(string.IsNullOrEmpty(MessageCodec.ValidateMessageBytes(output)));

            output[0] = 0x00;
            output[output.Length - 2] = 0x00;
            output[output.Length - 1] = 0x00;
            Assert.AreEqual(crc, MessageCodec.CalculateCRC(output));
        }

        [TestMethod]
        public void EncodeMessage_CommandDataEscapeCharacters()
        {
            ProtocolMessage msg = new ProtocolMessage(ESC, ESC, Command.ConfigChannel);

            byte[] output = MessageCodec.EncodeMessage(msg);
            byte crc = output[output.Length - 2];

            Assert.AreEqual(8, output.Length);
            Assert.AreEqual(ESC, output[1]);
            Assert.AreEqual(ESC, (byte)(ESC ^ output[2]));
            Assert.AreEqual(ESC, output[3]);
            Assert.AreEqual(ESC, (byte)(ESC ^ output[4]));
        }

        [TestMethod]
        public void EncodeMessage_CommandDataEscapeCharactersForSTXETX()
        {
            ProtocolMessage msg = new ProtocolMessage(STX, ETX, Command.ConfigChannel);

            byte[] output = MessageCodec.EncodeMessage(msg);
            byte crc = output[output.Length - 2];

            Assert.AreEqual(8, output.Length);
            Assert.AreEqual(ESC, output[1]);
            Assert.AreEqual(STX, (byte)(ESC ^ output[2]));
            Assert.AreEqual(ESC, output[3]);
            Assert.AreEqual(ETX, (byte)(ESC ^ output[4]));
        }

        [TestMethod]
        public void DecodeMessage_OneMessageNoCommandData_NoRemainder()
        {
            byte controllerID = 0x01;
            byte msgID = 0x34;
            byte cmd = (byte)Command.GetVersion;
            byte[] inputData = new byte[] { 0x00, controllerID, msgID, cmd, 0x00, 0x00 };
            inputData[inputData.Length - 2] = MessageCodec.CalculateCRC(inputData);
            inputData[0] = STX;
            inputData[inputData.Length - 1] = ETX;

            MessageCodec.DecodeMessages(inputData, out List<ProtocolMessage> msgs, out byte[] outputRemainder);
            Assert.IsTrue(msgs.Count == 1);

            ProtocolMessage msg = msgs.First();
            Assert.AreEqual(msg.ControllerID, controllerID);
            Assert.AreEqual(msg.MsgID, msgID);
            Assert.AreEqual((byte)msg.Command, cmd);
            CollectionAssert.AreEqual(new byte[0], outputRemainder);
        }

        [TestMethod]
        public void DecodeMessage_OneMessageCommandData_NoRemainder()
        {
            byte controllerID = 0x01;
            byte msgID = 0x34;
            byte cmd = (byte)Command.GetVersion;
            byte[] commandData = new byte[] { 0x49, 0x03, 0x92, 0x96, 0x00 };
            byte[] inputData = new byte[] { 0x00, controllerID, msgID, cmd, 0x00, 0x00 };
            List<byte> dataList = inputData.ToList();
            dataList.InsertRange(4, commandData);
            inputData = dataList.ToArray();
            inputData[inputData.Length - 2] = MessageCodec.CalculateCRC(inputData);
            inputData[0] = STX;
            inputData[inputData.Length - 1] = ETX;

            MessageCodec.DecodeMessages(inputData, out List<ProtocolMessage> msgs, out byte[] outputRemainder);
            Assert.IsTrue(msgs.Count == 1);

            ProtocolMessage msg = msgs.First();
            Assert.AreEqual(msg.ControllerID, controllerID);
            Assert.AreEqual(msg.MsgID, msgID);
            Assert.AreEqual((byte)msg.Command, cmd);
            CollectionAssert.AreEqual(commandData, msg.CommandData);
            CollectionAssert.AreEqual(new byte[0], outputRemainder);
        }
        // TODO: Add tests for the decodemessage method
        [TestMethod]
        public void DecodeMessage_OneMessageCommandNoData_Remainder()
        {
            byte controllerID = 0x01;
            byte msgID = 0x34;
            byte cmd = (byte)Command.GetVersion;
            byte[] commandData = new byte[0];
            byte[] inputData = new byte[] { 0x00, controllerID, msgID, cmd, 0x00, 0x00 };
            List<byte> dataList = inputData.ToList();
            dataList.InsertRange(4, commandData);
            inputData = dataList.ToArray();
            inputData[inputData.Length - 2] = MessageCodec.CalculateCRC(inputData);
            inputData[0] = STX;
            inputData[inputData.Length - 1] = ETX;
            dataList = inputData.ToList();
            dataList.AddRange(new byte[] { 0xFF, 0xFF, 0xFF, 0xFF });
            inputData = dataList.ToArray();

            MessageCodec.DecodeMessages(inputData, out List<ProtocolMessage> msgs, out byte[] outputRemainder);
            Assert.IsTrue(msgs.Count == 1);

            ProtocolMessage msg = msgs.First();
            Assert.AreEqual(msg.ControllerID, controllerID);
            Assert.AreEqual(msg.MsgID, msgID);
            Assert.AreEqual((byte)msg.Command, cmd);
            CollectionAssert.AreEqual(commandData, msg.CommandData);
            CollectionAssert.AreEqual(new byte[] { 0xFF, 0xFF, 0xFF, 0xFF }, outputRemainder);
        }

        [TestMethod]
        public void DecodeMessage_OneMessageCommandData_Remainder()
        {
            byte controllerID = 0x01;
            byte msgID = 0x34;
            byte cmd = (byte)Command.GetVersion;
            byte[] commandData = new byte[] { 0x49, 0x03, 0x92, 0x96, 0x00 };
            byte[] inputData = new byte[] { 0x00, controllerID, msgID, cmd, 0x00, 0x00 };
            List<byte> dataList = inputData.ToList();
            dataList.InsertRange(4, commandData);
            inputData = dataList.ToArray();
            inputData[inputData.Length - 2] = MessageCodec.CalculateCRC(inputData);
            inputData[0] = STX;
            inputData[inputData.Length - 1] = ETX;
            dataList = inputData.ToList();
            dataList.AddRange(new byte[] { 0xFF, 0xFF, 0xFF, 0xFF });
            inputData = dataList.ToArray();

            MessageCodec.DecodeMessages(inputData, out List<ProtocolMessage> msgs, out byte[] outputRemainder);
            Assert.IsTrue(msgs.Count == 1);

            ProtocolMessage msg = msgs.First();
            Assert.AreEqual(msg.ControllerID, controllerID);
            Assert.AreEqual(msg.MsgID, msgID);
            Assert.AreEqual((byte)msg.Command, cmd);
            CollectionAssert.AreEqual(commandData, msg.CommandData);
            CollectionAssert.AreEqual(new byte[] { 0xFF, 0xFF, 0xFF, 0xFF }, outputRemainder);
        }

        [TestMethod]
        public void DecodeMessage_OneMessageCommandData_Front()
        {
            byte controllerID = 0x01;
            byte msgID = 0x34;
            byte cmd = (byte)Command.GetVersion;
            byte[] commandData = new byte[] { 0x49, 0x03, 0x92, 0x96, 0x00 };
            byte[] inputData = new byte[] { 0x00, controllerID, msgID, cmd, 0x00, 0x00 };
            List<byte> dataList = inputData.ToList();
            dataList.InsertRange(4, commandData);
            inputData = dataList.ToArray();
            inputData[inputData.Length - 2] = MessageCodec.CalculateCRC(inputData);
            inputData[0] = STX;
            inputData[inputData.Length - 1] = ETX;
            dataList = inputData.ToList();
            //dataList.AddRange(new byte[] { 0xFF, 0xFF, 0xFF, 0xFF });
            dataList.InsertRange(0, new byte[] { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF });
            inputData = dataList.ToArray();

            MessageCodec.DecodeMessages(inputData, out List<ProtocolMessage> msgs, out byte[] outputRemainder);
            Assert.IsTrue(msgs.Count == 1);

            ProtocolMessage msg = msgs.First();
            Assert.AreEqual(msg.ControllerID, controllerID);
            Assert.AreEqual(msg.MsgID, msgID);
            Assert.AreEqual((byte)msg.Command, cmd);
            CollectionAssert.AreEqual(commandData, msg.CommandData);
            CollectionAssert.AreEqual(new byte[0], outputRemainder);
        }

        [TestMethod]
        public void DecodeMessage_OneMessageCommandData_RemainderFront()
        {
            byte controllerID = 0x01;
            byte msgID = 0x34;
            byte cmd = (byte)Command.GetVersion;
            byte[] commandData = new byte[] { 0x49, 0x03, 0x92, 0x96, 0x00 };
            byte[] inputData = new byte[] { 0x00, controllerID, msgID, cmd, 0x00, 0x00 };
            List<byte> dataList = inputData.ToList();
            dataList.InsertRange(4, commandData);
            inputData = dataList.ToArray();
            inputData[inputData.Length - 2] = MessageCodec.CalculateCRC(inputData);
            inputData[0] = STX;
            inputData[inputData.Length - 1] = ETX;
            dataList = inputData.ToList();
            dataList.AddRange(new byte[] { 0xFF, 0xFF, 0xFF, 0xFF });
            dataList.InsertRange(0, new byte[] { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF });
            inputData = dataList.ToArray();

            MessageCodec.DecodeMessages(inputData, out List<ProtocolMessage> msgs, out byte[] outputRemainder);
            Assert.IsTrue(msgs.Count == 1);

            ProtocolMessage msg = msgs.First();
            Assert.AreEqual(msg.ControllerID, controllerID);
            Assert.AreEqual(msg.MsgID, msgID);
            Assert.AreEqual((byte)msg.Command, cmd);
            CollectionAssert.AreEqual(commandData, msg.CommandData);
            CollectionAssert.AreEqual(new byte[] { 0xFF, 0xFF, 0xFF, 0xFF }, outputRemainder);
        }

        [TestMethod]
        public void DecodeMessage_TwoMessagesSameStream_NoFrontNoRemainder()
        {
            byte controllerID = 0x01;
            byte msgID = 0x34;
            byte cmd = (byte)Command.GetVersion;
            byte[] commandData = new byte[] { 0x49, 0x03, 0x92, 0x96, 0x00 };
            byte[] inputData = new byte[] { 0x00, controllerID, msgID, cmd, 0x00, 0x00 };
            List<byte> dataList = inputData.ToList();
            dataList.InsertRange(4, commandData);
            inputData = dataList.ToArray();
            inputData[inputData.Length - 2] = MessageCodec.CalculateCRC(inputData);
            inputData[0] = STX;
            inputData[inputData.Length - 1] = ETX;
            dataList = inputData.ToList();
            dataList.AddRange(inputData);
            inputData = dataList.ToArray();

            MessageCodec.DecodeMessages(inputData, out List<ProtocolMessage> msgs, out byte[] outputRemainder);
            Assert.IsTrue(msgs.Count == 2);

            foreach (ProtocolMessage msg in msgs)
            {
                Assert.AreEqual(msg.ControllerID, controllerID);
                Assert.AreEqual(msg.MsgID, msgID);
                Assert.AreEqual((byte)msg.Command, cmd);
                CollectionAssert.AreEqual(commandData, msg.CommandData);
            }
            CollectionAssert.AreEqual(new byte[0], outputRemainder);
        }

        [TestMethod]
        public void DecodeMessage_TwoMessagesSameStream_FrontNoRemainder()
        {
            byte controllerID = 0x01;
            byte msgID = 0x34;
            byte cmd = (byte)Command.GetVersion;
            byte[] commandData = new byte[] { 0x49, 0x03, 0x92, 0x96, 0x00 };
            byte[] inputData = new byte[] { 0x00, controllerID, msgID, cmd, 0x00, 0x00 };
            List<byte> dataList = inputData.ToList();
            dataList.InsertRange(4, commandData);
            inputData = dataList.ToArray();
            inputData[inputData.Length - 2] = MessageCodec.CalculateCRC(inputData);
            inputData[0] = STX;
            inputData[inputData.Length - 1] = ETX;
            dataList = inputData.ToList();
            dataList.AddRange(inputData);
            dataList.InsertRange(0, new byte[] { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF });
            inputData = dataList.ToArray();

            MessageCodec.DecodeMessages(inputData, out List<ProtocolMessage> msgs, out byte[] outputRemainder);
            Assert.IsTrue(msgs.Count == 2);

            foreach (ProtocolMessage msg in msgs)
            {
                Assert.AreEqual(msg.ControllerID, controllerID);
                Assert.AreEqual(msg.MsgID, msgID);
                Assert.AreEqual((byte)msg.Command, cmd);
                CollectionAssert.AreEqual(commandData, msg.CommandData);
            }
            CollectionAssert.AreEqual(new byte[0], outputRemainder);
        }

        [TestMethod]
        public void DecodeMessage_TwoMessagesSameStream_FrontRemainder()
        {
            byte controllerID = 0x01;
            byte msgID = 0x34;
            byte cmd = (byte)Command.GetVersion;
            byte[] commandData = new byte[] { 0x49, 0x03, 0x92, 0x96, 0x00 };
            byte[] inputData = new byte[] { 0x00, controllerID, msgID, cmd, 0x00, 0x00 };
            List<byte> dataList = inputData.ToList();
            dataList.InsertRange(4, commandData);
            inputData = dataList.ToArray();
            inputData[inputData.Length - 2] = MessageCodec.CalculateCRC(inputData);
            inputData[0] = STX;
            inputData[inputData.Length - 1] = ETX;
            dataList = inputData.ToList();
            dataList.AddRange(inputData);
            dataList.AddRange(new byte[] { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF });
            inputData = dataList.ToArray();

            MessageCodec.DecodeMessages(inputData, out List<ProtocolMessage> msgs, out byte[] outputRemainder);
            Assert.IsTrue(msgs.Count == 2);

            foreach (ProtocolMessage msg in msgs)
            {
                Assert.AreEqual(msg.ControllerID, controllerID);
                Assert.AreEqual(msg.MsgID, msgID);
                Assert.AreEqual((byte)msg.Command, cmd);
                CollectionAssert.AreEqual(commandData, msg.CommandData);
            }
            CollectionAssert.AreEqual(new byte[] { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF }, outputRemainder);
        }

        [TestMethod]
        public void DecodeMessage_TwoMessagesSameStream_GarbageInBetween()
        {
            byte controllerID = 0x01;
            byte msgID = 0x34;
            byte cmd = (byte)Command.GetVersion;
            byte[] commandData = new byte[] { 0x49, 0x03, 0x92, 0x96, 0x00 };
            byte[] inputData = new byte[] { 0x00, controllerID, msgID, cmd, 0x00, 0x00 };
            List<byte> dataList = inputData.ToList();
            dataList.InsertRange(4, commandData);
            inputData = dataList.ToArray();
            inputData[inputData.Length - 2] = MessageCodec.CalculateCRC(inputData);
            inputData[0] = STX;
            inputData[inputData.Length - 1] = ETX;
            dataList = inputData.ToList();
            dataList.AddRange(new byte[] { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF });
            dataList.AddRange(inputData);
            inputData = dataList.ToArray();

            MessageCodec.DecodeMessages(inputData, out List<ProtocolMessage> msgs, out byte[] outputRemainder);
            Assert.IsTrue(msgs.Count == 2);

            foreach (ProtocolMessage msg in msgs)
            {
                Assert.AreEqual(msg.ControllerID, controllerID);
                Assert.AreEqual(msg.MsgID, msgID);
                Assert.AreEqual((byte)msg.Command, cmd);
                CollectionAssert.AreEqual(commandData, msg.CommandData);
            }
            CollectionAssert.AreEqual(new byte[0], outputRemainder);
        }

        [TestMethod]
        public void DecodeMessage_TwoMessagesDifferentStream_NoFrontNoRemainder()
        {
            byte controllerID = 0x01;
            byte msgID = 0x34;
            byte cmd = (byte)Command.GetVersion;
            byte[] commandData = new byte[] { 0x49, 0x03, 0x92, 0x96, 0x00 };
            byte[] inputData = new byte[] { 0x00, controllerID, msgID, cmd, 0x00, 0x00 };
            List<byte> dataList = inputData.ToList();
            dataList.InsertRange(4, commandData);
            inputData = dataList.ToArray();
            inputData[inputData.Length - 2] = MessageCodec.CalculateCRC(inputData);
            inputData[0] = STX;
            inputData[inputData.Length - 1] = ETX;
            List<ProtocolMessage> myMsgs = new List<ProtocolMessage>();

            MessageCodec.DecodeMessages(inputData, out List<ProtocolMessage> msgs, out byte[] outputRemainder);
            myMsgs.AddRange(msgs);
            MessageCodec.DecodeMessages(inputData, out List<ProtocolMessage> msgs2, out byte[] outputRemainder2, outputRemainder);
            myMsgs.AddRange(msgs2);

            Assert.IsTrue(myMsgs.Count == 2);
            foreach (ProtocolMessage msg in myMsgs)
            {
                Assert.AreEqual(msg.ControllerID, controllerID);
                Assert.AreEqual(msg.MsgID, msgID);
                Assert.AreEqual((byte)msg.Command, cmd);
                CollectionAssert.AreEqual(commandData, msg.CommandData);
            }
            CollectionAssert.AreEqual(new byte[0], outputRemainder);
            CollectionAssert.AreEqual(new byte[0], outputRemainder2);
        }

        [TestMethod]
        public void DecodeMessage_TwoMessagesDifferentStream_FrontNoRemainder()
        {
            byte controllerID = 0x01;
            byte msgID = 0x34;
            byte cmd = (byte)Command.GetVersion;
            byte[] commandData = new byte[] { 0x49, 0x03, 0x92, 0x96, 0x00 };
            byte[] inputData = new byte[] { 0x00, controllerID, msgID, cmd, 0x00, 0x00 };
            List<byte> dataList = inputData.ToList();
            dataList.InsertRange(4, commandData);
            inputData = dataList.ToArray();
            inputData[inputData.Length - 2] = MessageCodec.CalculateCRC(inputData);
            inputData[0] = STX;
            inputData[inputData.Length - 1] = ETX;
            dataList = inputData.ToList();
            dataList.InsertRange(0, new byte[] { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF });
            inputData = dataList.ToArray();
            List<ProtocolMessage> myMsgs = new List<ProtocolMessage>();

            MessageCodec.DecodeMessages(inputData, out List<ProtocolMessage> msgs, out byte[] outputRemainder);
            myMsgs.AddRange(msgs);
            MessageCodec.DecodeMessages(inputData, out List<ProtocolMessage> msgs2, out byte[] outputRemainder2, outputRemainder);
            myMsgs.AddRange(msgs2);

            Assert.IsTrue(myMsgs.Count == 2);
            foreach (ProtocolMessage msg in myMsgs)
            {
                Assert.AreEqual(msg.ControllerID, controllerID);
                Assert.AreEqual(msg.MsgID, msgID);
                Assert.AreEqual((byte)msg.Command, cmd);
                CollectionAssert.AreEqual(commandData, msg.CommandData);
            }
            CollectionAssert.AreEqual(new byte[0], outputRemainder);
            CollectionAssert.AreEqual(new byte[0], outputRemainder2);
        }

        [TestMethod]
        public void DecodeMessage_TwoMessagesDifferentStream_FrontRemainder()
        {
            byte controllerID = 0x01;
            byte msgID = 0x34;
            byte cmd = (byte)Command.GetVersion;
            byte[] commandData = new byte[] { 0x49, 0x03, 0x92, 0x96, 0x00 };
            byte[] inputData = new byte[] { 0x00, controllerID, msgID, cmd, 0x00, 0x00 };
            List<byte> dataList = inputData.ToList();
            dataList.InsertRange(4, commandData);
            inputData = dataList.ToArray();
            inputData[inputData.Length - 2] = MessageCodec.CalculateCRC(inputData);
            inputData[0] = STX;
            inputData[inputData.Length - 1] = ETX;
            dataList = inputData.ToList();
            dataList.InsertRange(0, new byte[] { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF });
            dataList.AddRange(new byte[] { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF });
            inputData = dataList.ToArray();
            List<ProtocolMessage> myMsgs = new List<ProtocolMessage>();

            MessageCodec.DecodeMessages(inputData, out List<ProtocolMessage> msgs, out byte[] outputRemainder);
            myMsgs.AddRange(msgs);
            MessageCodec.DecodeMessages(inputData, out List<ProtocolMessage> msgs2, out byte[] outputRemainder2, outputRemainder);
            myMsgs.AddRange(msgs2);

            Assert.IsTrue(myMsgs.Count == 2);
            foreach (ProtocolMessage msg in myMsgs)
            {
                Assert.AreEqual(msg.ControllerID, controllerID);
                Assert.AreEqual(msg.MsgID, msgID);
                Assert.AreEqual((byte)msg.Command, cmd);
                CollectionAssert.AreEqual(commandData, msg.CommandData);
            }
            CollectionAssert.AreEqual(new byte[] { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF }, outputRemainder);
            CollectionAssert.AreEqual(new byte[] { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF }, outputRemainder2);
        }

        [TestMethod]
        public void DecodeMessage_OneMessageDifferentStream()
        {
            byte controllerID = 0x01;
            byte msgID = 0x34;
            byte cmd = (byte)Command.GetVersion;
            byte[] commandData = new byte[] { 0x49, 0x03, 0x92, 0x96, 0x00 };
            byte[] inputData = new byte[] { 0x00, controllerID, msgID, cmd, 0x00, 0x00 };
            List<byte> dataList = inputData.ToList();
            dataList.InsertRange(4, commandData);
            inputData = dataList.ToArray();
            inputData[inputData.Length - 2] = MessageCodec.CalculateCRC(inputData);
            inputData[0] = STX;
            inputData[inputData.Length - 1] = ETX;
            List<ProtocolMessage> myMsgs = new List<ProtocolMessage>();

            MessageCodec.DecodeMessages(inputData.Take(4).ToArray(), out List<ProtocolMessage> msgs, out byte[] outputRemainder);
            myMsgs.AddRange(msgs);
            MessageCodec.DecodeMessages(inputData.Skip(4).ToArray(), out List<ProtocolMessage> msgs2, out byte[] outputRemainder2, outputRemainder);
            myMsgs.AddRange(msgs2);

            Assert.IsTrue(myMsgs.Count == 1);
            foreach (ProtocolMessage msg in myMsgs)
            {
                Assert.AreEqual(msg.ControllerID, controllerID);
                Assert.AreEqual(msg.MsgID, msgID);
                Assert.AreEqual((byte)msg.Command, cmd);
                CollectionAssert.AreEqual(commandData, msg.CommandData);
            }
            CollectionAssert.AreEqual(inputData.Take(4).ToArray(), outputRemainder);
            CollectionAssert.AreEqual(new byte[0], outputRemainder2);
        }

        [TestMethod]
        public void DecodeMessage_TwoMessagesThreeDifferentStream()
        {
            byte controllerID = 0x01;
            byte msgID = 0x34;
            byte cmd = (byte)Command.GetVersion;
            byte[] commandData = new byte[] { 0x49, 0x03, 0x92, 0x96, 0x00 };
            byte[] inputData = new byte[] { 0x00, controllerID, msgID, cmd, 0x00, 0x00 };
            List<byte> dataList = inputData.ToList();
            dataList.InsertRange(4, commandData);
            inputData = dataList.ToArray();
            inputData[inputData.Length - 2] = MessageCodec.CalculateCRC(inputData);
            inputData[0] = STX;
            inputData[inputData.Length - 1] = ETX;
            List<ProtocolMessage> myMsgs = new List<ProtocolMessage>();
            byte[] firstStream = inputData.Take(4).ToArray();
            List<byte> secondStream = inputData.Skip(4).ToList();
            secondStream.AddRange(inputData.Take(5));
            byte[] thirdStream = inputData.Skip(5).ToArray();

            MessageCodec.DecodeMessages(firstStream, out List<ProtocolMessage> msgs, out byte[] outputRemainder);
            myMsgs.AddRange(msgs);
            MessageCodec.DecodeMessages(secondStream.ToArray(), out List<ProtocolMessage> msgs2, out byte[] outputRemainder2, outputRemainder);
            myMsgs.AddRange(msgs2);
            MessageCodec.DecodeMessages(thirdStream, out List<ProtocolMessage> msgs3, out byte[] outputRemainder3, outputRemainder2);
            myMsgs.AddRange(msgs3);

            Assert.IsTrue(myMsgs.Count == 2);
            foreach (ProtocolMessage msg in myMsgs)
            {
                Assert.AreEqual(msg.ControllerID, controllerID);
                Assert.AreEqual(msg.MsgID, msgID);
                Assert.AreEqual((byte)msg.Command, cmd);
                CollectionAssert.AreEqual(commandData, msg.CommandData);
            }
            CollectionAssert.AreEqual(inputData.Take(4).ToArray(), outputRemainder);
            CollectionAssert.AreEqual(inputData.Take(5).ToArray(), outputRemainder2);
            CollectionAssert.AreEqual(new byte[0], outputRemainder3);
        }

        [TestMethod]
        public void DecodeMessage_OneMessageThreeDifferentStream()
        {
            byte controllerID = 0x01;
            byte msgID = 0x34;
            byte cmd = (byte)Command.GetVersion;
            byte[] commandData = new byte[] { 0x49, 0x03, 0x92, 0x96, 0x00 };
            byte[] inputData = new byte[] { 0x00, controllerID, msgID, cmd, 0x00, 0x00 };
            List<byte> dataList = inputData.ToList();
            dataList.InsertRange(4, commandData);
            inputData = dataList.ToArray();
            inputData[inputData.Length - 2] = MessageCodec.CalculateCRC(inputData);
            inputData[0] = STX;
            inputData[inputData.Length - 1] = ETX;
            List<ProtocolMessage> myMsgs = new List<ProtocolMessage>();
            byte[] firstStream = inputData.Take(4).ToArray();
            byte[] secondStream = inputData.Skip(4).Take(3).ToArray();
            byte[] thirdStream = inputData.Skip(7).ToArray();

            MessageCodec.DecodeMessages(firstStream, out List<ProtocolMessage> msgs, out byte[] outputRemainder);
            myMsgs.AddRange(msgs);
            MessageCodec.DecodeMessages(secondStream, out List<ProtocolMessage> msgs2, out byte[] outputRemainder2, outputRemainder);
            myMsgs.AddRange(msgs2);
            MessageCodec.DecodeMessages(thirdStream, out List<ProtocolMessage> msgs3, out byte[] outputRemainder3, outputRemainder2);
            myMsgs.AddRange(msgs3);

            Assert.IsTrue(myMsgs.Count == 1);
            foreach (ProtocolMessage msg in myMsgs)
            {
                Assert.AreEqual(msg.ControllerID, controllerID);
                Assert.AreEqual(msg.MsgID, msgID);
                Assert.AreEqual((byte)msg.Command, cmd);
                CollectionAssert.AreEqual(commandData, msg.CommandData);
            }
            CollectionAssert.AreEqual(inputData.Take(4).ToArray(), outputRemainder);
            CollectionAssert.AreEqual(inputData.Take(7).ToArray(), outputRemainder2);
            CollectionAssert.AreEqual(new byte[0], outputRemainder3);
        }

        [TestMethod]
        public void DecodeMessage_OneMessage_UnexpectedSTX()
        {
            byte controllerID = 0x01;
            byte msgID = 0x34;
            byte cmd = (byte)Command.GetVersion;
            byte[] commandData = new byte[] { 0x49, STX, 0x67, 0x96, 0x00 };
            byte[] inputData = new byte[] { 0x00, controllerID, msgID, cmd, 0x00, 0x00 };
            List<byte> dataList = inputData.ToList();
            dataList.InsertRange(4, commandData);
            inputData = dataList.ToArray();
            inputData[inputData.Length - 2] = MessageCodec.CalculateCRC(inputData);
            inputData[0] = STX;
            inputData[inputData.Length - 1] = ETX;

            MessageCodec.DecodeMessages(inputData, out List<ProtocolMessage> msgs, out byte[] outputRemainder);
            Assert.IsFalse(msgs.First().Valid);
        }

        [TestMethod]
        public void DecodeMessage_DecodeEncodedMessageNoData()
        {
            ProtocolMessage msg = new ProtocolMessage(0x04, 0x02, Command.ConfigChannel);

            byte[] encodedMsg = MessageCodec.EncodeMessage(msg);
            MessageCodec.DecodeMessages(encodedMsg, out List<ProtocolMessage> msgs, out byte[] remainder);

            Assert.IsTrue(msgs.Count == 1);
            Assert.IsTrue(remainder.Length == 0);
            Assert.AreEqual(msg.ControllerID, msgs.First().ControllerID);
            Assert.AreEqual(msg.MsgID, msgs.First().MsgID);
            Assert.AreEqual(msg.Command, msgs.First().Command);
            CollectionAssert.AreEqual(msg.CommandData, msgs.First().CommandData);
        }

        [TestMethod]
        public void DecodeMessage_DecodeEncodedMessageData()
        {
            ProtocolMessage msg = new ProtocolMessage(0x04, 0x02, Command.ConfigChannel, new byte[] { 0x43, 0x92, 0x02 });

            byte[] encodedMsg = MessageCodec.EncodeMessage(msg);
            MessageCodec.DecodeMessages(encodedMsg, out List<ProtocolMessage> msgs, out byte[] remainder);

            Assert.IsTrue(msgs.Count == 1);
            Assert.IsTrue(remainder.Length == 0);
            Assert.AreEqual(msg.ControllerID, msgs.First().ControllerID);
            Assert.AreEqual(msg.MsgID, msgs.First().MsgID);
            Assert.AreEqual(msg.Command, msgs.First().Command);
            CollectionAssert.AreEqual(msg.CommandData, msgs.First().CommandData);
        }

        [TestMethod]
        public void DecodeMessage_DecodeEncodedMessageDataEscaped()
        {
            ProtocolMessage msg = new ProtocolMessage(STX, 0x02, Command.ConfigChannel, new byte[] { ESC, STX, ETX });

            byte[] encodedMsg = MessageCodec.EncodeMessage(msg);
            MessageCodec.DecodeMessages(encodedMsg, out List<ProtocolMessage> msgs, out byte[] remainder);

            Assert.IsTrue(msgs.Count == 1);
            Assert.IsTrue(remainder.Length == 0);
            Assert.AreEqual(msg.ControllerID, msgs.First().ControllerID);
            Assert.AreEqual(msg.MsgID, msgs.First().MsgID);
            Assert.AreEqual(msg.Command, msgs.First().Command);
            CollectionAssert.AreEqual(msg.CommandData, msgs.First().CommandData);
        }

        [TestMethod]
        public void EncodeMessage_EncodeDecodedMessage_NormalData()
        {
            byte controllerID = 0x01;
            byte msgID = 0x34;
            byte cmd = (byte)Command.GetVersion;
            byte[] commandData = new byte[] { 0x49, 0x05, 0x67, 0x96, 0x00 };
            byte[] inputData = new byte[] { 0x00, controllerID, msgID, cmd, 0x00, 0x00 };
            List<byte> dataList = inputData.ToList();
            dataList.InsertRange(4, commandData);
            inputData = dataList.ToArray();
            inputData[inputData.Length - 2] = MessageCodec.CalculateCRC(inputData);
            inputData[0] = STX;
            inputData[inputData.Length - 1] = ETX;

            MessageCodec.DecodeMessages(inputData, out List<ProtocolMessage> msgs, out byte[] remainder);
            byte[] msg = MessageCodec.EncodeMessage(msgs.First());

            Assert.IsTrue(msgs.Count == 1);
            Assert.IsTrue(remainder.Length == 0);
            CollectionAssert.AreEqual(inputData, msg);
        }

        [TestMethod]
        public void DecodeMessage_WrongCRC()
        {
            byte controllerID = 0x01;
            byte msgID = 0x34;
            byte cmd = (byte)Command.GetVersion;
            byte[] commandData = new byte[] { 0x49, 0x66, 0xCC, 0x66, 0x00 };
            byte[] inputData = new byte[] { 0x00, controllerID, msgID, cmd, 0x00, 0x00 };
            List<byte> dataList = inputData.ToList();
            dataList.InsertRange(4, commandData);
            inputData = dataList.ToArray();
            inputData[inputData.Length - 2] = 0x11;
            inputData[0] = STX;
            inputData[inputData.Length - 1] = ETX;

            MessageCodec.DecodeMessages(inputData, out List<ProtocolMessage> msgs, out byte[] outputRemainder);

            Assert.IsFalse(msgs.First().Valid);
        }

        [TestMethod]
        public void DecodeMessage_MultipleEscapeCharactersEndingIn_f_x3()
        {
            byte controllerID = 0x01;
            byte msgID = 0x90;
            Command cmd = Command.DebugString;
            byte[] commandData = Encoding.UTF8.GetBytes("fff");
            byte[] expectedMsg = new byte[] { 0x00, controllerID, msgID, (byte)cmd,
                0x66, 0x00, 0x66, 0x00, 0x66, 0x00,
                0x00, 0x00 };
            byte[] expectedMsgNotEscaped = new byte[]{0x00, controllerID, msgID, (byte)cmd,
                0x66, 0x66, 0x66, 
                0x00, 0x00 };
            expectedMsg[expectedMsg.Length - 2] = MessageCodec.CalculateCRC(expectedMsgNotEscaped);
            expectedMsg[expectedMsg.Length - 1] = ETX;
            expectedMsg[0] = STX;

            byte[] msg = MessageCodec.EncodeMessage(new ProtocolMessage(controllerID, msgID, cmd, commandData));
            MessageCodec.DecodeMessages(msg, out List<ProtocolMessage> msgs, out byte[] outputRemainder);
            ProtocolMessage myMessage = msgs.First();

            Assert.AreEqual(expectedMsg.Length, msg.Length);
            CollectionAssert.AreEqual(expectedMsg, msg);
            CollectionAssert.AreEqual(commandData, myMessage.CommandData);
        }

        [TestMethod]
        public void DecodeMessage_MultipleEscapeCharactersEndingIn_f_x4()
        {
            byte controllerID = 0x01;
            byte msgID = 0x90;
            Command cmd = Command.DebugString;
            byte[] commandData = Encoding.UTF8.GetBytes("ffff");
            byte[] expectedMsg = new byte[] { 0x00, controllerID, msgID, (byte)cmd,
                0x66, 0x00, 0x66, 0x00, 0x66, 0x00, 0x66, 0x00,
                0x00, 0x00 };
            byte[] expectedMsgNotEscaped = new byte[]{0x00, controllerID, msgID, (byte)cmd,
                0x66, 0x66, 0x66, 0x66,
                0x00, 0x00 };
            expectedMsg[expectedMsg.Length - 2] = MessageCodec.CalculateCRC(expectedMsgNotEscaped);
            expectedMsg[expectedMsg.Length - 1] = ETX;
            expectedMsg[0] = STX;

            byte[] msg = MessageCodec.EncodeMessage(new ProtocolMessage(controllerID, msgID, cmd, commandData));
            MessageCodec.DecodeMessages(msg, out List<ProtocolMessage> msgs, out byte[] outputRemainder);
            ProtocolMessage myMessage = msgs.First();

            Assert.AreEqual(expectedMsg.Length, msg.Length);
            CollectionAssert.AreEqual(expectedMsg, msg);
            CollectionAssert.AreEqual(commandData, myMessage.CommandData);
        }

        [TestMethod]
        public void DecodeMessage_MultipleEscapeCharactersNotEndingIn_f_x4_EndIna()
        {
            byte controllerID = 0x01;
            byte msgID = 0x90;
            Command cmd = Command.DebugString;
            byte[] commandData = Encoding.UTF8.GetBytes("ffffa");
            byte[] expectedMsg = new byte[] { 0x00, controllerID, msgID, (byte)cmd,
                0x66, 0x00, 0x66, 0x00, 0x66, 0x00, 0x66, 0x00, 0x61,
                0x00, 0x00 };
            byte[] expectedMsgNotEscaped = new byte[]{0x00, controllerID, msgID, (byte)cmd,
                0x66, 0x66, 0x66, 0x66, 0x61,
                0x00, 0x00 };
            expectedMsg[expectedMsg.Length - 2] = MessageCodec.CalculateCRC(expectedMsgNotEscaped);
            expectedMsg[expectedMsg.Length - 1] = ETX;
            expectedMsg[0] = STX;

            byte[] msg = MessageCodec.EncodeMessage(new ProtocolMessage(controllerID, msgID, cmd, commandData));
            MessageCodec.DecodeMessages(msg, out List<ProtocolMessage> msgs, out byte[] outputRemainder);
            ProtocolMessage myMessage = msgs.First();

            Assert.AreEqual(expectedMsg.Length, msg.Length);
            CollectionAssert.AreEqual(expectedMsg, msg);
            CollectionAssert.AreEqual(commandData, myMessage.CommandData);
        }

        [TestMethod]
        public void DecodeMessage_MultipleEscapeCharactersEndingIn_f_x5()
        {
            byte controllerID = 0x01;
            byte msgID = 0x90;
            Command cmd = Command.DebugString;
            byte[] commandData = Encoding.UTF8.GetBytes("fffff");
            byte[] expectedMsg = new byte[] { 0x00, controllerID, msgID, (byte)cmd,
                0x66, 0x00, 0x66, 0x00, 0x66, 0x00, 0x66, 0x00, 0x66, 0x00,
                0x00, 0x00 };
            byte[] expectedMsgNotEscaped = new byte[]{0x00, controllerID, msgID, (byte)cmd,
                0x66, 0x66, 0x66, 0x66, 0x66,
                0x00, 0x00 };
            expectedMsg[expectedMsg.Length - 2] = MessageCodec.CalculateCRC(expectedMsgNotEscaped);
            expectedMsg[expectedMsg.Length - 1] = ETX;
            expectedMsg[0] = STX;

            byte[] msg = MessageCodec.EncodeMessage(new ProtocolMessage(controllerID, msgID, cmd, commandData));
            MessageCodec.DecodeMessages(msg, out List<ProtocolMessage> msgs, out byte[] outputRemainder);
            ProtocolMessage myMessage = msgs.First();

            Assert.AreEqual(expectedMsg.Length, msg.Length);
            CollectionAssert.AreEqual(expectedMsg, msg);
            CollectionAssert.AreEqual(commandData, myMessage.CommandData);
        }
    }
}
