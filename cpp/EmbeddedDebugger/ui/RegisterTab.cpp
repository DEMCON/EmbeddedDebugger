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

#include "RegisterTab.h"
#include "ui_RegisterTab.h"
#include "Core.h"
#include "ProfileManager/ProfileManager.h"
#include "ProfileManager/ProfileListModel.h"
#include <QDebug>


RegisterTab::RegisterTab(QWidget *parent) :
    QWidget(parent),
    ui(new Ui::RegisterTab),
    m_refreshButtonDelegate(tr("Refresh"))
{
    QStringList channelModes;
    channelModes.append("Off");
    channelModes.append("High speed");
    channelModes.append("Low speed");
    channelModes.append("Once");
    m_channelModeDelegate.addItems(channelModes);
    ui->setupUi(this);
}

RegisterTab::~RegisterTab()
{
    qDebug() << "Delete RegisterTab";
    delete ui;
    qDebug() << "Delete RegisterTab done";
}

void RegisterTab::init()
{
    ui->registerTableView->setModel(Core::Instance().profileManager().registerListModel());
    ui->registerTableView->setItemDelegateForColumn(4,&m_channelModeDelegate);
    ui->registerTableView->setItemDelegateForColumn(5,&m_refreshButtonDelegate);
      connect(ui->registerTableView->model(), &QAbstractItemModel::rowsInserted, this, [&](){
          //qDebug() << ui->registerTableView->indexAt(ui->registerTableView->viewport()->rect().topLeft());
          //qDebug() << ui->registerTableView->indexAt(ui->registerTableView->viewport()->rect().bottomLeft());
          for (int i=0; i< ui->registerTableView->model()->rowCount(); i++) {
              ui->registerTableView->openPersistentEditor(ui->registerTableView->model()->index(i, 4));
              ui->registerTableView->openPersistentEditor(ui->registerTableView->model()->index(i, 5));
          }

      });
}
