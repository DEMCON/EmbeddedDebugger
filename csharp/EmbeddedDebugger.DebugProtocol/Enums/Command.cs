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
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmbeddedDebugger.DebugProtocol.Enums
{
    /// <summary>
    /// This enum has been defined for the custom protocol used by the embedded debugger
    /// For more information, have a look at the documentation
    /// </summary>
    public enum Command : byte
    {
        GetVersion = 0x56,
        GetInfo = 0x49,
        WriteRegister = 0x57,
        QueryRegister = 0x51,
        ConfigChannel = 0x43,
        Decimation = 0x44,
        ResetTime = 0x54,
        ReadChannelData = 0x52,
        DebugString = 0x53,
        EmbeddedConfiguration = 0x45,
        Tracing = 0x41,
    }
}
