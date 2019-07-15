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

#ifndef CORE_H
#define CORE_H

#include <QObject>
#include <QThread>
#include "ui/MainWindow.h"
#include "ProfileManager/ProfileManager.h"

/**
 * @class Core
 * @brief Singleton that provides access to the different parts of Embedded Debugger
 *
 * Needs to be initialize before any widgets are created in MainWindow.
 */

class Core : public QObject
{
    Q_OBJECT
public:

    /** @brief Initializes Core object. */
    static void Initialize();

    /** @brief Returns a reference to the singleton object. */
    static Core& Instance();

    ~Core() override;

    /** delete copy and move operators */
    Core(Core const&) = delete;
    Core(Core&&) = delete;
    Core& operator=(Core const&) = delete;
    Core& operator=(Core &&) = delete;


    /** @brief Returns a reference to the main window. */
    MainWindow &window() {return m_mainWindow;}

    /** @brief Returns a reference to the profile manager. */
    ProfileManager &profileManager() {return m_profileManager;}

private:
    explicit Core();

    MainWindow m_mainWindow;
    QThread m_profileThread;
    ProfileManager m_profileManager;
};

#endif // CORE_H
