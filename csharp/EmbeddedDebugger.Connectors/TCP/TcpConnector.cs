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
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace EmbeddedDebugger.Connectors.TCP
{
    /// <summary>
    /// This class is an interface between a TCP connection with a microcontroller or embedded system and the core of the program
    /// </summary>
    public class TcpConnector : BaseEmbeddedDebugProtocolConnection
    {
        // What we should name this connector
        private const string myName = "TCP";

        public override List<ConnectionSetting> ConnectionSettings { get; } =
            new List<ConnectionSetting> {
                new ConnectionSetting{Name = "HostName"},
                new ConnectionSetting{Name = "Port",Value = 69483}
            };

        private string hostName;
        private int port;
        private bool isConnected = false;
        private TcpClient client;
        private NetworkStream stream;
        private TcpListener server;

        #region Properties
        public virtual string HostName { get => hostName; set => hostName = value; }
        public virtual int Port { get => port; set => port = value; }
        #endregion

        public TcpConnector() { }

        #region IConnector Members
        public override string Name => myName;
        public override bool IsConnected => isConnected;
        public override event EventHandler UnexpectedlyDisconnected;
        public override event EventHandler HasConnected = delegate { };
        public event EventHandler<BytesReceivedEventArgs> MessageReceived = delegate { };
        public event EventHandler UnexpectedDisconnect = delegate { };
        public override event EventHandler<string> NewTerminalMessageReceived;
        private bool asServer;
        public bool AsServer { get => asServer; set => asServer = value; }

        public override void SetConnectionSettings(List<ConnectionSetting> settings)
        {
            throw new NotImplementedException();
        }

        public override bool Connect()
        {
            // If the hostname is not set, show the settings form
            if (hostName == null && ShowDialog() == false)
            {
                return false;

            }
            // If the connector is the server, set up the server
            if (asServer)
            {
                try
                {
                    server = new TcpListener(IPAddress.Parse(hostName), port);
                    isConnected = true;
                    Thread thread = new Thread(WaitInputServer)
                    {
                        IsBackground = true
                    };
                    thread.Start();
                }
                catch (Exception e)
                {
                    System.Windows.Forms.MessageBox.Show("Port in use");
#if DEBUG
                    Console.WriteLine(e);
#endif
                    return false;
                }
            }
            else
            {
                try
                {
                    client = new TcpClient();
                    client.Connect(hostName, port);
                    isConnected = true;
                    // Let the input be gathered with another thread
                    Thread thread = new Thread(WaitInput)
                    {
                        IsBackground = true
                    };
                    thread.Start();
                }
                catch (Exception e)
                {
                    System.Windows.Forms.MessageBox.Show(e.Message);
#if DEBUG
                    Console.WriteLine(e);
#endif
                    return false;
                }
            }
            return true;
        }

        public override void Disconnect()
        {
            try
            {
                client.Close();
            }
            catch (Exception e)
            {
#if DEBUG
                Console.WriteLine(e);
#endif
            }
            try
            {
                stream.Close();
            }
            catch (Exception e)
            {
#if DEBUG
                Console.WriteLine(e);
#endif
            }
            finally
            {
                client = null;
                stream = null;
            }
            if (asServer)
            {
                try
                {
                    server.Stop();
                }
                catch (Exception e)
                {
#if DEBUG
                    Console.WriteLine(e);
#endif
                }
                finally
                {
                    client = null;
                    stream = null;
                    server = null;
                }
            }
            isConnected = false;
        }

        public override void SendMessage(byte[] msg)
        {
            if (!isConnected || stream == null) return;
            try
            {
                stream.WriteAsync(msg, 0, msg.Length);
            }
            catch (Exception e)
            {
#if DEBUG
                Console.WriteLine(e);
#endif
                if (!asServer)
                {
                    Disconnect();
                    UnexpectedDisconnect(this, new EventArgs());
                }
            }
        }

        public bool? ShowDialog()
        {
            // Set the current settings to the settings form
            TcpConnectorSettingsWindow tcpcs = new TcpConnectorSettingsWindow
            {
                HostName = hostName,
                Port = port,
                AsServer = asServer,
            };

            bool? dr = tcpcs.ShowDialog();
            // Fetch the new settings from the form
            if (dr == true)
            {
                hostName = tcpcs.HostName;
                port = tcpcs.Port;
                asServer = tcpcs.AsServer;
            }
            return dr;
        }

        public override string ToString()
        {
            return myName;
        }

        public void ReceiveMessage(byte[] msg)
        {
            MessageReceived(this, new BytesReceivedEventArgs(msg));
        }
        #endregion

        private void WaitInputServer()
        {
            byte[] bytes;
            int length;
            server.Start();
            while (isConnected && server != null)
            {
                try
                {
                    client = server.AcceptTcpClient();
                    stream = client.GetStream();
                    HasConnected(this, new EventArgs());
                    while (client.Connected)
                    {
                        if (client.ReceiveBufferSize > 0)
                        {
                            bytes = new byte[client.ReceiveBufferSize];
                            // Define the amount of bytes we actually received
                            length = stream.Read(bytes, 0, bytes.Length);
                            // Resize the array to the amount we know is in there
                            Array.Resize(ref bytes, length);
                            ReceiveMessage(bytes);
                        }
                    }
                }
                catch (Exception e)
                {
#if DEBUG
                    Console.WriteLine(e);
#endif
                    UnexpectedDisconnect(this, new EventArgs());
                }
            }
        }

        /// <summary>
        /// This method keep checking if there are new messages received and raises an event 
        /// </summary>
        private void WaitInput()
        {
            byte[] bytes;
            int length;
            try
            {
                stream = client.GetStream();
                HasConnected(this, new EventArgs());
                while (client.Connected)
                {
                    if (client.ReceiveBufferSize > 0)
                    {
                        bytes = new byte[client.ReceiveBufferSize];
                        // Define the amount of bytes we actually received
                        length = stream.Read(bytes, 0, bytes.Length);
                        // Resize the array to the amount we know is in there
                        Array.Resize(ref bytes, length);
                        ReceiveMessage(bytes);
                    }
                }
            }
            catch (Exception e)
            {
#if DEBUG
                Console.WriteLine(e);
#endif
            }
            Disconnect();
            // Let the world know this was not an intended disconnect!
            UnexpectedDisconnect(this, new EventArgs());

        }

        public void Dispose()
        {
            if (client != null)
            {
                client.Dispose();
            }
            if (stream != null)
            {
                stream.Dispose();
            }
            server = null;
        }
    }
}
