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

#include "ConnectTab.h"
#include "ui_ConnectTab.h"
#include "Core.h"
#include "ProfileManager/ProfileManager.h"
#include <QDebug>
#include <QHeaderView>
#include <QMenu>

ConnectTab::ConnectTab(QWidget *parent) :
    QWidget(parent),
    ui(new Ui::ConnectTab)
{
    ui->setupUi(this);
    QHeaderView* tableViewHeader = ui->cpuTableView->horizontalHeader();
    tableViewHeader->setSectionsMovable(true);
    tableViewHeader->setSectionResizeMode(QHeaderView::Interactive);
    tableViewHeader->setContextMenuPolicy(Qt::CustomContextMenu);
    connect(tableViewHeader, &QWidget::customContextMenuRequested, this, &ConnectTab::customHeaderMenuRequested);
}


ConnectTab::~ConnectTab()
{
    qDebug() << "Delete ConnectTab";
    delete ui;
    qDebug() << "Delete ConnectTab done";
}

void ConnectTab::init()
{
    ui->profileCombobox->setModel(&Core::Instance().profileManager().profileList());
    ui->cpuTableView->setModel(Core::Instance().profileManager().cpuListModel());
}

void ConnectTab::on_connectButton_clicked()
{
    if (Core::Instance().profileManager().getActiveProfile() != nullptr)
    {
        Core::Instance().profileManager().getActiveProfile()->connect();
    }
}

void ConnectTab::on_profileCombobox_currentIndexChanged(int index)
{
    Core::Instance().profileManager().setActiveConnector(index);
}

void ConnectTab::customHeaderMenuRequested()
{
    QMenu * menu = new QMenu(this);
    for(int i = 0; i < ui->cpuTableView->horizontalHeader()->count(); i++)
    {
        QAction *actNone = new QAction(ui->cpuTableView->model()->headerData(i, Qt::Horizontal).toString(), this);
        actNone->setCheckable(true);
        actNone->setChecked(!ui->cpuTableView->horizontalHeader()->isSectionHidden(i));
        connect(actNone, &QAction::toggled,this, [i,this](bool checked) {
            ui->cpuTableView->horizontalHeader()->setSectionHidden(i, !checked);
            });
        menu->addAction(actNone);
    }
    menu->popup(QCursor::pos());
}

void ConnectTab::on_disconnectButton_clicked()
{
    if (Core::Instance().profileManager().getActiveProfile() != nullptr)
    {
        Core::Instance().profileManager().getActiveProfile()->disconnect();
    }
}

void ConnectTab::on_settingsButton_clicked()
{
    if (Core::Instance().profileManager().getActiveProfile() != nullptr)
    {
        Core::Instance().profileManager().getActiveProfile()->showSettings();
    }
}
