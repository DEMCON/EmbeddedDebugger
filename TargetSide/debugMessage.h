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

#ifndef DEBUGMESSAGE_H
#define DEBUGMESSAGE_H

#ifdef __cplusplus
extern "C" {
#endif

#include <stdlib.h>
#include <stdint.h>
#include <stdbool.h>

#define DEBUG_MSG_SIZE              (128)
#define DEBUG_BUF_IN_SIZE_BITS      (10)
#define DEBUG_BUF_IN_SIZE           (1024)      //2^DEBUG_BUF_SIZE_BITS


typedef enum EDebugCmd
{
    cmdVersion          = 'V',
    cmdInfo             = 'I',
    cmdWriteRegister    = 'W',
    cmdQueryRegister    = 'Q',
    cmdConfigChannel    = 'C',
    cmdDecimation       = 'D',
    cmdResetTime        = 'T',
    cmdReadChannelData  = 'R',
    cmdDebugString      = 'S'
} EDebugCmd;


typedef struct SDebugMessageIn
{
    bool        fBufferOverrun;
    bool        fValidMessage;
    uint32_t    uNodeID;
    uint8_t     uMsgID;
    EDebugCmd   cmd;
    int32_t     nCmdParamSize;
    uint8_t     rgMessage[DEBUG_MSG_SIZE];
    bool        _fFoundSTX;
    uint32_t    _uIndexSTX;
    uint32_t    _uIndexETX;
    uint32_t    _uIndexParseNext;
    uint32_t    _uIndexPush;
    uint8_t    _rgRawMsgData[DEBUG_BUF_IN_SIZE];
} SDebugMessageIn;


typedef struct SDebugMessageOut
{
    uint32_t    uNodeID;
    EDebugCmd   cmd;
    uint8_t     uMsgID;
    uint8_t     rgMessage[DEBUG_MSG_SIZE];
    uint32_t    _uIndexMessage;
    uint8_t     _rgRawMsgData[DEBUG_MSG_SIZE];
    uint32_t    _uIndexRawData;
} SDebugMessageOut;


void DebugMsgIn_Init(SDebugMessageIn* pMsg);
void DebugMsgIn_AddReceivedData(SDebugMessageIn* pMsg, uint8_t* rgData, uint32_t uSize);
bool DebugMsgIn_DecodeAndCheck(SDebugMessageIn* pMsg);

void DebugMsgOut_Init(SDebugMessageOut* pMsg);
bool DebugMsgOut_AddByte(SDebugMessageOut* pMsg, const uint8_t uData);
bool DebugMsgOut_AddData(SDebugMessageOut* pMsg, const uint8_t* rgParam, uint32_t uSize);
void DebugMsgOut_Encode(SDebugMessageOut* pMsg);

#ifdef __cplusplus
}
#endif


#endif //DEBUGMESSAGE_H
