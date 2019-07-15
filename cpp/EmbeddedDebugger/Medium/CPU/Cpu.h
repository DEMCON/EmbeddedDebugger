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

#ifndef CPUNODE_H
#define CPUNODE_H

#include <QObject>
#include <QVector>
#include "Medium/Register/RegisterListModel.h"
#include "Medium/Register/Register.h"

class Cpu : public QObject
{
    Q_OBJECT

public:
    explicit Cpu(uint8_t id,const QString& name,const QString& serialNumber,const QString& protocolVersion,
                    const QString& applicationVersion, QObject* parent = nullptr);
    virtual ~Cpu();

    //Getters
    uint8_t id() const {return m_id;}    
    QString name() const {return m_name;}
    QString serialNumber() const {return m_serialNumber;}
    QString protocolVersion() const {return m_protocolVersion;}
    QString applicationVersion() const {return m_applicationVersion;}
    int decimation() const {return m_decimation;}
    int messageCounter() const {return m_messageCounter;}
    int invalidMessageCounter() const {return m_invalidMessageCounter;}

    void setVariableTypeSize(const Register::VariableType &variableType, int size);
    int getVariableTypeSize(const Register::VariableType& variableType);
    void increaseMessageCounter();
    void increaseInvalidMessageCounter();
    void increaseNbrOfActiveDebugChannels() {m_activeDebugChannels++;}
    void decreaseNbrOfActiveDebugChannels() {m_activeDebugChannels--;}
    int maxDebugChannels() const {return m_maxDebugChannels;}
    int  nextDebugChannel();
    QVector<Register*>& debugChannels() {return m_debugChannels;}

signals:
    void resetTime(Cpu& cpu);
    void getDecimation(Cpu& cpu);
    void setDecimation(Cpu& cpu);
    void decimationChanged();
    void newRegisterFound(Register* newRegister);

public slots:

    void setDecimation(int newDecimation);
    bool loadConfiguration();
    void receivedDecimation(int decimation);

private:
    uint8_t m_id = 0;
    QString m_name;
    QString m_serialNumber;
    QString m_protocolVersion;
    QString m_applicationVersion;
    int m_activeDebugChannels = 0;
    int m_maxDebugChannels = 16;
    int m_decimation = 0;
    int m_messageCounter= 0;
    int m_invalidMessageCounter = 0;
    QVector<Register*> m_debugChannels;
    QVector<QPair<Register::VariableType,int>> m_variableTypeSizes;

};

#endif // CPUNODE_H
