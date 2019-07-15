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

#include "ui/MainWindow.h"
#include <QApplication>
#include <QDir>
#include <QPluginLoader>
#include <QDebug>
#include "Medium/Medium.h"
#include "Core.h"
#include <QSettings>

Q_DECLARE_METATYPE(QModelIndex)

int main(int argc, char *argv[])
{
    QApplication a(argc, argv);

    QCoreApplication::setOrganizationName("DEMCON");
    QCoreApplication::setOrganizationDomain("www.demcon.nl");
    QCoreApplication::setApplicationName("Embedded Debugger");

    QSettings settings;

    Core::Initialize();

    return a.exec();
}
