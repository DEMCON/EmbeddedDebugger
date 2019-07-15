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

#ifndef TRANSPORTLAYERV0_H
#define TRANSPORTLAYERV0_H

#include "../BaseInterface/TransportLayerBase.h"
#include "../BaseInterface/Common.h"

class TransportLayerV0 : public TransportLayerBase
{
    Q_OBJECT
public:
    explicit TransportLayerV0(QObject *parent = nullptr);
    virtual ~TransportLayerV0();



public slots:
    void sendDebugProtocolCommand(uint8_t uCId, QVector<uint8_t> messageVector) override;
    void receivedData(QByteArray message) override;

private:
    uint8_t msgId();
    uint8_t calculateCRC(const QVector<uint8_t>& messageVector);
    void addEscapeCharacters(QVector<uint8_t>& messageVector);
    void replaceEscapeCharacters(QVector<uint8_t>& messageVector);

private:
    uint8_t m_msgId = 0;
    QByteArray m_dataBuffer;
};

#endif // TRANSPORTLAYERV0_H
