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
    public enum Source : byte
    {
        HandwrittenOffset = 0b0000_0000,
        HandwrittenIndex = 0b0001_0000,
        SimulinkCApiOffset = 0b0100_0000,
        SimulinkCApiIndex = 0b0101_0000,
        ElfParsed = 0b0111_0000,
    }
}
