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
    public enum VariableType : byte
    {
        MemoryAlignment = 0x00, // memory alignment (given in size_n; typically 1 or 4; example: memory alignment = 4  addresses are a multiple of 4)
        Pointer = 0x01,
        Bool = 0x02,
        Char = 0x03,
        Short = 0x04,
        Int = 0x05,
        Long = 0x06,
        Float = 0x07,
        Double = 0x08,
        LongDouble = 0x09,
        TimeStamp = 0x0A,        // time-stamp units in μs (uses 4 bytes for size_n!)
        SChar = 0x0B,
        UChar = 0x0C,
        UShort = 0x0D,
        UInt = 0x0E,
        ULong = 0x0F,
        String = 0x10,
        Blob = 0x11,
        Unknown = 0xFF,
    }
}
