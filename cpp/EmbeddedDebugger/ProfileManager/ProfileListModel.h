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

#ifndef PROFILELISTMODEL_H
#define PROFILELISTMODEL_H

#include <QAbstractListModel>
#include <QList>
#include <QPluginLoader>

class ProfileListModel : public QAbstractListModel
{
    Q_OBJECT
public:
    explicit ProfileListModel(QObject *parent = nullptr);
    virtual ~ProfileListModel();

    //Basic funtionality:
    void clear();
    int rowCount(const QModelIndex &parent = QModelIndex()) const override;
    QVariant data(const QModelIndex &index, int role = Qt::DisplayRole) const override;
    QHash<int, QByteArray> roleNames() const override;

    void insert(int index, QPluginLoader* connector);
    void append(QPluginLoader* connector);
    QPluginLoader* at(int index);
    QPluginLoader* last();

private:
   QList<QPluginLoader*> m_profileList;
};

#endif // PROFILELISTMODEL_H
