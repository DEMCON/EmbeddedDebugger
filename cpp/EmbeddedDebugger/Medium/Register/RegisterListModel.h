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

#ifndef REGISTERLISTMODEL_H
#define REGISTERLISTMODEL_H

class Register;
#include <QAbstractTableModel>
#include <QVector>

class RegisterListModel : public QAbstractTableModel
{
    Q_OBJECT
public:
    explicit RegisterListModel(QObject* parent = nullptr);
    virtual ~RegisterListModel();

    //Basic funtionality:
    int rowCount(const QModelIndex &parent) const override;
    int columnCount(const QModelIndex &parent) const override;
    Qt::ItemFlags flags(const QModelIndex &index) const override;
    QVariant data(const QModelIndex &index, int role = Qt::DisplayRole) const override;
    bool setData(const QModelIndex &index, const QVariant &value, int role = Qt::EditRole) override;
    QVariant headerData(int section, Qt::Orientation orientation, int role) const override;
    QVector<Register*>::iterator begin() {return m_registers.begin();}
    QVector<Register*>::iterator end() {return m_registers.end();}


    void insert(int index, Register* registerNode);
    void append(Register *registerNode);
    void clear();
    bool contains(uint registerId);
    Register* getRegisterById(uint registerID);
    Register* getRegisterByOffset(uint32_t offset);
    Register* getRegisterByCpuIdAndOffset(uint8_t uCId, int32_t offset);

private slots:
    void registerDataChanged(Register& Register);

private:
    QVector<Register*> m_registers;
};

#endif // REGISTERLISTMODEL_H
