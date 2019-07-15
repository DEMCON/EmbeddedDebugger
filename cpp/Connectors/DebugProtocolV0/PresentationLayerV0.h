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

#ifndef PRESENTATIONLAYERV0_H
#define PRESENTATIONLAYERV0_H

#include <QVector>
#include "../BaseInterface/PresentationLayerBase.h"
class Register;


class PresentationLayerV0 : public PresentationLayerBase
{
    Q_OBJECT
public:
    explicit PresentationLayerV0(CpuListModel& cpuListModel, RegisterListModel& registerListModel, QObject *parent = nullptr);
    virtual ~PresentationLayerV0();

public slots:
    /**
     * @copydoc PresentationLayerBase::receivedDebugProtocolCommand
     */
    void receivedDebugProtocolCommand(uint8_t uCID, QVector<uint8_t> protocolCommand) override;

    /**
     * @brief Create a debug protocol command to scan all the Cpu`s
     */
    void scanForCpu();
    /**
     * @brief Create a debug protocol command to query a Register
     * @param Register that needs to be queried
     */
    void queryRegister(const Register& registerToRead);

    /**
     * @brief Create a debug protocol command to write a Register
     * @param Register that needs to be written
     */
    void writeRegister(const Register& registerToWrite);

    /**
     * @brief Create a debug protocol command to reset the time of a Cpu
     * @param uCId Id of the Cpu where the time needs to be reset from.
     */
    void resetTime(uint8_t uCId);

    /**
     * @brief Create a debug protocol command  to config a debug channel
     * @param Register that needs to be configured to be a debug channel
     */
    void configDebugChannel(Register &registerToConfigDebugChannel);

    /**
     * @brief Create a debug protocol command to get the decimation of a Cpu
     * @param uCId Cpu that you want the decimation from
     */
    void getDecimation(uint8_t uCId);

    /**
     * @brief Create a debug protocol command to set the decimation of a Cpu
     * @param  uCId Cpu where you want to set decimation
     * @param newDecimation decimation that you want to be set.
     */
    void setDecimation(uint8_t uCId, int newDecimation);

private:
    void receivedGetInfo(uint8_t uCId,QVector<uint8_t>& commandData);
    void receivedGetVersion(uint8_t& uCId,const QVector<uint8_t>& commandData);
    void receivedWriteRegister(uint8_t& uCId,const QVector<uint8_t>& commandData);
    void receivedQueryRegister(uint8_t& uCId,const QVector<uint8_t>& commandData);
    void receivedConfigChannel(uint8_t& uCId,const QVector<uint8_t>& commandData);
    void receivedDecimation(uint8_t uCId,const QVector<uint8_t>& commandData);
    void receivedReadChannelData(uint8_t uCId, QVector<uint8_t> &commandData);
    void receivedDebugString(uint8_t uCId,const QVector<uint8_t>& commandData);
    void sendGetVersion(uint8_t uCId);
    void sendGetInfo(uint8_t uCId);
    void disableAllConfigChannels(uint8_t uCId, uint8_t nbrOfConfigChannels);
    uint8_t controlByte(const Register& Register);
};

#endif // PRESENTATIONLAYERV0_H
