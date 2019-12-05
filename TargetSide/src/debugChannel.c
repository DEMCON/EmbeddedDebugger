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
#include "debugChannel.h"
#include <string.h>             //for using memset and memcmp


void DbgChan_Init(SDebugChannel* pChan)
{
    memset(pChan, 0, sizeof(SDebugChannel));
}


void DbgChan_WriteValue(SDebugChannel* pChan, uint8_t* pValueNew)
{
    int32_t i;
    void* pValue;

    //check if we have a valid source
    pValue = pChan->pSource;
    if (pValue == NULL)
    {
        return;
    }

    //dereference the proper number of times
    for (i = 0; i < pChan->uPointerDepth; ++i)
    {
        //trap null-pounters
        if (pValue == NULL)
        {
            return;
        }

        //dereference 1 level
        pValue = *((void**)pValue);
    }

    //write (copy) the actual data
    memcpy(pChan->pSource, pValueNew, pChan->uSize_bytes);
}


bool DbgChan_ReadValue(SDebugChannel* pChan, uint8_t* pValueRead)
{
    int32_t i;
    void* pValue;
    bool fValueChanged;

    //check if we have a valid source
    pValue = pChan->pSource;
    if (pValue == NULL)
    {
        return false;
    }

    //dereference the proper number of times
    for (i = 0; i < pChan->uPointerDepth; ++i)
    {
        //trap null-pounters
        if (pValue == NULL)
        {
            return false;
        }

        //dereference 1 level
        pValue = *((void**)pValue);
    }

    //always read (copy) the actual data
    memcpy(pValueRead, pValue, pChan->uSize_bytes);

    //check if the value has changed by comparing memory
    fValueChanged = (memcmp(pValue, pChan->rgValuePrev, pChan->uSize_bytes) != 0) ? true : false;
    if (fValueChanged)
    {
        //remember the new value for next time
        memcpy(pChan->rgValuePrev, pValue, pChan->uSize_bytes);
    }

    //return true if we have a new value
    return fValueChanged;
}
