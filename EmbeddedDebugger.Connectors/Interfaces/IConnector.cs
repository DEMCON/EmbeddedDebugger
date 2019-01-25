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
using EmbeddedDebugger.Connectors.CustomEventArgs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmbeddedDebugger.Connectors.Interfaces
{
    /// <summary>
    /// This interface is used to define how a connector should behave and how can be communicated with one
    /// It allows the rest of the software to talk to a microcontroller in a modular way
    /// </summary>
    public interface IConnector
    {
        /// <summary>
        /// The name of the connector
        /// </summary>
        string Name { get; }
        /// <summary>
        /// Whether this connector is connected
        /// </summary>
        bool IsConnected { get; }
        /// <summary>
        /// This eventhandler is called whenever a new message is received
        /// </summary>
        event EventHandler<BytesReceivedEventArgs> MessageReceived;
        /// <summary>
        /// Sometimes a port is disconnected unexpectedly, this eventhandler can be used to perform the appropriate actions
        /// </summary>
        event EventHandler UnexpectedDisconnect;
        /// <summary>
        /// Some connectors work like servers, which do not connect, but have to be connected to, therefore an event is needed when it has connected
        /// </summary>
        event EventHandler HasConnected;
        /// <summary>
        /// The connector needs to be configured, choosing things like a port or hostname
        /// </summary>
        /// <param name="owner">The parent window, to center to</param>
        /// <returns>Whether or not the configuration is accepted</returns>
        bool? ShowDialog();
        /// <summary>
        /// Try to connect to the port
        /// </summary>
        /// <returns>Whether or not a connection has been established</returns>
        bool Connect();
        /// <summary>
        /// Disconnect from the connected port
        /// </summary>
        void Disconnect();
        /// <summary>
        /// Send a message over the connection
        /// </summary>
        /// <param name="msg">The message to send</param>
        void SendMessage(byte[] msg);
        /// <summary>
        /// This should return the name of the connector, ensuring we can simply put them all in a list and have a nice name for it
        /// </summary>
        /// <returns>Should return Name</returns>
        string ToString();
        /// <summary>
        /// Some connectors can/should be used as server
        /// </summary>
        bool AsServer { get; set; }
        /// <summary>
        /// Used to raise the event, not usefull for a standard connector
        /// But when a standard connector is inherited, this method can be used to override whenever the project connector has an application protocol. 
        /// Therefore method should ALWAYS be virtual!
        /// </summary>
        /// <param name="inputMsg">The received byte array</param>
        void ReceiveMessage(byte[] msg);
    }
}
