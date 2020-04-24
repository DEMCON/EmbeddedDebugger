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
using EmbeddedDebugger.Connectors.Interfaces;
using EmbeddedDebugger.Connectors.CustomEventArgs;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmbeddedDebugger.Connectors.Serial
{
    /// <summary>
    /// This class is an interface between a serial connection with a microcontroller and the core of the program
    /// </summary>
    public class SerialConnector : Connector
    {
        // What we should name this connector
        private const string myName = "Serial";

        private string portName;
        private int baudRate;
        private Parity parity;
        private int dataBits;
        private StopBits stopBits;
        private Handshake handshake;
        private int readTimeout;
        private int writeTimeout;
        private bool isConnected = false;
        private readonly int blockLimit = 4096;
        private SerialPort port;

        #region Properties
        public string PortName { get => portName; set => portName = value; }
        public int BaudRate { get => baudRate; set => baudRate = value; }
        public Parity Parity { get => parity; set => parity = value; }
        public int DataBits { get => dataBits; set => dataBits = value; }
        public StopBits StopBits { get => stopBits; set => stopBits = value; }
        public Handshake Hanshake { get => handshake; set => handshake = value; }
        public int ReadTimeout { get => readTimeout; set => readTimeout = value; }
        public int WriteTimeout { get => writeTimeout; set => writeTimeout = value; }
        #endregion

        public SerialConnector() { }

        #region IConnector Members
        public override string Name => myName;
        public override bool AsServer { get => false; set => myName.ToString(); }
        public override bool IsConnected => isConnected;
        public override event EventHandler HasConnected = delegate { };
        public override event EventHandler<BytesReceivedEventArgs> MessageReceived = delegate { };
        public override event EventHandler UnexpectedDisconnect = delegate { };

        public override bool Connect()
        {
            // If no portname has been defined, show the settings dialog
            if (string.IsNullOrEmpty(portName) && ShowDialog() == false)
            {
                return false;
            }
            port = new SerialPort
            {
                PortName = portName,
                BaudRate = baudRate,
                Parity = parity,
                DataBits = dataBits,
                StopBits = stopBits,
                Handshake = handshake,
                ReadTimeout = readTimeout,
                WriteTimeout = writeTimeout
            };
            try
            {
                port.Open();
            }
            catch (Exception e)
            {
                System.Windows.Forms.MessageBox.Show("Port in use");
#if DEBUG
                Console.WriteLine(e);
#endif
                return false;
            }
            isConnected = true;
            this.HasConnected(this, new EventArgs());
            // Define an action which takes care of reading new data from the serialport
            // This uses the underlying basestream of the comport, since the original com port is not robust enough
            byte[] buffer = new byte[blockLimit];
            Action kickoffRead = null;
            kickoffRead = delegate
            {
                if (!port.IsOpen) return;
                port.BaseStream.BeginRead(buffer, 0, buffer.Length, delegate (IAsyncResult ar)
                {
                    try
                    {
                        int actualLength = port.BaseStream.EndRead(ar);
                        byte[] received = new byte[actualLength];
                        Buffer.BlockCopy(buffer, 0, received, 0, actualLength);
                        ReceiveMessage(received);
                    }
                    catch (Exception exc)
                    {
#if DEBUG
                        Console.WriteLine(exc);
#endif
                        isConnected = false;
                        UnexpectedDisconnect(this, new EventArgs());
                        try
                        {
                            port.Close();
                        }
                        catch (IOException e)
                        {
#if DEBUG
                            Console.WriteLine(e);
#endif
                        }
                    }
                    if (isConnected)
                    {
                        // If still connected, read from the serial port again
                        kickoffRead();
                    }
                }, null);
            };
            kickoffRead();
            return true;
        }

        public override void Disconnect()
        {
            port.Close();
        }

        public override void SendMessage(byte[] msg)
        {
            try
            {
                port.BaseStream.WriteAsync(msg, 0, msg.Length);
            }
            catch (Exception e)
            {
#if DEBUG
                Console.WriteLine(e);
#endif
                Disconnect();
                isConnected = false;
                UnexpectedDisconnect(this, new EventArgs());
            }
        }

        public override bool? ShowDialog()
        {
            // Write the current settings to the form
            SerialConnectorSettingsWindow scs = new SerialConnectorSettingsWindow
            {
                PortName = portName,
                BaudRate = baudRate,
                Parity = parity,
                DataBits = dataBits,
                StopBits = stopBits,
                Handshake = handshake,
                ReadTimeout = readTimeout,
                WriteTimout = writeTimeout
            };

            bool? dr = scs.ShowDialog();
            // Read the new settings from the form
            if (dr == true)
            {
                portName = scs.PortName;
                baudRate = scs.BaudRate;
                parity = scs.Parity;
                dataBits = scs.DataBits;
                stopBits = scs.StopBits;
                handshake = scs.Handshake;
                readTimeout = scs.ReadTimeout;
                writeTimeout = scs.WriteTimout;
            }
            return dr;
        }

        public override void ReceiveMessage(byte[] msg)
        {
            MessageReceived(this, new BytesReceivedEventArgs(msg));
        }

        public override string ToString()
        {
            return myName;
        }
        #endregion
    }
}
