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

#include "PresentationLayerV0.h"
#include "../DebugProtocolV0/DebugProtocolV0Enums.h"
#include <QDebug>
#include <QVector>
#include "Medium/CPU/CpuListModel.h"

PresentationLayerV0::PresentationLayerV0(CpuListModel& cpuListModel, RegisterListModel& registerListModel, QObject *parent) :
    PresentationLayerBase(cpuListModel,registerListModel, parent)
{

}

PresentationLayerV0::~PresentationLayerV0()
{
    qDebug() << "presentation Layer destroyed";
}

void PresentationLayerV0::receivedDebugProtocolCommand(uint8_t uCID, QVector<uint8_t> protocolCommand)
{
    switch(protocolCommand.takeFirst())
    {
    case DebugProtocolV0Enums::ProtocolCommand::GetVersion:
    {
        receivedGetVersion(uCID, protocolCommand);
        break;
    }
    case DebugProtocolV0Enums::ProtocolCommand::GetInfo:
    {
        receivedGetInfo(uCID,protocolCommand);
        break;
    }
    case DebugProtocolV0Enums::ProtocolCommand::WriteRegister:
    {
        receivedWriteRegister(uCID,protocolCommand);
        break;
    }
    case DebugProtocolV0Enums::ProtocolCommand::QueryRegister:
    {
        receivedQueryRegister(uCID,protocolCommand);
        break;
    }
    case DebugProtocolV0Enums::ProtocolCommand::ReadChannelData:
    {
        receivedReadChannelData(uCID,protocolCommand);
        break;
    }

    default:
    {

        break;
    }
    }
}

void PresentationLayerV0::scanForCpu()
{
    QVector<uint8_t> debugProtocolMessage;
    debugProtocolMessage.append(DebugProtocolV0Enums::GetVersion);
    emit newDebugProtocolCommand(0xFF, debugProtocolMessage); //Send GetVersion to all cpu's
    qDebug() << "Send scanForCpu";
}



void PresentationLayerV0::queryRegister(const Register &registerToRead)
{
    QVector<uint8_t> newDebugProtocolMessage;
    newDebugProtocolMessage.append(DebugProtocolV0Enums::QueryRegister);
    append32BitValue(newDebugProtocolMessage, registerToRead.offset());
    newDebugProtocolMessage.append(controlByte(registerToRead));
    newDebugProtocolMessage.append(registerToRead.getVariableTypeSize());
    emit newDebugProtocolCommand(registerToRead.cpu().id(),newDebugProtocolMessage);
}

void PresentationLayerV0::writeRegister(const Register &registerToWrite)
{
    QVector<uint8_t> newDebugProtocolMessage;
    newDebugProtocolMessage.append(DebugProtocolV0Enums::WriteRegister);
    append32BitValue(newDebugProtocolMessage, registerToWrite.offset());
    newDebugProtocolMessage.append(controlByte(registerToWrite));
    newDebugProtocolMessage.append(registerToWrite.getVariableTypeSize());
    newDebugProtocolMessage.append(toQVector(registerToWrite.value()));
    emit newDebugProtocolCommand(registerToWrite.cpu().id(),newDebugProtocolMessage);
}

void PresentationLayerV0::resetTime(uint8_t uCId)
{
    QVector<uint8_t> newDebugProtocolMessage;
    newDebugProtocolMessage.append(DebugProtocolV0Enums::ResetTime);
    emit newDebugProtocolCommand(uCId,newDebugProtocolMessage);

}

void PresentationLayerV0::configDebugChannel(Register &registerToConfigDebugChannel)
{
    QVector<uint8_t> newDebugProtocolMessage;
    newDebugProtocolMessage.append(DebugProtocolV0Enums::ConfigChannel);
    int debugChannel = registerToConfigDebugChannel.cpu().debugChannels().indexOf(&registerToConfigDebugChannel);
    if (debugChannel >= 0)
    {
        //Debugchannel already exists. only need to change channelmode
        newDebugProtocolMessage.append(static_cast<uint8_t>(debugChannel));
        newDebugProtocolMessage.append(static_cast<uint8_t>(registerToConfigDebugChannel.channelMode()));
        emit newDebugProtocolCommand(registerToConfigDebugChannel.cpu().id(),newDebugProtocolMessage);

        if (registerToConfigDebugChannel.channelMode() == Register::ChannelMode::Off)
        {
            registerToConfigDebugChannel.cpu().debugChannels().removeOne(&registerToConfigDebugChannel);
        }
    }
    else
    {
        //Debugchannel does not exists.
        debugChannel = registerToConfigDebugChannel.cpu().nextDebugChannel();
        if(debugChannel >= 0)
        {
            registerToConfigDebugChannel.cpu().debugChannels().append(&registerToConfigDebugChannel);
            newDebugProtocolMessage.append(static_cast<uint8_t>(debugChannel));
            newDebugProtocolMessage.append(static_cast<uint8_t>(registerToConfigDebugChannel.channelMode()));
            append32BitValue(newDebugProtocolMessage, registerToConfigDebugChannel.offset());
            newDebugProtocolMessage.append(controlByte(registerToConfigDebugChannel));
            newDebugProtocolMessage.append(registerToConfigDebugChannel.getVariableTypeSize());
            emit newDebugProtocolCommand(registerToConfigDebugChannel.cpu().id(),newDebugProtocolMessage);
        }
    }
}

void PresentationLayerV0::getDecimation(uint8_t uCId)
{
    QVector<uint8_t> debugProtocolMessage;
    debugProtocolMessage.append(DebugProtocolV0Enums::Decimation);
    emit newDebugProtocolCommand(uCId, debugProtocolMessage);
}

void PresentationLayerV0::setDecimation(uint8_t uCId,int newDecimation)
{
    QVector<uint8_t> debugProtocolMessage;
    debugProtocolMessage.append(DebugProtocolV0Enums::Decimation);
    debugProtocolMessage.append(toQVector(newDecimation));
    emit newDebugProtocolCommand(uCId, debugProtocolMessage);
}

void PresentationLayerV0::receivedGetVersion(uint8_t &uCId, const QVector<uint8_t> &commandData)
{
    qDebug() << "ReceivedGetVersion";
    if(commandData.size() < 9)
    {
        qWarning() << "Message too short for version message";
    }
    else
    {
        uint8_t id = uCId;
        QString protocolVersion;
        QString applicationVersion;
        QString name;
        QString serialNumber;

        protocolVersion = QStringLiteral("%1.%2.%3.%4").arg(QString::number(commandData.value(0)),
                                                            QString::number(commandData.value(1)),
                                                            QString::number(commandData.value(2)),
                                                            QString::number(commandData.value(3)));

        applicationVersion = QStringLiteral("%1.%2.%3.%4").arg(QString::number(commandData.value(4)),
                                                               QString::number(commandData.value(5)),
                                                               QString::number(commandData.value(6)),
                                                               QString::number(commandData.value(7)));

        uint8_t nameLength = commandData.value(8);
        for(int i = 9; i < 9+nameLength ; i++)
        {
            name.append(commandData.value(i));
        }

        uint8_t serialLength = commandData.value(9 + nameLength);
        for(int i = 10 + nameLength; i < 10 + nameLength + serialLength; i++)
        {
            serialNumber.append(commandData.value(i));
        }

        auto* cpu = new Cpu(id,name,serialNumber,protocolVersion,applicationVersion);
        cpu->increaseMessageCounter();
        cpu->loadConfiguration();
        emit newCpuFound(cpu);
        //Disable All Cpu debugChannels
        disableAllConfigChannels(id,cpu->maxDebugChannels());
        sendGetInfo(cpu->id());
    }
}

void PresentationLayerV0::receivedWriteRegister(uint8_t &uCId, const QVector<uint8_t> &commandData)
{
    if(commandData.size() == 1)
    {
        switch(commandData[0])
        {
        case 0x00: break; //ok, value is written
        case 0x01: qWarning() << "Invalid (offset) address at writing value"; break;
        case 0x02: qWarning() << "error dereferencing (null-pointer appeared at some dereference)"; break;
        default: qWarning() << "received unknown writeRegister result valuez"; break;
        }
    }
}

void PresentationLayerV0::receivedQueryRegister(uint8_t &uCId, const QVector<uint8_t> &commandData)
{
    if(commandData.size() < 7)
    {
        qWarning() << "Received query register commmand from uC: " << uCId << " is invalid";
    }
    else
    {
        auto offset = toValue<qint32>(commandData.mid(0,4));
        uint8_t ctrl = commandData[4];
        uint8_t size = commandData[5];
        Register* reg = m_registerListModel.getRegisterByCpuIdAndOffset(uCId,offset);
        if (reg != nullptr)
        {
            QVariant newValue;
            switch (reg->variableType())
            {
            case Register::VariableType::Bool:
            {
                newValue.setValue(toValue<bool>(commandData.mid(6,size)));
                break;
            }
            case Register::VariableType::Char:
            {
                newValue.setValue(toValue<uint8_t>(commandData.mid(6,size)));
                break;
            }
            default:      break;
            }

            reg->receivedNewRegisterValue(newValue);
        }
        else
        {
            qWarning() << "Received Query Register from unknown register offset or cpu id: " << offset << uCId;
            //TODO: ADD EXTRA DEBUG INFO. WHAT IS WRONG CPU OR OFFSET.
        }
    }
}

void PresentationLayerV0::receivedDecimation(uint8_t uCId, const QVector<uint8_t> &commandData)
{

}

void PresentationLayerV0::receivedReadChannelData(uint8_t uCId, QVector<uint8_t> &commandData)
{
    if(commandData.size() < 5)
    {
        qWarning() << "Received read channel datacommmand from uC: " << uCId << " is invalid";
    }
    Cpu* cpu = m_cpuListModel.getCpuNodeById(uCId);
    if(cpu != nullptr)
    {
        auto time = static_cast<int>(((commandData[2] << 16) | (commandData[1] << 8) | commandData[0]));
        auto mask = toValue<uint8_t>(commandData.mid(3,2));
        commandData.remove(0,5);

        for (int i = cpu->debugChannels().size(); i >= 0 ; i--)
        {
            if ((mask >> i & 1) == 1)
            {
                Register* reg = cpu->debugChannels().value(i);
                if(reg != nullptr)
                {
                    switch (reg->variableType())
                    {
                    case Register::VariableType::Bool:
                    {
                        reg->receivedNewRegisterValue(toValue<bool>(commandData.mid(0,reg->getVariableTypeSize())),time);
                        break;
                    }
                    case Register::VariableType::Char:
                    {
                        reg->receivedNewRegisterValue(toValue<uint8_t>(commandData.mid(0,reg->getVariableTypeSize())),time);
                        break;
                    }
                    default: break;
                    }
                    commandData.remove(commandData.size() - reg->getVariableTypeSize() ,reg->getVariableTypeSize());
                }
            }
        }
    }
}

void PresentationLayerV0::receivedDebugString(uint8_t uCId, const QVector<uint8_t> &commandData)
{

}

void PresentationLayerV0::receivedGetInfo(uint8_t uCId,QVector<uint8_t>& commandData)
{
    //Check if Cpu exists in list
    qDebug() << "ReceivedGetInfo";
    Cpu* cpu = m_cpuListModel.getCpuNodeById(uCId);

    if(cpu != nullptr)
    {
        // Check if message is too small
        if (commandData.size() < 2)
        {
            qWarning() << "Invalid GetInfo received";
            cpu->increaseInvalidMessageCounter();
        }
        else
        {
            QVector<uint8_t> record;
            for (QVector<uint8_t>::iterator it=commandData.begin(); it != commandData.end(); ++it)
            {
                if (*it == DebugProtocolV0Enums::ProtocolChar::RS || it == commandData.end())
                {
                    if (record[0] == static_cast<uint8_t>(Register::VariableType::TimeStamp))
                    {
                        //Timestamp is 4 byte
                        cpu->setVariableTypeSize(Register::VariableType::TimeStamp,int(record[4] << 24 | record[3] << 16 | record[2] << 8 | record[1]));
                    }
                    else
                    {
                        //Everything else is 1 byte.
                        cpu->setVariableTypeSize(static_cast<Register::VariableType>(record[0]),record[1]);
                    }
                    record.clear();
                }
                else
                {
                    record.append(*it);
                }
            }
            cpu->increaseMessageCounter();;
        }
    }
}


void PresentationLayerV0::sendGetVersion(uint8_t uCId)
{
    QVector<uint8_t> debugProtocolMessage;
    debugProtocolMessage.append(DebugProtocolV0Enums::GetVersion);
    emit newDebugProtocolCommand(uCId, debugProtocolMessage);
}

void PresentationLayerV0::sendGetInfo(uint8_t uCId)
{
    QVector<uint8_t> debugProtocolMessage;
    debugProtocolMessage.append(DebugProtocolV0Enums::GetInfo);
    emit newDebugProtocolCommand(uCId, debugProtocolMessage);

}

void PresentationLayerV0::disableAllConfigChannels(uint8_t uCId, uint8_t nbrOfConfigChannels)
{
    QVector<uint8_t> newDebugProtocolMessage;
    for(int i = 0; i < nbrOfConfigChannels; i++)
    {
        newDebugProtocolMessage.append(DebugProtocolV0Enums::ConfigChannel);
        newDebugProtocolMessage.append(static_cast<uint8_t>(i));
        newDebugProtocolMessage.append(static_cast<uint8_t>(Register::ChannelMode::Off));
        emit newDebugProtocolCommand(uCId,newDebugProtocolMessage);
        newDebugProtocolMessage.clear();
    }
}

uint8_t PresentationLayerV0::controlByte(const Register &Register)
{
    uint8_t control = 0;
    control |= Register.readWrite() == Register::ReadWrite::Write ? 0x80 : 0x00;
    control |= static_cast<uint8_t>(Register.source());
    control |= (Register.derefDepth() & 0x0F);
    return control;
}

