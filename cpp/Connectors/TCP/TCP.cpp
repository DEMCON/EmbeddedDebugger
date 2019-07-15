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

#include "TCP.h"
#include <QDebug>

#include "../DebugProtocolV0/ApplicationLayerV0.h"
#include "../DebugProtocolV0/PresentationLayerV0.h"
#include "../DebugProtocolV0/TransportLayerV0.h"


TCP::TCP(QObject* parent) :
    Medium(parent),
    m_tcpSocket(this)
{
    m_availableProtocols.append("DebugProtocol V0");

    QObject::connect(&m_tcpSocket,&QTcpSocket::connected, this, [&]()
    {
        if(m_presentationLayer != nullptr)
        {
            static_cast<PresentationLayerV0*>(m_presentationLayer)->scanForCpu();
            setConnected(true);
        }
    });
    QObject::connect(&m_tcpSocket,&QTcpSocket::disconnected, this, [&](){setConnected(false);});
    QObject::connect(&m_cpuListModel,&CpuListModel::newRegisterFound,this,[&](Register* newRegister)
    {
       if (m_applicationLayer != nullptr)
       {
           QObject::connect(newRegister,QOverload<Register&>::of(&Register::configDebugChannel),m_applicationLayer,&ApplicationLayerBase::configDebugChannel);
           QObject::connect(newRegister,&Register::writeRegister,m_applicationLayer,&ApplicationLayerBase::writeRegister);
           QObject::connect(newRegister,QOverload<Register&>::of(&Register::queryRegister),m_applicationLayer,&ApplicationLayerBase::queryRegister);
       }
       m_registerListModel.append(newRegister);
    });

    QObject::connect(&m_tcpSocket,QOverload<QAbstractSocket::SocketError>::of(&QAbstractSocket::error), this, [&]()
    {
        emit errorOccured(m_tcpSocket.errorString());
        qDebug() << m_tcpSocket.errorString();
    });
}

TCP::~TCP()
{
    disconnect();
}

void TCP::createDebugProtocolV0Layers()
{
    destroyProtocolLayers();
    m_transportLayer = new TransportLayerV0(this);
    m_presentationLayer = new PresentationLayerV0(m_cpuListModel,m_registerListModel,this);
    m_applicationLayer = new ApplicationLayerV0(static_cast<PresentationLayerV0&>(*m_presentationLayer),this);
}

void TCP::connectLayers()
{
    QObject::connect(m_transportLayer,&TransportLayerBase::receivedDebugProtocolCommand,
                     m_presentationLayer,&PresentationLayerBase::receivedDebugProtocolCommand);
    QObject::connect(&m_tcpSocket,&QTcpSocket::readyRead, this, [&]()
    {
        m_transportLayer->receivedData(m_tcpSocket.readAll());
    });
    QObject::connect(m_transportLayer,&TransportLayerBase::write, this, [&](const QByteArray& message)
    {
        m_tcpSocket.write(message);
    });
    QObject::connect(m_presentationLayer,&PresentationLayerBase::newDebugProtocolCommand,
                     m_transportLayer,&TransportLayerBase::sendDebugProtocolCommand);
    QObject::connect(m_presentationLayer,&PresentationLayerBase::newCpuFound,this, [&](Cpu* newCpu)
    {
        if (!m_cpuListModel.contains(newCpu->id()))
        {
            QObject::connect(newCpu,&Cpu::resetTime,m_applicationLayer,&ApplicationLayerBase::resetTime);
            QObject::connect(newCpu,QOverload<Cpu&>::of(&Cpu::setDecimation),m_applicationLayer,&ApplicationLayerBase::setDecimation);
            m_cpuListModel.append(newCpu);
        }
    });
}

void TCP::destroyProtocolLayers()
{
    if (m_applicationLayer != nullptr)
    {
        m_applicationLayer->deleteLater();
        m_applicationLayer = nullptr;
    }
    if (m_presentationLayer != nullptr)
    {
        m_presentationLayer->deleteLater();
        m_presentationLayer = nullptr;
    }
    if (m_transportLayer != nullptr)
    {
        QObject::disconnect(&m_tcpSocket, &QTcpSocket::readyRead, this, nullptr); //Disconnect tcpSocket lambda
        m_transportLayer->deleteLater();
        m_transportLayer = nullptr;
    }
}


void TCP::connect()
{

    m_settings.beginGroup("TCP");

    QString hostname = m_settings.value("IPAddress","").toString();
    bool portConverted;
    uint16_t port = static_cast<uint16_t>(m_settings.value("IPPort",0).toInt(&portConverted));

    m_settings.endGroup();
    if (hostname.isEmpty() ||
        !portConverted ||
        port == 0)
    {
        m_tcpSettingsDialog.show();
    }
    else
    {
        switch(m_selectedProtocolVersion)
        {
            case 0:  createDebugProtocolV0Layers(); break;
        }

        connectLayers();
        m_tcpSocket.connectToHost(hostname,port);
    }
}

void TCP::disconnect()
{
    m_tcpSocket.disconnectFromHost();
    m_tcpSocket.reset();
    m_cpuListModel.clear();
    m_registerListModel.clear();
    destroyProtocolLayers();
}

void TCP::showSettings()
{
    m_tcpSettingsDialog.show();
}

bool TCP::setHostAddress(const QString &ipAddress, int ipPort)
{
    m_hostPort = ipPort;
    return m_hostAddress.setAddress(ipAddress);
}

void TCP::setProtocolVersion(int availableProtocolVersionIndex)
{
    if(availableProtocolVersionIndex < m_availableProtocols.size())
    {
        m_selectedProtocolVersion = availableProtocolVersionIndex;
    }
}
