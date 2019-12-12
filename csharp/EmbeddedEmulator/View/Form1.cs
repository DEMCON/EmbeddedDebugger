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
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using EmbeddedDebugger.Connectors.Interfaces;
using System.Diagnostics;
using EmbeddedDebugger.DebugProtocol.Messages;
using EmbeddedDebugger.DebugProtocol;
using System.Drawing;
using EmbeddedEmulator.Model;
using EmbeddedDebugger.DebugProtocol.Enums;
using EmbeddedDebugger.DebugProtocol.RegisterValues;
using System.Runtime.InteropServices;
using System.Text;

namespace EmbeddedEmulator.View
{
    public partial class Form1 : Form
    {
        private List<ProtocolMessage> messages;
        private EmbeddedConfig embeddedConfig;
        private bool sendSine = true;
        private Stopwatch stopwatch;
        private DebugProtocolServer dp;

        private IConnector connector;
        public bool Bool { get => (bool)embeddedConfig.WriteRegisters.First(x => x.Offset == 2).Value.Value; }
        public int Int { get => (int)embeddedConfig.WriteRegisters.First(x => x.Offset == 3).Value.Value; }
        public bool RequestSine { get => (bool)embeddedConfig.WriteRegisters.First(x => x.Offset == 4).Value.Value; }
        public string String { get => (string)embeddedConfig.ReadRegisters.First(x => x.Offset == 0).Value.Value; }

        public Form1()
        {
            InitializeComponent();

            embeddedConfig = new EmbeddedConfig();
            FillConfig();
            dp = new DebugProtocolServer(embeddedConfig);
            dp.NewWriteMessage += Dp_NewWriteMessage;
            dp.NewDebugString += Dp_NewDebugString;
            messages = new List<ProtocolMessage>();
            closeServerButton.Enabled = false;
            connectionChooserComboBox.DisplayMember = "Name";
            connectionChooserComboBox.ValueMember = "Name";

            embeddedTerminal1.NewMessage += EmbeddedTerminal1_NewMessage;

            connectionChooserComboBox.DataSource = dp.Connectors;
            stopwatch = new Stopwatch();
            stopwatch.Start();
        }

        private void EmbeddedTerminal1_NewMessage(object sender, string e)
        {
            dp.Connector.SendMessage(MessageCodec.EncodeMessage(TemplateProvider.GetDebugStringMessage(e)));
        }

        private void Dp_NewDebugString(object sender, string e)
        {
            embeddedTerminal1.AddResponse(e);
        }

        private void Dp_NewWriteMessage(object sender, ProtocolMessage e)
        {
            if (InvokeRequired)
            {
                Invoke(new MethodInvoker(() => Refresh()));
            }
            else
            {
                Refresh();
            }
        }

        private void OpenServerButton_Click(object sender, EventArgs e)
        {
            connector.UnexpectedDisconnect += Connector_UnexpectedDisconnect;
            connector.HasConnected += Connector_HasConnected;
            if (!connector.Connect())
            {
                return;
            }
            openServerButton.Enabled = false;
            closeServerButton.Enabled = true;
            dp.Connector = connector;
        }

        private void Connector_HasConnected(object sender, EventArgs e)
        {
            if (InvokeRequired)
            {
                Invoke(new MethodInvoker(delegate
                {
                    Connector_HasConnected(sender, e);
                }));
            }
            else
            {
                sendingGroupBox.Enabled = true;
            }
        }

        private void Connector_UnexpectedDisconnect(object sender, EventArgs e)
        {
            if (InvokeRequired)
            {
                Invoke(new MethodInvoker(delegate
                {
                    Connector_UnexpectedDisconnect(sender, e);
                }));
            }
            else
            {
                sendingGroupBox.Enabled = false;
            }
        }

        private void TextBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        private void CloseServerButton_Click(object sender, EventArgs e)
        {
            sendSine = false;
            openServerButton.Enabled = true;
            closeServerButton.Enabled = false;
            sendingGroupBox.Enabled = false;
            connector.Disconnect();
        }

        private void SendVersionButton_Click(object sender, EventArgs e)
        {
            connector.SendMessage(MessageCodec.EncodeMessage(TemplateProvider.GetVersionMessage(0x00, embeddedConfig, 0x01)));
        }
        private void SendInfoButton_Click(object sender, EventArgs e)
        {
            connector.SendMessage(MessageCodec.EncodeMessage(TemplateProvider.GetInfoMessage(0x00, 0x01)));
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                connector.Disconnect();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            sendSine = false;
        }

        private void SendValueButton_Click(object sender, EventArgs e)
        {
            Thread t = new Thread(SendSine);
            t.Start();
        }

        private void SendSine()
        {
            sendSine = true;
            long currentTime = stopwatch.ElapsedMilliseconds;
            while (stopwatch.ElapsedMilliseconds < currentTime + 600000 && sendSine)
            {
                double value = (((double)stopwatch.ElapsedMilliseconds / 100) % 6.28 - 3.14);
                connector.SendMessage(MessageCodec.EncodeMessage(TemplateProvider.GetChannelDataMessage((int)(100 * Math.Sin(value) + 128), stopwatch.ElapsedMilliseconds)));
                Thread.Sleep(1);
            }
        }

        private void SettingsButton_Click(object sender, EventArgs e)
        {
            ((IConnector)connectionChooserComboBox.SelectedItem).ShowDialog();
        }

        private void ConnectionChooserComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            connector = (IConnector)connectionChooserComboBox.SelectedItem;
        }

        private void StopSendingButton_Click(object sender, EventArgs e)
        {
            sendSine = false;
        }

        private void SendConfigButton_Click(object sender, EventArgs e)
        {
            foreach (ProtocolMessage msg in TemplateProvider.GetConfiguration(0x00, embeddedConfig, 0x01))
            {
                connector.SendMessage(MessageCodec.EncodeMessage(msg));
            }
        }

        private void AutoRespondCheckbox_CheckedChanged(object sender, EventArgs e)
        {
            dp.AutoRespond = AutoRespondCheckbox.Checked;
            ResponsesGroupBox.Enabled = !AutoRespondCheckbox.Checked;
        }

        private void BoolButton_Click(object sender, EventArgs e)
        {
            embeddedConfig.WriteRegisters.First(x => x.Offset == 2).Value.ValueByteArray = BitConverter.GetBytes(!Bool);
            Refresh();
        }

        public override void Refresh()
        {
            BoolButton.BackColor = Bool ? Color.FromName("Green") : Color.FromName("Red");
            IntTextBox.Text = Int.ToString();
            StringTextBox.Text = String;
            if (RequestSine && !sendSine)
            {
                sendSine = true;
                SendValueButton_Click(this, new EventArgs());
            }
            else if (!RequestSine && sendSine)
            {
                sendSine = false;
            }
            base.Refresh();
        }

        private void FillConfig()
        {
            embeddedConfig.CpuName = "Foitn";
            embeddedConfig.ApplicationVersion = new Version(0, 0, 1);
            embeddedConfig.ProtocolVersion = new Version(1, 0, 0);
            embeddedConfig.SerialNumber = "0.1.2.3.4";
            embeddedConfig.ReadRegisters.Add(
                new Register(0, "myString", ReadWrite.ReadWrite, VariableType.String, Source.ElfParsed, 0, 0, 0) { Value = new RegisterValueString() { ValueByteArray = Encoding.UTF8.GetBytes("SomeString") } });
            //embeddedConfig.ReadRegisters.Add(
            //    new Register(1, "myBlob", ReadWrite.ReadWrite, VariableType.Blob, Source.ElfParsed, 0, 1, 0) { Value = new RegisterValueBlob() { ValueByteArray = (byte[])(new ImageConverter()).ConvertTo(Image.FromFile(@"C:/Temp/TestImage.jpg"), typeof(byte[])) } });
            embeddedConfig.ReadRegisters.Add(
                new Register(1, "Sine", ReadWrite.Read, VariableType.UChar, Source.ElfParsed, 0, 0, 1) { Value = new RegisterValueUChar() });
            //embeddedConfig.ReadRegisters.Add(
            //    new Register(1, "Cosine", ReadWrite.Read, VariableType.Double, Source.ElfParsed, 0, 1, 8) { Value = new RegisterValueDouble() });
            embeddedConfig.WriteRegisters.Add(
                new Register(2, "My red green button", ReadWrite.Write, VariableType.Bool, Source.ElfParsed, 0, 2, 1) { Value = new RegisterValueBool() });
            embeddedConfig.WriteRegisters.Add(
                new Register(3, "Int in textbox", ReadWrite.Write, VariableType.UChar, Source.ElfParsed, 0, 3, 1) { Value = new RegisterValueSInt() });
            embeddedConfig.WriteRegisters.Add(
                new Register(4, "Request Sine", ReadWrite.Write, VariableType.Bool, Source.ElfParsed, 0, 4, 1) { Value = new RegisterValueBool() });
            //embeddedConfig.ReadRegisters.Add(
            //    new Register(5, "Tangent", ReadWrite.Read, VariableType.UChar, Source.ElfParsed, 0, 5, 1) { Value = new RegisterValueUChar() });
            //embeddedConfig.ReadRegisters.Add(
            //    new Register(6, "Random", ReadWrite.Read, VariableType.SChar, Source.ElfParsed, 0, 6, 1) { Value = new RegisterValueUChar() });
            embeddedConfig.ReadRegisters.Add(
                new Register(6, "Counter", ReadWrite.None, VariableType.Unknown, Source.ElfParsed, 0, 6, 32));
            embeddedConfig.ReadRegisters.Add(
                new Register(7, @"Counter.Char", ReadWrite.ReadWrite, VariableType.Char, Source.ElfParsed, 0, 7, 2) { Value = new RegisterValueChar() });
            embeddedConfig.ReadRegisters.Add(
                new Register(20, @"Counter SChar", ReadWrite.Read, VariableType.SChar, Source.ElfParsed, 0, 20, 1) { Value = new RegisterValueSChar() });
            embeddedConfig.ReadRegisters.Add(
                new Register(9, @"Counter\UChar", ReadWrite.Read, VariableType.UChar, Source.ElfParsed, 0, 9, 1) { Value = new RegisterValueUChar() });
            embeddedConfig.ReadRegisters.Add(
                new Register(10, @"Counter.Short", ReadWrite.Read, VariableType.Short, Source.ElfParsed, 0, 10, 2) { Value = new RegisterValueSShort() });
            embeddedConfig.ReadRegisters.Add(
                new Register(11, @"Counter.UShort", ReadWrite.Read, VariableType.UShort, Source.ElfParsed, 0, 11, 2) { Value = new RegisterValueUShort() });
            embeddedConfig.ReadRegisters.Add(
                new Register(12, @"Counter.Int", ReadWrite.Read, VariableType.Int, Source.ElfParsed, 0, 12, 4) { Value = new RegisterValueSInt() });
            embeddedConfig.ReadRegisters.Add(
                new Register(13, @"Counter.UInt", ReadWrite.Read, VariableType.UInt, Source.ElfParsed, 0, 13, 4) { Value = new RegisterValueUInt() });
            embeddedConfig.ReadRegisters.Add(
                new Register(14, @"Counter.Long", ReadWrite.Read, VariableType.Long, Source.ElfParsed, 0, 14, 8) { Value = new RegisterValueSLong() });
            embeddedConfig.ReadRegisters.Add(
                new Register(15, @"Counter.ULong", ReadWrite.Read, VariableType.ULong, Source.ElfParsed, 0, 15, 8) { Value = new RegisterValueULong() });
            embeddedConfig.ReadRegisters.Add(
                new Register(999, @"Some name", ReadWrite.None, VariableType.Unknown, Source.ElfParsed, 0, 999, 0));
            for (int i = 1000; i < 2500; i++)
            {
                embeddedConfig.ReadRegisters.Add(
                    new Register((uint)i, $"Some name\\{i}", ReadWrite.Read, VariableType.Int, Source.ElfParsed, 0, (uint)i, 4) { Value = new RegisterValueSInt() { ValueByteArray = BitConverter.GetBytes(i) } });
            }
            for (int i = 4000; i < 5500; i++)
            {
                embeddedConfig.ReadRegisters.Add(
                    new Register((uint)i, $"Some name\\{i}", ReadWrite.Read, VariableType.Int, Source.ElfParsed, 0, (uint)i, 4) { Value = new RegisterValueSInt() { ValueByteArray = BitConverter.GetBytes(i) } });
            }
        }

        private void SimulateMultipleNodes_CheckedChanged(object sender, EventArgs e)
        {
            dp.SimulateMultipleNodes = SimulateMultipleNodes.Checked;
            MultipleNodesConfigGroupBox.Enabled = SimulateMultipleNodes.Checked;
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            dp.NumberOfNodesToSimulate = (int)numericUpDown1.Value;
        }

        private void StringTextBox_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
