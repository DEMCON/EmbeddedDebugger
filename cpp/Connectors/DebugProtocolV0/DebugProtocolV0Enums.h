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

#ifndef DEBUGPROTOCOLV0ENUMS_H
#define DEBUGPROTOCOLV0ENUMS_H

class DebugProtocolV0Enums{

public:
    enum class ReadWrite{
        Read,
        Write
    };

    enum ProtocolChar{
        STX = 0x55,
        ETX = 0xAA,
        ESC = 0x66,
        RS = 0x33
    };

      enum class ChannelMode{
        Off,
        OnChange,
        LowSpeed,
        Once
    };

    enum class TraceMode{
        Off,
        On,
        Once
    };

    enum ProtocolCommand{
        UNKNOWN = 0x00,
        GetVersion = 0x56,
        GetInfo = 0x49,
        WriteRegister = 0x57,
        QueryRegister = 0x51,
        ConfigChannel = 0x43,
        Decimation = 0x44,
        ResetTime = 0x54,
        ReadChannelData = 0x52,
        DebugString = 0x53,
    };

    enum class Source{
        HandWrittenOffset,
        HandWrittenIndex,
        SimulinkCApiOffset,
        SimulinkCApiIndex
    };
};

#endif // DEBUGPROTOCOLV0ENUMS_H
