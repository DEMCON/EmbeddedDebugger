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
#include "debugMessage.h"
#include <string.h>             //for using memset

#define STX             0x55
#define ETX             0xAA
#define ESC             0x66

#define INDEX_INC(X)    ++X; X &= ~(0xFFFFFFFF << DEBUG_BUF_IN_SIZE_BITS);
#define INDEX_DEC(X)    --X; X &= ~(0xFFFFFFFF << DEBUG_BUF_IN_SIZE_BITS);


//local function prototypes
static uint8_t CrcAdd(uint8_t uCRC, uint8_t uByte);
static bool CheckMsgIn(SDebugMessageIn* pMsg);


const uint8_t crcTable[ ] =
{
     0,  94, 188, 226,  97,  63, 221, 131, 194, 156, 126,  32, 163, 253,  31,  65,
   157, 195,  33, 127, 252, 162,  64,  30,  95,   1, 227, 189,  62,  96, 130, 220,
    35, 125, 159, 193,  66,  28, 254, 160, 225, 191,  93,   3, 128, 222,  60,  98,
   190, 224,   2,  92, 223, 129,  99,  61, 124,  34, 192, 158,  29,  67, 161, 255,
    70,  24, 250, 164,  39, 121, 155, 197, 132, 218,  56, 102, 229, 187,  89,   7,
   219, 133, 103,  57, 186, 228,   6,  88,  25,  71, 165, 251, 120,  38, 196, 154,
   101,  59, 217, 135,   4,  90, 184, 230, 167, 249,  27,  69, 198, 152, 122,  36,
   248, 166,  68,  26, 153, 199,  37, 123,  58, 100, 134, 216,  91,   5, 231, 185,
   140, 210,  48, 110, 237, 179,  81,  15,  78,  16, 242, 172,  47, 113, 147, 205,
    17,  79, 173, 243, 112,  46, 204, 146, 211, 141, 111,  49, 178, 236,  14,  80,
   175, 241,  19,  77, 206, 144, 114,  44, 109,  51, 209, 143,  12,  82, 176, 238,
    50, 108, 142, 208,  83,  13, 239, 177, 240, 174,  76,  18, 145, 207,  45, 115,
   202, 148, 118,  40, 171, 245,  23,  73,   8,  86, 180, 234, 105,  55, 213, 139,
    87,   9, 235, 181,  54, 104, 138, 212, 149, 203,  41, 119, 244, 170,  72,  22,
   233, 183,  85,  11, 136, 214,  52, 106,  43, 117, 151, 201,  74,  20, 246, 168,
   116,  42, 200, 150,  21,  75, 169, 247, 182, 232,  10,  84, 215, 137, 107,  53
};


uint8_t CrcAdd(uint8_t uCRC, uint8_t uByte)
{
    // current CRC XOR new byte -> table index
    int32_t index = uCRC ^ uByte;

    // new CRC from LUT
    return crcTable[index];
}


//----------------------------------------------------------------------------
//    MessageIn
//----------------------------------------------------------------------------
void DebugMsgIn_Init(SDebugMessageIn* pMsg)
{
    memset(pMsg, 0, sizeof(SDebugMessageIn));
}


void DebugMsgIn_AddReceivedData(SDebugMessageIn* pMsg, uint8_t* rgData, uint32_t uSize)
{
    uint32_t i;

    //copy the new data to the ring-buffer
    for (i = 0; i < uSize; ++i)
    {
        //put data in buffer
        pMsg->_rgRawMsgData[pMsg->_uIndexPush] = rgData[i];

        //goto next write-position in buffer
        INDEX_INC(pMsg->_uIndexPush);

        //check if we have a buffer-overrun
        if (pMsg->_uIndexPush == pMsg->_uIndexSTX)
        {
            pMsg->fBufferOverrun = true;
            INDEX_DEC(pMsg->_uIndexPush);
            break;
        }
    }
}


bool DebugMsgIn_DecodeAndCheck(SDebugMessageIn* pMsg)
{
    //restart parsing, reset result of previous parse
    pMsg->fValidMessage = false;

    //parse all non-parsed data
    while ((pMsg->fValidMessage == false) && (pMsg->_uIndexParseNext != pMsg->_uIndexPush))
    {
        //check for STX
        if (pMsg->_rgRawMsgData[pMsg->_uIndexParseNext] == STX)
        {
            if (pMsg->_fFoundSTX == true)
            {
                pMsg->_fFoundSTX = true;
            }
            pMsg->_uIndexSTX = pMsg->_uIndexParseNext;
            pMsg->_fFoundSTX = true;
        }

        //check for ETX
        if (pMsg->_rgRawMsgData[pMsg->_uIndexParseNext] == ETX)
        {
            pMsg->_uIndexETX = pMsg->_uIndexParseNext;

            //check if message is ok (check STX, CRC, fill in nodeID, cmd, cmdParam)
            pMsg->fValidMessage = CheckMsgIn(pMsg);
        }

        //goto next char
        INDEX_INC(pMsg->_uIndexParseNext);
    }

    return pMsg->fValidMessage;
}


bool CheckMsgIn(SDebugMessageIn* pMsg)
{
    bool fValidMsg = false;

    //we found an ETX, check if we also have a STX
    if (pMsg->_fFoundSTX == true)
    {
        int32_t nMsgSize;
        uint8_t uCRC, uNextByte;

        //decode the raw message-data, skip STX
        INDEX_INC(pMsg->_uIndexSTX);
        nMsgSize = 0;
        uCRC = 0;
        while (pMsg->_uIndexSTX != pMsg->_uIndexETX)
        {
            //get the next byte
            uNextByte = pMsg->_rgRawMsgData[pMsg->_uIndexSTX];

            //trap excape-chars
            if (uNextByte == ESC)
            {
                INDEX_INC(pMsg->_uIndexSTX);
                uNextByte = pMsg->_rgRawMsgData[pMsg->_uIndexSTX] ^ ESC;
            }

            //add the next (decoded) byte to the decoded list
            pMsg->rgMessage[nMsgSize] = uNextByte;
            //calc the CRC while decoding
            uCRC = CrcAdd(uCRC, uNextByte);

            //goto next array-positions
            INDEX_INC(pMsg->_uIndexSTX);
            ++nMsgSize;
        }

        //check if we have enough bytes between STX and ETX
        if (nMsgSize >= 4)
        {
            //calc parameter-size by removing overhead from message-size
            pMsg->nCmdParamSize = nMsgSize - 4;

            //check CRC (since CRC is included in the CRC-calc, it should always result in 0)
            if (uCRC == 0)
            {
                //get the uC nodeID and cmd
                pMsg->uNodeID = pMsg->rgMessage[0];
                pMsg->uMsgID = pMsg->rgMessage[1];
                pMsg->cmd = (EDebugCmd)pMsg->rgMessage[2];

                //ok, all checks are passed
                fValidMsg = true;
            }
            else
            {
                fValidMsg = false;
            }
        }
        else
        {
            fValidMsg = false;
        }
    }

    //if no valid message, ignore by restart looking for STX
    pMsg->_fFoundSTX = false;

    return fValidMsg;
}


//----------------------------------------------------------------------------
//    MessageOut
//----------------------------------------------------------------------------
void DebugMsgOut_Init(SDebugMessageOut* pMsg)
{
    memset(pMsg, 0, sizeof(SDebugMessageOut));
    pMsg->_uIndexMessage = 3;
}


bool DebugMsgOut_AddByte(SDebugMessageOut* pMsg, const uint8_t uData)
{
    //check if the new parameter will fit
    if (pMsg->_uIndexMessage + 1 > DEBUG_MSG_SIZE - 3)
    {
        return false;
    }

    pMsg->rgMessage[pMsg->_uIndexMessage] = uData;
    ++pMsg->_uIndexMessage;

    return true;
}


bool DebugMsgOut_AddData(SDebugMessageOut* pMsg, const uint8_t* rgParam, uint32_t uSize)
{
    int32_t i;

    //check if the new parameter will fit
    if (pMsg->_uIndexMessage + uSize > DEBUG_MSG_SIZE - 3)
    {
        return false;
    }

    //add the parameter-data
    for (i = 0; i < uSize; ++i)
    {
        pMsg->rgMessage[pMsg->_uIndexMessage] = rgParam[i];
        ++pMsg->_uIndexMessage;
    }

    return true;
}


void DebugMsgOut_Encode(SDebugMessageOut* pMsg)
{
    uint32_t i;
    uint8_t uCRC, uNextByte;

    //be sure to copy uC nodeID and cmd to message
    pMsg->rgMessage[0] = pMsg->uNodeID;
    pMsg->rgMessage[1] = (uint8_t)pMsg->uMsgID;
    pMsg->rgMessage[2] = (uint8_t)pMsg->cmd;

    pMsg->_uIndexRawData = 0;

    //start with STX
    pMsg->_rgRawMsgData[pMsg->_uIndexRawData] = STX;
    pMsg->_uIndexRawData++;

    //encode the message to buffer (calc CRC while encoding)
    uCRC = 0;
    for (i = 0; i < pMsg->_uIndexMessage; ++i)
    {
        //get next byte
        uNextByte = pMsg->rgMessage[i];
        //add to crc
        uCRC = CrcAdd(uCRC, uNextByte);
        //encode if necessary
        if ((uNextByte == STX) || (uNextByte == ETX) || (uNextByte == ESC))
        {
            pMsg->_rgRawMsgData[pMsg->_uIndexRawData] = ESC;
            pMsg->_uIndexRawData++;

            pMsg->_rgRawMsgData[pMsg->_uIndexRawData] = uNextByte ^ ESC;
            pMsg->_uIndexRawData++;
        }
        else
        {
            pMsg->_rgRawMsgData[pMsg->_uIndexRawData] = uNextByte;
            pMsg->_uIndexRawData++;
        }
    }

    //add CRC (encode if necessary)
    if ((uCRC == STX) || (uCRC == ETX) || (uCRC == ESC))
    {
        pMsg->_rgRawMsgData[pMsg->_uIndexRawData] = ESC;
        pMsg->_uIndexRawData++;
        pMsg->_rgRawMsgData[pMsg->_uIndexRawData] = uCRC ^ ESC;
        pMsg->_uIndexRawData++;
    }
    else
    {
        pMsg->_rgRawMsgData[pMsg->_uIndexRawData] = uCRC;
        pMsg->_uIndexRawData++;
    }

    //end with ETX
    pMsg->_rgRawMsgData[pMsg->_uIndexRawData] = ETX;
    pMsg->_uIndexRawData++;

    //be ready for new message
    pMsg->_uIndexMessage = 3;
}
