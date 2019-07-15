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

#ifndef PRESENTATIONLAYERBASE_H
#define PRESENTATIONLAYERBASE_H

#include <QObject>
#include <QVector>
#include <QVariant>
class Cpu;
#include "Medium/CPU/CpuListModel.h"
#include "../BaseInterface/Common.h"

class PresentationLayerBase : public QObject
{
    Q_OBJECT
public:

    explicit PresentationLayerBase(CpuListModel& cpuListModel, RegisterListModel& registerListModel, QObject* parent = nullptr) :
        QObject(parent),
        m_cpuListModel(cpuListModel),
        m_registerListModel(registerListModel){}

signals:

    /**
     * @brief Signal that is emitted when a new Cpu is found.
     * @param Cpu that is found.
     */
    void newCpuFound(Cpu* newCpu);

    /**
     * @brief Signal that is emitted when a new debug protocol command needs to be send
     * @param uCId id of the Cpu where the message came from.
     * @param protocolCommand QVector containing the data of the protocol
     */
    void newDebugProtocolCommand(uint8_t uCId, QVector<uint8_t> protocolCommand);

public slots:

    /**
     * @brief received debug proctocol command calls the correct function to parse the protocolCommand
     * @param uCID id of the Cpu where the message came from.
     * @param protocolCommand QVector containing the data of the protocol
     */
    virtual void receivedDebugProtocolCommand(uint8_t uCID, QVector<uint8_t> protocolCommand) = 0;

protected:
    CpuListModel& m_cpuListModel; /**< Reference to CpuListModel contains all Cpu`s from this medium */
    RegisterListModel& m_registerListModel; /**< Reference to RegisterListModel containing all Registers from this medium */
};

#endif // PRESENTATIONLAYERBASE_H
