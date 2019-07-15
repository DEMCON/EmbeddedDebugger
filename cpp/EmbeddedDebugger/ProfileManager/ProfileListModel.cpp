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

#include "ProfileListModel.h"
#include <QDebug>

ProfileListModel::ProfileListModel(QObject *parent) :
    QAbstractListModel(parent)
{
}

ProfileListModel::~ProfileListModel()
{
    clear();
}

void ProfileListModel::clear()
{
    for(auto Plugin : qAsConst(m_profileList))
    {
        Plugin->unload();
        Plugin->deleteLater();
    }
    m_profileList.clear();
}

int ProfileListModel::rowCount(const QModelIndex &parent) const
{
    Q_UNUSED(parent);
    return m_profileList.size();
}

QVariant ProfileListModel::data(const QModelIndex &index, int role) const
{
    QVariant returnValue;

       int row = index.row();
       if(row > 0 || row <= m_profileList.count())
       {
           QJsonObject metaData = m_profileList.at(row)->metaData().value("MetaData").toObject();
           switch (role)
           {
           case Qt::DisplayRole:
           {
               returnValue = metaData.value("profile").toString();
               break;
           }
           }
       }
       return returnValue;
}

QHash<int, QByteArray> ProfileListModel::roleNames() const
{

}

void ProfileListModel::insert(int index, QPluginLoader *connector)
{
    if(index < 0) {
        return;
    }
    emit beginInsertRows(QModelIndex(), index, index);
    m_profileList.insert(index,connector);
    emit endInsertRows();
}

void ProfileListModel::append(QPluginLoader *connector)
{
    insert(m_profileList.count(),connector);
}

QPluginLoader *ProfileListModel::at(int index)
{
    QPluginLoader* returnValue = nullptr;
    if (index >= 0 && index < m_profileList.size())
    {
        returnValue = m_profileList.at(index);
    }
    return returnValue;
}

QPluginLoader *ProfileListModel::last()
{
    QPluginLoader* returnValue = nullptr;
    if (!m_profileList.isEmpty())
    {
        returnValue = m_profileList.last();
    }
    return returnValue;
}
