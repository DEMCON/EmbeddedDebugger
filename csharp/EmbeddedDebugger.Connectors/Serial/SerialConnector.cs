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
using EmbeddedDebugger.Connectors.BaseClasses;
using EmbeddedDebugger.Connectors.Settings;
using EmbeddedDebugger.DebugProtocol.CustomEventArgs;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Xml.Serialization;

namespace EmbeddedDebugger.Connectors.Serial
{
    /// <summary>
    /// This class is an interface between a serial connection with a microcontroller and the core of the program
    /// </summary>
    public class SerialConnector : BaseEmbeddedDebugProtocolConnection
    {
        // What we should name this connector
        private const string MyName = "Serial";
        private const int BlockLimit = 4096;
        // Some default baud rates to use
        private static readonly int[] DefaultBaudRates = { 110, 300, 600, 1200, 2400, 4800, 9600, 14400, 19200, 38400, 57600, 115200, 128000, 256000 };
        private static readonly List<string> PortNames = SerialPort.GetPortNames().ToList();

        [XmlIgnore]
        public override List<ConnectionSetting> ConnectionSettings =>
        new List<ConnectionSetting> {
                new ConnectionSetting{
                    Name = "PortName",
                    Value = this.PortName,
                    Possibilities = PortNames,
                    EnforcePossibilities = true,
                    PossibilitiesRefresher = ()=>
                    {
                        PortNames.Clear();
                        PortNames.AddRange(SerialPort.GetPortNames());
                    }
                },
                new ConnectionSetting
                {
                    Name = "Baud rate",
                    Value = this.BaudRate == 0 ? 115200 : this.BaudRate,
                    Possibilities = DefaultBaudRates.Cast<object>(),
                    EnforcePossibilities = false
                },
                new ConnectionSetting
                {
                    Name = "Parity",
                    // Here we use the baud rate == 0, because if this is 0, it was not saved, therefore generate default value
                    Value = this.BaudRate == 0 ? Parity.None : this.Parity,
                    Possibilities = Enum.GetValues(typeof(Parity)).Cast<object>(),
                    EnforcePossibilities = true
                },
                new ConnectionSetting
                {
                    Name = "Data bits",
                    Value = this.DataBits == 0 ? 8 : this.DataBits,
                    Possibilities = new [] {5,6,7,8}.Cast<object>(),
                    EnforcePossibilities = true
                },
                new ConnectionSetting
                {
                    Name = "Stop bits",
                    // Here we use the baud rate == 0, because if this is 0, it was not saved, therefore generate default value
                    Value = this.BaudRate == 0 ? StopBits.One : this.StopBits,
                    Possibilities = Enum.GetValues(typeof(StopBits)).Cast<object>(),
                    EnforcePossibilities = true
                },
                new ConnectionSetting
                {
                    Name = "Handshake",
                    // Here we use the baud rate == 0, because if this is 0, it was not saved, therefore generate default value
                    Value = this.Handshake == 0 ? Handshake.None : this.Handshake,
                    Possibilities = Enum.GetValues(typeof(Handshake)).Cast<object>(),
                    EnforcePossibilities = true
                },
                new ConnectionSetting
                {
                    Name = "Read timeout",
                    Value = this.ReadTimeout == 0 ? 500 : this.ReadTimeout,
                },
                new ConnectionSetting
                {
                    Name = "Write timeout",
                    Value = this.WriteTimeout == 0 ? 500 : this.WriteTimeout,
                },
            };

        private bool isConnected;
        private SerialPort port;

        public SerialConnector()
        {
        }


        #region Properties
        public string PortName { get; set; }
        public int BaudRate { get; set; }
        public Parity Parity { get; set; }
        public int DataBits { get; set; }
        public StopBits StopBits { get; set; }
        public Handshake Handshake { get; set; }
        public int ReadTimeout { get; set; }
        public int WriteTimeout { get; set; }
        #endregion

        #region IConnector Members
        public override string Name => MyName;
        public override bool IsConnected => this.isConnected;
        public override event EventHandler UnexpectedlyDisconnected = delegate { };
        public override event EventHandler HasConnected = delegate { };

        public override void SetConnectionSettings(List<ConnectionSetting> settings)
        {
            //Add some testing for the values
            this.PortName = (string)(settings.First(x => x.Type == typeof(string) && x.Name == "PortName")
                                ?.Value ?? throw new ArgumentException("PortName not included in settings"));
            this.BaudRate = (int)(settings.First(x => x.Type == typeof(int) && x.Name == "Baud rate")
                                 .Value ?? throw new ArgumentException("BaudRate not included in settings"));
            this.Parity = (Parity)(settings.First(x => x.Type == typeof(Parity) && x.Name == "Parity")
                                ?.Value ?? throw new ArgumentException("Parity not included in settings"));
            this.DataBits = (int)(settings.First(x => x.Type == typeof(int) && x.Name == "Data bits")
                                 .Value ?? throw new ArgumentException("BaudRate not included in settings"));
            this.StopBits = (StopBits)(settings.First(x => x.Type == typeof(StopBits) && x.Name == "Stop bits")
                                ?.Value ?? throw new ArgumentException("StopBits not included in settings"));
            this.Handshake = (Handshake)(settings.First(x => x.Type == typeof(Handshake) && x.Name == "Handshake")
                                 .Value ?? throw new ArgumentException("Handshake not included in settings"));
            this.ReadTimeout = (int)(settings.First(x => x.Type == typeof(int) && x.Name == "Read timeout")
                                ?.Value ?? throw new ArgumentException("ReadTimeout not included in settings"));
            this.WriteTimeout = (int)(settings.First(x => x.Type == typeof(int) && x.Name == "Write timeout")
                                 .Value ?? throw new ArgumentException("WriteTimeout not included in settings"));
        }

        public override bool Connect()
        {
            // If no portname has been defined, show the settings dialog
            if (string.IsNullOrEmpty(this.PortName) && this.ShowDialog() == false)
            {
                return false;
            }

            this.port = new SerialPort
            {
                PortName = this.PortName,
                BaudRate = this.BaudRate,
                Parity = this.Parity,
                DataBits = this.DataBits,
                StopBits = this.StopBits,
                Handshake = this.Handshake,
                ReadTimeout = this.ReadTimeout,
                WriteTimeout = this.WriteTimeout
            };
            try
            {
                this.port.Open();
            }
            catch (Exception e)
            {
                System.Windows.Forms.MessageBox.Show("Port in use");
#if DEBUG
                Console.WriteLine(e);
#endif
                return false;
            }

            this.isConnected = true;
            // Define an action which takes care of reading new data from the serialport
            // This uses the underlying basestream of the comport, since the original com port is not robust enough
            byte[] buffer = new byte[BlockLimit];

            void KickoffRead()
            {
                if (!this.port.IsOpen) return;
                this.port.BaseStream.BeginRead(buffer, 0, buffer.Length, delegate (IAsyncResult ar)
                {
                    try
                    {
                        int actualLength = this.port.BaseStream.EndRead(ar);
                        byte[] received = new byte[actualLength];
                        Buffer.BlockCopy(buffer, 0, received, 0, actualLength);
                        this.ReceiveMessage(new BytesReceivedEventArgs(received));
                    }
                    catch (Exception exc)
                    {
#if DEBUG
                        Console.WriteLine(exc);
#endif
                        this.isConnected = false;
                        this.UnexpectedlyDisconnected(this, new EventArgs());
                        try
                        {
                            this.port.Close();
                        }
                        catch (IOException e)
                        {
#if DEBUG
                            Console.WriteLine(e);
#endif
                        }
                    }

                    if (this.isConnected)
                    {
                        // If still connected, read from the serial port again
                        KickoffRead();
                    }
                }, null);
            }

            KickoffRead();
            this.HasConnected(this, new EventArgs());
            return true;
        }

        public override void Disconnect()
        {
            this.port.Close();
        }

        public override void SendMessage(byte[] msg)
        {
            try
            {
                this.port.BaseStream.WriteAsync(msg, 0, msg.Length);
            }
            catch (Exception e)
            {
#if DEBUG
                Console.WriteLine(e);
#endif
                this.Disconnect();
                this.isConnected = false;
                this.UnexpectedlyDisconnected(this, new EventArgs());
            }
        }

        public bool? ShowDialog()
        {
            // Write the current settings to the form
            SerialConnectorSettingsWindow scs = new SerialConnectorSettingsWindow
            {
                PortName = this.PortName,
                BaudRate = this.BaudRate,
                Parity = this.Parity,
                DataBits = this.DataBits,
                StopBits = this.StopBits,
                Handshake = this.Handshake,
                ReadTimeout = this.ReadTimeout,
                WriteTimout = this.WriteTimeout
            };

            bool? dr = scs.ShowDialog();
            // Read the new settings from the form
            if (dr == true)
            {
                this.PortName = scs.PortName;
                this.BaudRate = scs.BaudRate;
                this.Parity = scs.Parity;
                this.DataBits = scs.DataBits;
                this.StopBits = scs.StopBits;
                this.Handshake = scs.Handshake;
                this.ReadTimeout = scs.ReadTimeout;
                this.WriteTimeout = scs.WriteTimout;
            }
            return dr;
        }

        public override string ToString()
        {
            return MyName;
        }
        #endregion
    }
}
