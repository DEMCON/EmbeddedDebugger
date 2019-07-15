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

#ifndef TCP_H
#define TCP_H

#include <QTcpSocket>
#include <QHostAddress>
#include "../../EmbeddedDebugger/Medium/Medium.h"
#include <QStringList>
#include "Settings.h"

class ApplicationLayerBase;
class PresentationLayerBase;
class TransportLayerBase;

class TCP : public Medium
{
    Q_OBJECT
public:
    explicit TCP(QObject* parent = nullptr);
    virtual ~TCP();

    QString hostAddress() const {return m_hostAddress.toString();}
    int hostPort() const {return m_hostPort;}

    QStringList availableProtocolVersions() {return m_availableProtocols;}

public slots:
    void connect() override;
    void disconnect() override;
    void showSettings() override;
    bool setHostAddress(const QString& ipAddress, int ipPort);
    void setProtocolVersion(int availableProtocolVersionIndex);

private:
    void createDebugProtocolV0Layers();
    void connectLayers();
    void destroyProtocolLayers();

private:
    ApplicationLayerBase* m_applicationLayer = nullptr;
    PresentationLayerBase* m_presentationLayer = nullptr;
    TransportLayerBase* m_transportLayer = nullptr;
    QTcpSocket m_tcpSocket;
    QStringList m_availableProtocols;
    QHostAddress m_hostAddress;
    Settings m_tcpSettingsDialog;
    QSettings m_settings;
    int m_hostPort = 0;
    int m_selectedProtocolVersion = 0;
};

#endif // TCP_H
