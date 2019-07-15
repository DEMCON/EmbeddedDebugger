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

#include "PushButtonDelegate.h"
#include <QPushButton>

#include <QApplication>
#include <QWidget>
#include <QModelIndex>
#include <QApplication>
#include <QDebug>

PushButtonDelegate::PushButtonDelegate(const QString &buttonText, QObject *parent) :
    QStyledItemDelegate(parent),
    m_buttonText(buttonText)
{

}

QWidget *PushButtonDelegate::createEditor(QWidget *parent, const QStyleOptionViewItem &option, const QModelIndex &index) const
{
    QPushButton* editor = new QPushButton(m_buttonText, parent);
    if (editor != nullptr)
    {
        connect(editor,  &QPushButton::clicked, [=]()
        {
            const_cast<PushButtonDelegate*>(this)->closeEditor(editor);
        });
    }
    return editor;
}

void PushButtonDelegate::setEditorData(QWidget *editor, const QModelIndex &index) const
{
    QPushButton* button = qobject_cast<QPushButton*>(editor);
    Q_ASSERT(button);
}

void PushButtonDelegate::setModelData(QWidget *editor, QAbstractItemModel *model, const QModelIndex &index) const
{
    QPushButton* button = qobject_cast<QPushButton*>(editor);
    Q_ASSERT(button);
    model->setData(index,true);
}

void PushButtonDelegate::paint(QPainter *painter, const QStyleOptionViewItem &option, const QModelIndex &index) const
{
    QStyleOptionButton btn;
    btn.rect = option.rect;
    btn.text = m_buttonText;
    btn.state |= QStyle::State_Enabled;
    QApplication::style()->drawControl(QStyle::CE_PushButton,&btn,painter);
}
