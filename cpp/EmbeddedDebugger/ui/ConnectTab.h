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

#ifndef CONNECTTAB_H
#define CONNECTTAB_H

#include <QWidget>

namespace Ui {
class ConnectTab;
}

class ConnectTab : public QWidget
{
    Q_OBJECT

public:
    explicit ConnectTab(QWidget *parent = nullptr);
    ~ConnectTab();

    void init();

private slots:
    void on_connectButton_clicked();

    void on_profileCombobox_currentIndexChanged(int index);
    void customHeaderMenuRequested();
    void on_disconnectButton_clicked();

    void on_settingsButton_clicked();

private:
    Ui::ConnectTab *ui;
};

#endif // CONNECTTAB_H
