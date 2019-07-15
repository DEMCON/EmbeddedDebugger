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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmbeddedDebugger.Connectors.ProjectConnectors.Interfaces
{
    public interface IProjectConnector : IConnector
    {
        /// <summary>
        /// The overriden method from the normal connector, allowing for a project specific piece of magic to be applied
        /// </summary>
        /// <param name="msg">An incoming message from the original connector</param>
        void ReceiveMessage(byte[] msg);
        /// <summary>
        /// The overriden method from the normal connector, allowing for a project specific piece of magic to be applied
        /// </summary>
        /// <param name="msg">An outgoing message from the original connector</param>
        void SendMessage(byte[] msg);
    }
}
