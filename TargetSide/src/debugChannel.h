/*
Embedded Debugger system side which can be used to debug embedded systems at a high level.
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

#ifndef DEBUGCHANNEL_H
#define DEBUGCHANNEL_H

#ifdef __cplusplus
extern "C" {
#endif

#include <stdint.h>
#include <stdbool.h>

typedef enum EUpdateMode
{
    updateOff       = 0x00,     //not active
    updateFast      = 0x01,     //typically every ms (use decimation!!)
    updateSlow      = 0x02,     //typically 1Hz
    updateOnce      = 0x03,     //update once, goes to off after sending 1 sample
    updateAll       = 0x04      //send all
} EUpdateMode;


typedef enum ESource
{
    sourceHandwrittenOffset = 0x00,
    sourceHandwrittenIndex  = 0x10,
    sourceSimulinkOffset    = 0x40,
    sourceSimulinkIndex     = 0x50,
    sourceAbsoluteAddress   = 0x70,
} ESource;


typedef enum EDirection
{
    debugRead               = 0x00,
    debugWrite              = 0x80
} EDirection;


typedef struct SDebugChannel
{
    uint8_t*            pSource;
    uint8_t             rgValuePrev[8];
    uint8_t             uSize_bytes;
    uint8_t             uPointerDepth;
    EUpdateMode         updateMode;
    uint8_t             _uCtrl;
    uint32_t            _uOffset;
} SDebugChannel;


void DbgChan_Init(SDebugChannel* pChan);
void DbgChan_WriteValue(SDebugChannel* pChan, uint8_t* pValue);
bool DbgChan_ReadValue(SDebugChannel* pChan, uint8_t* pValue);

#ifdef __cplusplus
}
#endif

#endif //DEBUGCHANNEL_H
