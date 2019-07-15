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

#ifndef MEDIUM_H
#define MEDIUM_H

#include <QObject>
#include "CPU/CpuListModel.h"
#include "../Profiles/kconcatenaterowsproxymodel.h"
#include "Register/RegisterListModel.h"

class Medium : public QObject
{
    Q_OBJECT
public:
    explicit Medium(QObject *parent = nullptr) :
        QObject(parent){}

    virtual void connect() = 0;
    virtual void disconnect() = 0;
    virtual void showSettings() = 0;
    CpuListModel& cpuListModel() {return m_cpuListModel;}
    RegisterListModel& registerListModel() {return m_registerListModel;}

    bool isConnected() const {return m_connected;}
    void setConnected(bool isConnected)
    {
        if(m_connected != isConnected)
        {
            m_connected = isConnected;
            emit connectedChanged();
        }
    }

signals:
    void errorOccured(QString error);
    void connectedChanged();
    void registerListModelChanged();

protected:
    CpuListModel m_cpuListModel;
    RegisterListModel m_registerListModel;
    bool m_connected = false;
};

#endif // MEDIUM_H
