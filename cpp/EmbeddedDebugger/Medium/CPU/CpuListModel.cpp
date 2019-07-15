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

#include "CpuListModel.h"
#include <QSharedPointer>
#include <QDebug>

CpuListModel::CpuListModel(QObject *parent) : QAbstractTableModel(parent)
{

}

CpuListModel::~CpuListModel()
{
    clear();
}

int CpuListModel::rowCount(const QModelIndex &parent) const
{
    Q_UNUSED(parent);
    return m_cpuNodes.size();
}

int CpuListModel::columnCount(const QModelIndex &parent) const
{
    Q_UNUSED(parent);
    return 6;
}

QVariant CpuListModel::data(const QModelIndex &index, int role) const
{
    QVariant returnValue;

    if (index.isValid() &&
        index.row() < m_cpuNodes.size() &&
        index.row() >= 0 &&
        role == Qt::DisplayRole)
    {
        const auto &cpu = m_cpuNodes.at(index.row());

        switch(index.column())
        {
        case 0:
            returnValue = cpu->id(); break;
        case 1:
            returnValue = cpu->name(); break;
        case 2:
            returnValue = cpu->serialNumber(); break;
        case 3:
            returnValue = cpu->protocolVersion(); break;
        case 4:
            returnValue = cpu->applicationVersion(); break;
        case 5:
        {
            QString messageCount;
            messageCount = QString::number(cpu->invalidMessageCounter()) + "/" +
                    QString::number(cpu->messageCounter());

            returnValue = messageCount; break;
        }
        default:  break;
        }

    }

    return returnValue;
}

QVariant CpuListModel::headerData(int section, Qt::Orientation orientation, int role) const
{
    QVariant returnValue;
    if (role == Qt::DisplayRole &&
        orientation == Qt::Horizontal)
    {
        switch (section) {
        case 0:
        {
            returnValue = tr("Cpu ID");
            break;
        }
        case 1:
        {
            returnValue = tr("Name");
            break;
        }
        case 2:
        {
            returnValue = tr("Serial Number");
            break;
        }
        case 3:
        {
            returnValue = tr("Protocol Version");
            break;
        }
        case 4:
        {
            returnValue = tr("Application Version");
            break;
        }
        case 5:
        {
            returnValue = tr("Invalid message count");
            break;
        }
        default:
            break;
        }
    }

    return returnValue;
}

void CpuListModel::insert(int index, Cpu* cpuNode)
{
    if(index < 0 || contains(cpuNode->id()))
    {
        cpuNode->deleteLater();
        return;
    }
    beginInsertRows(QModelIndex(), index, index);
    cpuNode->setParent(this); //Set the parent of the object cpuNode to this listModel
    m_cpuNodes.insert(index,cpuNode);
    connect(cpuNode,&Cpu::newRegisterFound,this,&CpuListModel::newRegisterFound);
    cpuNode->loadConfiguration();
    endInsertRows();
}

void CpuListModel::append(Cpu* cpuNode)
{
    insert(m_cpuNodes.count(),cpuNode);
}

void CpuListModel::clear()
{
    beginResetModel();
    for(auto cpuNode : qAsConst(m_cpuNodes))
    {
        cpuNode->deleteLater();
    }
    m_cpuNodes.clear();
    endResetModel();
}

bool CpuListModel::contains(uint8_t nodeId)
{
    bool returnValue = false;
    for (auto cpuNode : qAsConst(m_cpuNodes))
    {
        if(cpuNode->id() == nodeId)
        {
            returnValue = true;
        }
    }
    return returnValue;
}

Cpu* CpuListModel::getCpuNodeById(uint8_t cpuNodeID)
{
    Cpu* cpuNode = nullptr;

    for (auto node : qAsConst(m_cpuNodes))
    {
        if (node->id() == cpuNodeID)
        {
            cpuNode = node;
        }
    }
    return cpuNode;
}
