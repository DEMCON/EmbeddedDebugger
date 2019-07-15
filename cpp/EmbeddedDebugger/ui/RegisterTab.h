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

#ifndef REGISTERTAB_H
#define REGISTERTAB_H

#include <QWidget>
#include "ComboBoxDelegate.h"
#include "PushButtonDelegate.h"

namespace Ui {
class RegisterTab;
}

class RegisterTab : public QWidget
{
    Q_OBJECT

public:
    explicit RegisterTab(QWidget *parent = nullptr);
    ~RegisterTab();

    void init();

private:
    Ui::RegisterTab *ui;
    ComboBoxDelegate m_channelModeDelegate;
    PushButtonDelegate m_refreshButtonDelegate;
};

#endif // REGISTERTAB_H
