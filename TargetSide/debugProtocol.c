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
#include "debugProtocol.h"
#include <string.h>

//defines
#define REC_SEPARATOR           (0x33)
#define INDEX_INC(X)            ++X; X &= ~(0xFFFFFFFF << DEBUG_STRING_IN_SIZE_BITS);
#define INDEX_DEC(X)            --X; X &= ~(0xFFFFFFFF << DEBUG_STRING_IN_SIZE_BITS);
#define DEBUG_SLOW_UPDATE_MS    (1000)

//#define ASSERT (void)0;

//global variables
const uint8_t   g_rgVersionDebug[4] = {0x00, 0x05, 0x00, 0x00};     //major, minor, build-LSB, build-MSB
SDebugProtocol* g_pProtDebug        = NULL;

extern uint32_t time;
//local function prototypes
static void Dispatch(SDebugProtocol* pDebug);
static void CmdVersion(SDebugProtocol* pDebug, SDebugMessageOut* pMsgReply);
static void CmdInfo(SDebugProtocol* pDebug, SDebugMessageOut* pMsgReply);
static void CmdWriteRegister(SDebugProtocol* pDebug, SDebugMessageOut* pMsgReply);
static void CmdQueryRegister(SDebugProtocol* pDebug, SDebugMessageOut* pMsgReply);
static void CmdConfigChannel(SDebugProtocol* pDebug, SDebugMessageOut* pMsgReply);
static void CmdDecimation(SDebugProtocol* pDebug, SDebugMessageOut* pMsgReply);
static void CmdResetTime(SDebugProtocol* pDebug, SDebugMessageOut* pMsgReply);
static void CmdReadChannelData(SDebugProtocol* pDebug, SDebugMessageOut* pMsgReply);
static void CmdDebugString(SDebugProtocol* pDebug, SDebugMessageOut* pMsgReply);

static void SendChannelData(SDebugProtocol* pDebug, bool fSlowUpdate);
static void SendMessage(SDebugProtocol* pDebug, SDebugMessageOut* pMsg);


void DebugProt_Init(
    SDebugProtocol*         pDebug,
    const uint8_t*          rgVersionApp,
    const char*             szNodeName,
    const char*             szSerialNr,
    uint32_t                uNodeID,
    funcGetByte             pGetByte,
    funcWriteData           pWriteData,
    funcGetRegisterAddress  pGetRegisterAddress
    )
{
    //store access to this debugger, uC node-ID, application-version, app protocol
    g_pProtDebug = pDebug;
    memcpy(pDebug->_rgVersionApp, rgVersionApp, 4);
    pDebug->_szNodeName = szNodeName;
    pDebug->_szSerialNr = szSerialNr;
    pDebug->uNodeID = uNodeID;
    pDebug->pGetByte = pGetByte;
    pDebug->pWriteData = pWriteData;
    pDebug->pGetRegisterAddress = pGetRegisterAddress;
    pDebug->uDecimation = 1;
    pDebug->fChannelTracingOn = true;

    //init children
    DebugMsgIn_Init(&pDebug->_msgReceived);
}


void DebugProt_DoMain(SDebugProtocol* pDebug)
{
    uint32_t dT_tick;

    //check for debug-messages, and dispatch messages that are complete
    while (DebugMsgIn_DecodeAndCheck(&pDebug->_msgReceived) == true)
    {
        Dispatch(pDebug);
    }

    //check if we need to send new fast-update channel-data
    dT_tick = pDebug->uTimeDebug_tick - pDebug->_uTimeDebugPrevFast_tick;
    if ((pDebug->fChannelTracingOn || pDebug->fChannelTracingOnce) && (dT_tick >= pDebug->uDecimation))
    {
        //check if we also need to send slow-update channel-data
        if (pDebug->uTimeDebug_tick - pDebug->_uTimeDebugPrevSlow_tick >= ((uint32_t)(1000) * DEBUG_SLOW_UPDATE_MS / 1000))
        {
            SendChannelData(pDebug, true);
            pDebug->_uTimeDebugPrevFast_tick = pDebug->uTimeDebug_tick;
            pDebug->_uTimeDebugPrevSlow_tick = pDebug->uTimeDebug_tick;
        }
        else
        {
            SendChannelData(pDebug, false);
            pDebug->_uTimeDebugPrevFast_tick = pDebug->uTimeDebug_tick;
        }

        //reset the tracing-once mode
        pDebug->fChannelTracingOnce = false;
    }
}


void DebugProt_DoISR(SDebugProtocol* pDebug)
{
    uint8_t data = 0;
    //increase the internal debug-time
    ++pDebug->uTimeDebug_tick;


    if (pDebug->pGetByte(&data))
    {
        DebugProt_AddReceivedData(pDebug,&data,1);
    }
}


void DebugProt_AddReceivedData(SDebugProtocol* pDebug, uint8_t* rgData, uint32_t uSize)
{
    //store data in message-buffer
    DebugMsgIn_AddReceivedData(&pDebug->_msgReceived, rgData, uSize);
}


void Dispatch(SDebugProtocol* pDebug)
{
    SDebugMessageOut msgReply;

    //check if the message is for us
    if ((pDebug->_msgReceived.uNodeID != pDebug->uNodeID) && (pDebug->_msgReceived.uNodeID != 0xFF))
    {
        //discard the message
        return;
    }

    //copy uC nodeID, msgID, command
    DebugMsgOut_Init(&msgReply);
    msgReply.uNodeID = pDebug->uNodeID;
    msgReply.uMsgID = pDebug->_msgReceived.uMsgID;
    msgReply.cmd = pDebug->_msgReceived.cmd;

    //dispatch the command
    switch (pDebug->_msgReceived.cmd)
    {
        case cmdVersion:            CmdVersion(pDebug, &msgReply);          break;
        case cmdInfo:               CmdInfo(pDebug, &msgReply);             break;
        case cmdWriteRegister:      CmdWriteRegister(pDebug, &msgReply);    break;
        case cmdQueryRegister:      CmdQueryRegister(pDebug, &msgReply);    break;
        case cmdConfigChannel:      CmdConfigChannel(pDebug, &msgReply);    break;
        case cmdDecimation:         CmdDecimation(pDebug, &msgReply);       break;
        case cmdResetTime:          CmdResetTime(pDebug, &msgReply);        break;
        case cmdReadChannelData:    CmdReadChannelData(pDebug, &msgReply);  break;
        case cmdDebugString:        CmdDebugString(pDebug, &msgReply);      break;
        default:                                                            break;  //ignore, do nothing
    }

    //send the reply-message (either ACK, or reply-data)
    if ((msgReply.uMsgID != 0) || (msgReply._uIndexMessage > 3))
    {
        SendMessage(pDebug, &msgReply);
    }
}


void CmdVersion(SDebugProtocol* pDebug, SDebugMessageOut* pMsgReply)
{
    uint8_t uStrLength;

    //check for valid pointers
    ASSERT(pMsgReply != NULL);

    //copy version-info of debugger (proper endianess of build-UINT16
    DebugMsgOut_AddByte(pMsgReply, g_rgVersionDebug[0]);
    DebugMsgOut_AddByte(pMsgReply, g_rgVersionDebug[1]);
    DebugMsgOut_AddByte(pMsgReply, g_rgVersionDebug[2]);
    DebugMsgOut_AddByte(pMsgReply, g_rgVersionDebug[3]);

    //copy version-info of application
    DebugMsgOut_AddByte(pMsgReply, pDebug->_rgVersionApp[0]);
    DebugMsgOut_AddByte(pMsgReply, pDebug->_rgVersionApp[1]);
    DebugMsgOut_AddByte(pMsgReply, pDebug->_rgVersionApp[2]);
    DebugMsgOut_AddByte(pMsgReply, pDebug->_rgVersionApp[3]);

    //copy name of the node
    uStrLength = strlen(pDebug->_szNodeName);
    DebugMsgOut_AddByte(pMsgReply, uStrLength);
    DebugMsgOut_AddData(pMsgReply, (uint8_t*)pDebug->_szNodeName, uStrLength);

    //copy serial-number string
    uStrLength = strlen(pDebug->_szSerialNr);
    DebugMsgOut_AddByte(pMsgReply, uStrLength);
    DebugMsgOut_AddData(pMsgReply, (uint8_t*)pDebug->_szSerialNr, uStrLength);
}


void CmdInfo(SDebugProtocol* pDebug, SDebugMessageOut* pMsgReply)
{
    //check for valid pointers
    ASSERT(pMsgReply != NULL);

    //byte alignment
    DebugMsgOut_AddByte(pMsgReply, 0);
    DebugMsgOut_AddByte(pMsgReply, &pDebug->nDummyForAlignment1 - &pDebug->nDummyForAlignment0);
    DebugMsgOut_AddByte(pMsgReply, REC_SEPARATOR);

    //pointer
    DebugMsgOut_AddByte(pMsgReply, 1);
    DebugMsgOut_AddByte(pMsgReply, sizeof(void*));
    DebugMsgOut_AddByte(pMsgReply, REC_SEPARATOR);

    //boolean
    DebugMsgOut_AddByte(pMsgReply, 2);
    DebugMsgOut_AddByte(pMsgReply, sizeof(bool));
    DebugMsgOut_AddByte(pMsgReply, REC_SEPARATOR);

    //char
    DebugMsgOut_AddByte(pMsgReply, 3);
    DebugMsgOut_AddByte(pMsgReply, sizeof(char));
    DebugMsgOut_AddByte(pMsgReply, REC_SEPARATOR);

    //short
    DebugMsgOut_AddByte(pMsgReply, 4);
    DebugMsgOut_AddByte(pMsgReply, sizeof(short));
    DebugMsgOut_AddByte(pMsgReply, REC_SEPARATOR);

    //int
    DebugMsgOut_AddByte(pMsgReply, 5);
    DebugMsgOut_AddByte(pMsgReply, sizeof(int));
    DebugMsgOut_AddByte(pMsgReply, REC_SEPARATOR);

    //long
    DebugMsgOut_AddByte(pMsgReply, 6);
    DebugMsgOut_AddByte(pMsgReply, sizeof(long));
    DebugMsgOut_AddByte(pMsgReply, REC_SEPARATOR);

    //float
    DebugMsgOut_AddByte(pMsgReply, 7);
    DebugMsgOut_AddByte(pMsgReply, sizeof(float));
    DebugMsgOut_AddByte(pMsgReply, REC_SEPARATOR);

    //double
    DebugMsgOut_AddByte(pMsgReply, 8);
    DebugMsgOut_AddByte(pMsgReply, sizeof(double));
    DebugMsgOut_AddByte(pMsgReply, REC_SEPARATOR);

    //long double
    DebugMsgOut_AddByte(pMsgReply, 9);
    DebugMsgOut_AddByte(pMsgReply, sizeof(double));
    DebugMsgOut_AddByte(pMsgReply, REC_SEPARATOR);

    //time-stamp units
    DebugMsgOut_AddByte(pMsgReply, 10);
    DebugMsgOut_AddByte(pMsgReply, (((uint32_t)1000) >>  0) & 0xFF);   //LSB
    DebugMsgOut_AddByte(pMsgReply, (((uint32_t)1000) >>  8) & 0xFF);
    DebugMsgOut_AddByte(pMsgReply, (((uint32_t)1000) >> 16) & 0xFF);
    DebugMsgOut_AddByte(pMsgReply, (((uint32_t)1000) >> 24) & 0xFF);   //MSB
}


void CmdWriteRegister(SDebugProtocol* pDebug, SDebugMessageOut* pMsgReply)
{
    SDebugChannel debugChannel;

    //extract offset and control-byte from received message
    DbgChan_Init(&debugChannel);
    memcpy((uint8_t*)&debugChannel._uOffset, &pDebug->_msgReceived.rgMessage[3], 4);
    debugChannel._uCtrl = pDebug->_msgReceived.rgMessage[7];

    //get actual variable address
    pDebug->pGetRegisterAddress(&debugChannel);

    //get the size of the new value
    debugChannel.uSize_bytes = pDebug->_msgReceived.rgMessage[8];

    //write the new value
    DbgChan_WriteValue(&debugChannel, &pDebug->_msgReceived.rgMessage[9]);
}


void CmdQueryRegister(SDebugProtocol* pDebug, SDebugMessageOut* pMsgReply)
{
    SDebugChannel debugChannel;
    uint8_t rgValue[8];

    //extract offset and control-byte from received message
    DbgChan_Init(&debugChannel);
    memcpy((uint8_t*)&debugChannel._uOffset, &pDebug->_msgReceived.rgMessage[3], 4);
    debugChannel._uCtrl = pDebug->_msgReceived.rgMessage[7];
    //get the size of the register
    debugChannel.uSize_bytes = pDebug->_msgReceived.rgMessage[8];

    //get actual variable address
    pDebug->pGetRegisterAddress(&debugChannel);

    //get the value of the register
    DbgChan_ReadValue(&debugChannel, rgValue);

    //construct a reply: add the parameters of the quesry-cmd to the reply
    DebugMsgOut_AddData(pMsgReply, &pDebug->_msgReceived.rgMessage[3], 6);
    //add the value of the register to the reply
    DebugMsgOut_AddData(pMsgReply, rgValue, 4);
}


void CmdConfigChannel(SDebugProtocol* pDebug, SDebugMessageOut* pMsgReply)
{
    uint8_t uChan;
    SDebugChannel* pChan;

    //get the proper channel-nr
    uChan = pDebug->_msgReceived.rgMessage[3];

    //ignore invalid channel-nrs
    if (uChan > 0x0F)
    {
        return;
    }

    //get access to the proper channel (increase redability)
    pChan = &pDebug->_rgRegisterRead[uChan];

    switch (pDebug->_msgReceived.nCmdParamSize)
    {
        case 1:
        {
            //--- query current settings ---
            //add channel to reply
            DebugMsgOut_AddByte(pMsgReply, uChan);
            //add mode to reply
            DebugMsgOut_AddByte(pMsgReply, (uint8_t)pChan->updateMode);
            //add offset to reply
            DebugMsgOut_AddData(pMsgReply, (uint8_t*)(&pChan->_uOffset), 4);
            //add control-uint8_t to reply
            DebugMsgOut_AddByte(pMsgReply, (uint8_t)pChan->_uCtrl);
            //add data-size to reply
            DebugMsgOut_AddByte(pMsgReply, (uint8_t)pChan->uSize_bytes);
            break;
        }

        case 2:
        {
            //--- only change the update-mode ---
            pChan->updateMode = (EUpdateMode)pDebug->_msgReceived.rgMessage[4];
            //turn tracing once on if necessary
            if (pChan->updateMode == updateOnce)
            {
                pDebug->fChannelTracingOnce = true;
            }
            //add channel to reply
            DebugMsgOut_AddByte(pMsgReply, uChan);
            //add (new) mode to reply
            DebugMsgOut_AddByte(pMsgReply, (uint8_t)pChan->updateMode);
            break;
        }

        default:
        {
            //--- completely configure channel ---
            DbgChan_Init(pChan);
            //set new update-mode
            pChan->updateMode = (EUpdateMode)pDebug->_msgReceived.rgMessage[4];
            //turn tracing once on if necessary
            if (pChan->updateMode == updateOnce)
            {
                pDebug->fChannelTracingOnce = true;
            }
            //set new offset
            memcpy((uint8_t*)(&pChan->_uOffset), &pDebug->_msgReceived.rgMessage[5], 4);
            //set new control-uint8_t
            pChan->_uCtrl = pDebug->_msgReceived.rgMessage[9];
            //set new data-size
            pChan->uSize_bytes = pDebug->_msgReceived.rgMessage[10];
            //set the pointer-depth
            pChan->uPointerDepth = pChan->_uCtrl & 0x0F;
            //get source-address from application
            if (pDebug->pGetRegisterAddress != NULL)
            {
                pDebug->pGetRegisterAddress(pChan);
            }
            //make sure to send at least 1 time, by aktering 1 LSB byte
            pChan->rgValuePrev[0] ^= 0xFF;
            //add channel to reply
            DebugMsgOut_AddByte(pMsgReply, uChan);
            break;
        }
    }
}


void CmdDecimation(SDebugProtocol* pDebug, SDebugMessageOut* pMsgReply)
{
    //check for valid pointers
    ASSERT(pMsgReply != NULL);

    //set the new decimation
    if (pDebug->_msgReceived.nCmdParamSize == 1)
    {
        pDebug->uDecimation = (uint32_t)pDebug->_msgReceived.rgMessage[3];
        if (pDebug->uDecimation == 0)
        {
            pDebug->uDecimation = 1;
        }
    }

    //reply with the current decimation
    DebugMsgOut_AddByte(pMsgReply, (uint8_t)pDebug->uDecimation);
}


void CmdResetTime(SDebugProtocol* pDebug, SDebugMessageOut* pMsgReply)
{
    //reset the debug-time
    pDebug->uTimeDebug_tick = pDebug->_uTimeDebugPrevFast_tick = pDebug->_uTimeDebugPrevSlow_tick = 0;

    //reply with the same message
}


void CmdReadChannelData(SDebugProtocol* pDebug, SDebugMessageOut* pMsgReply)
{
    //set channel-data tracing-mode
    switch (pDebug->_msgReceived.rgMessage[3])
    {
        default:
            pDebug->fChannelTracingOn = false;
            break;
        case 1:
            pDebug->fChannelTracingOn = true;
            break;
        case 2:
            pDebug->fChannelTracingOnce = true;
            break;
    }

    //no reply
}


void CmdDebugString(SDebugProtocol* pDebug, SDebugMessageOut* pMsgReply)
{
    int32_t i;

    //check for valid pointers
    ASSERT(pMsgReply != NULL);

    //add the new received chars to buffer
    for (i = 0; i < pDebug->_msgReceived.nCmdParamSize; ++i)
    {
        pDebug->_rgDebugCharIn[pDebug->_uIndexPushChar] = pDebug->_msgReceived.rgMessage[i + 3];

        //goto next write-position in buffer
        INDEX_INC(pDebug->_uIndexPushChar);

        //check if we have a buffer-overrun, ignore extra received chars
        if (pDebug->_uIndexPushChar == pDebug->_uIndexPopChar)
        {
            INDEX_DEC(pDebug->_uIndexPushChar);
            break;
        }
    }
}


void SendChannelData(SDebugProtocol* pDebug, bool fSlowUpdate)
{
    int32_t i;
    SDebugChannel* pDbgChan;
    SDebugMessageOut msgOut;
    uint8_t rgValue[8];
    uint16_t uNewDataMask;
    bool fNeedUpdate;
    bool fForceUpdate;
    bool fChanged;

    //check for valid pointers
    ASSERT(pDebug->pWriteData != NULL);

    //create new message
    DebugMsgOut_Init(&msgOut);
    msgOut.uNodeID = pDebug->uNodeID;
    msgOut.uMsgID = 0;
    msgOut.cmd = cmdReadChannelData;

    //create timestamp
    rgValue[0] = (pDebug->uTimeDebug_tick >>  0) & 0xFF;
    rgValue[1] = (pDebug->uTimeDebug_tick >>  8) & 0xFF;
    rgValue[2] = (pDebug->uTimeDebug_tick >> 16) & 0xFF;
    DebugMsgOut_AddData(&msgOut, rgValue, 3);

    //create mask and values
    uNewDataMask = 0;
    DebugMsgOut_AddData(&msgOut, (uint8_t*)(&uNewDataMask), 2);
    for (i = 15; i >= 0; --i)
    {
        //get access to the debug-channel (increase readability)
        pDbgChan = &pDebug->_rgRegisterRead[i];
        //check if we need to send data for this channel
        fNeedUpdate   = (pDbgChan->updateMode == updateFast);


        // the force update flag will always send an update
        fForceUpdate  = (pDbgChan->updateMode == updateAll );
        fForceUpdate |= (pDbgChan->updateMode == updateSlow) && (fSlowUpdate == true);
        fForceUpdate |= (pDbgChan->updateMode == updateOnce) && (pDebug->fChannelTracingOnce == true);
//         //reset updateOnce flag if necessary
//         if (pDbgChan->updateMode == updateOnce)
//         {
//             pDbgChan->updateMode = updateOff;
//         }
        //if we need to update, check if the value-data has changed. In case of a force update always send a new value
        fChanged = DbgChan_ReadValue(pDbgChan, rgValue);
        if ( (fNeedUpdate && fChanged) ||
              fForceUpdate
            )
        {
            //set the bit in the mask
            uNewDataMask |= (0x0001 << i);
            //add the value to the message
            DebugMsgOut_AddData(&msgOut, rgValue, pDbgChan->uSize_bytes);
        }
    }

    //send the data if it is not empty
    if (uNewDataMask != 0)
    {
        //insert the channel-mask at the proper location in the message
        msgOut.rgMessage[6] = uNewDataMask & 0xFF;
        msgOut.rgMessage[7] = uNewDataMask >> 8;

        //encode the message
        DebugMsgOut_Encode(&msgOut);

        //send the new message over the debug-protocol
        pDebug->pWriteData( msgOut._rgRawMsgData, msgOut._uIndexRawData );
    }
}


void SendMessage(SDebugProtocol* pDebug, SDebugMessageOut* pMsg)
{
    //check for valid pointers
    ASSERT(pDebug->pWriteData != NULL);

    //encode the message
    DebugMsgOut_Encode(pMsg);

    //send the new message over the debug-protocol
    pDebug->pWriteData( pMsg->_rgRawMsgData, pMsg->_uIndexRawData );
}


//----------------------------------------------------------------------------
//    Debug tools
//----------------------------------------------------------------------------
void DebugProt_AssertFail(const char* szAssertion, const char* szFile, const int32_t nLineNr)
{
    char szLineNr[5];
    int32_t nStringPos = 0;
    int32_t nLine;
    int32_t i;

    //convert line-nr to string
    nLine = nLineNr;
    i = nLine / 1000;
    nLine -= i * 1000;
    if (i > 0)
    {
        szLineNr[nStringPos++] = i + '0';
    }
    i = nLine / 100;
    nLine -= i * 100;
    if ((i > 0) || (nStringPos > 0))
    {
        szLineNr[nStringPos++] = i + '0';
    }
    i = nLine / 10;
    nLine -= i * 10;
    if ((i > 0) || (nStringPos > 0))
    {
        szLineNr[nStringPos++] = i + '0';
    }
    szLineNr[nStringPos++] = nLine + '0';
    szLineNr[nStringPos++] = '\0';

    //output an assertion debug-string
    DebugProt_Trace("Assert fail: '");
    DebugProt_Trace(szAssertion);
    DebugProt_Trace("' (file: ");
    DebugProt_Trace(szFile);
    DebugProt_Trace(", line: ");
    DebugProt_Trace(szLineNr);
    DebugProt_Trace(")\r\n");
}


void DebugProt_Trace(const char* szString)
{
    SDebugMessageOut msg;

    //check if we have access to the (global) debug protocol
    if (g_pProtDebug == NULL)
    {
        ASSERT(g_pProtDebug != NULL);
        return;
    }

    //create new message
    DebugMsgOut_Init(&msg);
    msg.uNodeID = g_pProtDebug->uNodeID;
    msg.cmd = cmdDebugString;
    DebugMsgOut_AddData(&msg, (const uint8_t*)szString, strlen(szString));

    //send message
    SendMessage(g_pProtDebug, &msg);
}


bool DebugProt_GetChar(char* pChar)
{
    //check for valid pointers
    ASSERT(g_pProtDebug != NULL);
    ASSERT(pChar != NULL);

    //check if we have access to the (global) debug protocol
    if (g_pProtDebug == NULL)
    {
        //indicate no char to read
        return false;
    }

    //check if we have a char in the buffer that was not read yet
    if (g_pProtDebug->_uIndexPopChar != g_pProtDebug->_uIndexPushChar)
    {
        //read the char
        *pChar = g_pProtDebug->_rgDebugCharIn[g_pProtDebug->_uIndexPopChar];

        //increase read-position of ring-buffer
        INDEX_INC(g_pProtDebug->_uIndexPopChar);

        //indicate we have read a char
        return true;
    }

    //indicate no char to read
    return false;
}
