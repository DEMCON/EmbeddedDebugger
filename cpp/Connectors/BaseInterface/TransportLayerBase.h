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

#ifndef TRANSPORTLAYERBASE_H
#define TRANSPORTLAYERBASE_H

#include <QObject>

class TransportLayerBase : public QObject
{
    Q_OBJECT
public:
    explicit TransportLayerBase(QObject* parent = nullptr) :
        QObject(parent){}

signals:
    void receivedDebugProtocolCommand(uint8_t uCId, QVector<uint8_t> messageVector);
    void write(const QByteArray& message);

public slots:
    virtual void sendDebugProtocolCommand(uint8_t uCId, QVector<uint8_t> messageVector) = 0;
    virtual void receivedData(QByteArray message) = 0;

};


#endif // TRANSPORTLAYERBASE_H
