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

#ifndef DEBUGPROTOCOL_H
#define DEBUGPROTOCOL_H

#ifdef __cplusplus
extern "C" {
#endif

/*******************************************************************
* Includes
*******************************************************************/
#include "debugMessage.h"
#include "debugChannel.h"
/*******************************************************************
* Defines
*******************************************************************/

#define DEBUG
#ifdef DEBUG
    #define GETCHAR(x)          DebugProt_GetChar(x)
    #define TRACE(x)            DebugProt_Trace(x)
    #define ASSERT(x)           if (!(x))                                           \
                                {                                                   \
                                    DebugProt_AssertFail(#x, __FILE__, __LINE__);   \
                                }
    #define NOT_IMPLEMENTED     FALSE
#else
    #define GETCHAR(x)          FALSE
    #define TRACE(x)            ;
    #define ASSERT(x)           ;
#endif


#define DEBUG_STRING_IN_SIZE_BITS   (8)
#define DEBUG_STRING_IN_SIZE        (256)   //2^DEBUG_STRING_IN_SIZE_BITS


//necessary forward declarations
typedef struct SAppProtocol SAppProtocol;

extern uint32_t time;

/*******************************************************************
* Types
*******************************************************************/


typedef bool (*funcGetByte)(uint8_t* data);
typedef void (*funcWriteData)(uint8_t* data, uint16_t dataSize);
typedef void (*funcGetRegisterAddress)(SDebugChannel* pChan);


typedef struct SDebugProtocol
{
    uint8_t                 nDummyForAlignment0;
    uint8_t                 nDummyForAlignment1;
    uint16_t                uNodeID;
    bool                    fChannelTracingOn;
    bool                    fChannelTracingOnce;
    uint32_t                uTimeDebug_tick;
    uint32_t                _uTimeDebugPrevFast_tick;
    uint32_t                _uTimeDebugPrevSlow_tick;
    uint32_t                uDecimation;
    SDebugChannel           _rgRegisterRead[16];
    SDebugChannel           _rgRegisterWrite[16];
    SDebugMessageIn         _msgReceived;
    uint8_t                 _rgVersionApp[4];
    const char*             _szNodeName;
    const char*             _szSerialNr;
    uint8_t                 _rgDebugCharIn[DEBUG_STRING_IN_SIZE];
    uint32_t                _uIndexPushChar;
    uint32_t                _uIndexPopChar;
    uint8_t                 _uData;
    funcGetByte             pGetByte;
    funcWriteData           pWriteData;
    funcGetRegisterAddress  pGetRegisterAddress;
} SDebugProtocol;

/*******************************************************************
* Public Function Prototypes
*******************************************************************/
void DebugProt_Init(SDebugProtocol* pDebug, const uint8_t* rgVersionApp, const char* szNodeName, const char* szSerialNr, uint32_t uNodeID, funcGetByte pGetByte, funcWriteData pWriteData, funcGetRegisterAddress pGetRegisterAddress);
void DebugProt_DoMain(SDebugProtocol* pDebug);
void DebugProt_DoISR(SDebugProtocol* pDebug);
void DebugProt_AddReceivedData(SDebugProtocol* pDebug, uint8_t* rgData, uint32_t uSize);

void DebugProt_AssertFail(const char* szAssertion, const char* szFile, const int32_t nLineNr);
void DebugProt_Trace(const char* szString);
bool DebugProt_GetChar(char* pChar);

#ifdef __cplusplus
}
#endif

#endif //DEBUGPROTOCOL_H
