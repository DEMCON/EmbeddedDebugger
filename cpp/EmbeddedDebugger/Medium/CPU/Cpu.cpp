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

#include "Cpu.h"
#include <QDir>
#include <QJsonDocument>
#include <QJsonObject>
#include <QJsonArray>
#include <QDebug>

Cpu::Cpu(uint8_t id,const QString& name,const QString& serialNumber,const QString& protocolVersion,
         const QString& applicationVersion, QObject *parent) :
    QObject(parent),
    m_id(id),
    m_name(name),
    m_serialNumber(serialNumber),
    m_protocolVersion(protocolVersion),
    m_applicationVersion(applicationVersion)
{
    qDebug() << "New cpu: " << m_id;
}

Cpu::~Cpu()
{

}

void Cpu::setVariableTypeSize(const Register::VariableType &variableType, int size)
{
    m_variableTypeSizes.append(qMakePair(variableType,size));
}

int Cpu::getVariableTypeSize(const Register::VariableType& variableType)
{
    int returnValue = 0;
    for(auto variable : qAsConst(m_variableTypeSizes))
    {
        if(variable.first == variableType)
        {
            returnValue = variable.second;
        }
    }
    return returnValue;
}

void Cpu::increaseMessageCounter()
{
    m_messageCounter++;
}

void Cpu::increaseInvalidMessageCounter()
{
    increaseMessageCounter();
    m_invalidMessageCounter++;
}

int Cpu::nextDebugChannel()
{
    if(m_debugChannels.size() < m_maxDebugChannels)
    {
        return m_debugChannels.size();
    }
    return -1;
}


void Cpu::setDecimation(int newDecimation)
{
    m_decimation = newDecimation;
    emit setDecimation(*this);
}

bool Cpu::loadConfiguration()
{
    QString fileLocation(QDir::currentPath() + "/Registers/" + m_name + "/" + m_applicationVersion+ ".json");
    QFile loadFile(fileLocation);
    if (!loadFile.open(QIODevice::ReadOnly))
    {
        qWarning() << "Could not open register List at location: " << fileLocation.toStdString().c_str();
        return false;
    }

    QByteArray registerData = loadFile.readAll();

    QJsonDocument loadDoc(QJsonDocument::fromJson(registerData));

    QJsonObject registerObject = loadDoc.object();
    QJsonArray registerAray = registerObject["Registers"].toArray();
    for (auto RegisterRef : registerAray)
    {
        QJsonObject Reg = RegisterRef.toObject();
        Register* newRegister = new Register(Reg["id"].toInt(),
                Reg["name"].toString(),
                Register::ReadWritefromString(Reg["ReadWrite"].toString()),
                Register::variableTypeFromString(Reg["Type"].toString()),
                Register::SourcefromString(Reg["Source"].toString()),
                Reg["DerefDepth"].toInt(),
                Reg["Offset"].toInt(),
                *this);

        emit newRegisterFound(newRegister);
    }
    return true;
}

void Cpu::receivedDecimation(int decimation)
{
    m_decimation = decimation;
}


