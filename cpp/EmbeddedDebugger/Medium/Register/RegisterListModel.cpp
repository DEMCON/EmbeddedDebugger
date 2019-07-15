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

#include "RegisterListModel.h"
#include "Medium/Register/Register.h"
#include "Medium/CPU/Cpu.h"
#include <QDebug>

RegisterListModel::RegisterListModel(QObject* parent)
{

}

RegisterListModel::~RegisterListModel()
{
    clear();
}

int RegisterListModel::rowCount(const QModelIndex &parent) const
{
    Q_UNUSED(parent);
    return m_registers.count();
}

int RegisterListModel::columnCount(const QModelIndex &parent) const
{
    Q_UNUSED(parent);
    return 6;
}

Qt::ItemFlags RegisterListModel::flags(const QModelIndex &index) const
{
    if (!index.isValid())
        return Qt::ItemIsEnabled;
    if (index.column() == 3)
    {
        return Qt::ItemIsEnabled | Qt::ItemIsEditable;
    }
    else
    {
        return Qt::ItemIsEnabled;
    }


}

QVariant RegisterListModel::data(const QModelIndex &index, int role) const
{
    QVariant returnValue;

    if (index.isValid() &&
            index.row() < m_registers.size() &&
            index.row() >= 0 &&
            (role == Qt::DisplayRole ||
             role == Qt::EditRole))
    {
        const auto &Register = m_registers.at(index.row());

        switch(index.column())
        {
        case 0:
        {
            returnValue = Register->cpu().id();
            break;
        }
        case 1:
        {
            returnValue =  Register->name();
            break;
        }
        case 2:
        {
            returnValue =  Register::variableTypeToString(Register->variableType());
            break;
        }
        case 3:
        {
            if(!Register->value().isNull())
            {
                returnValue =  Register->value();
            }
            break;
        }
        case 4:
        {
            returnValue = static_cast<uint8_t>(Register->channelMode());
        }
        default: break;
        }
    }
    return returnValue;
}

bool RegisterListModel::setData(const QModelIndex &index, const QVariant &value, int role)
{
    if (index.isValid() &&
            index.row() < m_registers.size() &&
            index.row() >= 0 &&
            (role == Qt::DisplayRole ||
             role == Qt::EditRole))
    {
        const auto &Register = m_registers.at(index.row());

        switch(index.column())
        {
        case 3:
        {
            Register->setValue(value);
            break;
        }
        case 4:
        {
            //Channel Mode
            Register->configDebugChannel(static_cast<Register::ChannelMode>(value.toInt()));
            break;
        }
        case 5:
        {
            //Refesh Value
            Register->queryRegister();
            break;
        }
        default:
        {
            qDebug() << "default";
            break;
        }

        }
    }

    return true;
}

QVariant RegisterListModel::headerData(int section, Qt::Orientation orientation, int role) const
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
            returnValue = tr("Type");
            break;
        }
        case 3:
        {
            returnValue = tr("Value");
            break;
        }
        case 4:
        {
            returnValue = tr("Channel mode");
            break;
        }
        case 5:
        {
            returnValue = tr("Refresh");
            break;
        }
        default: break;
        }
    }
    return returnValue;
}

void RegisterListModel::insert(int index, Register* registerNode)
{
    if(index < 0)
    {
        registerNode->deleteLater();
        return;
    }
    beginInsertRows(QModelIndex(), index, index);
    registerNode->setParent(this);
    connect(registerNode,&Register::registerDataChanged,this,&RegisterListModel::registerDataChanged);
    m_registers.insert(index,registerNode);
    endInsertRows();
}


void RegisterListModel::append(Register* registerNode)
{
    insert(m_registers.count(),registerNode);
}

void RegisterListModel::clear()
{
    beginResetModel();
    for (auto registerNode : qAsConst(m_registers))
    {
        registerNode->deleteLater();
    }
    m_registers.clear();
    endResetModel();
}

bool RegisterListModel::contains(uint registerId)
{
    bool returnValue = false;
    for (auto registerNode : qAsConst(m_registers))
    {
        if (registerNode->id() == registerId)
        {
            returnValue = true;
        }
    }
    return returnValue;
}

Register* RegisterListModel::getRegisterById(uint registerID)
{
    Register* returnValue = nullptr;

    for (auto tempRegister : qAsConst(m_registers))
    {
        if (tempRegister->id() == registerID)
        {
            returnValue = tempRegister;
        }
    }
    return returnValue;
}

Register *RegisterListModel::getRegisterByOffset(uint32_t offset)
{
    Register* returnValue = nullptr;

    for (auto tempRegister : qAsConst(m_registers))
    {
        if (tempRegister->offset() == offset)
        {
            returnValue = tempRegister;
        }
    }
    return returnValue;
}

Register *RegisterListModel::getRegisterByCpuIdAndOffset(uint8_t uCId, int32_t offset)
{
    Register* returnValue = nullptr;

    for (auto tempRegister : qAsConst(m_registers))
    {
        if (tempRegister->cpu().id() == uCId &&
            tempRegister->offset() == offset)
        {
            returnValue = tempRegister;
        }
    }
    return returnValue;
}

void RegisterListModel::registerDataChanged(Register &Register)
{
    int row = m_registers.indexOf(&Register);
    QModelIndex startOfRow = this->index(row, 0);
    QModelIndex endOfRow   = this->index(row, columnCount(QModelIndex()));


    emit dataChanged(startOfRow,endOfRow, {Qt::DisplayRole}); //TODO: CHECK THIS HACK
}
