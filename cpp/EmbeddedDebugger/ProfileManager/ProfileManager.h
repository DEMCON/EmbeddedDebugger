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

#ifndef PROFILEMANAGER_H
#define PROFILEMANAGER_H

#include <QObject>
#include "ProfileListModel.h"
#include "../Profiles/BaseProfile.h"

class ProfileManager : public QObject
{
    Q_OBJECT
public:
    explicit ProfileManager(QObject *parent = nullptr);

    void searchProfiles();

    BaseProfile* getActiveProfile() {return m_activeProfile;}
    void setActiveConnector(int indexOfConnectorListModel);
    ProfileListModel& profileList() {return m_profileListModel;}
    KConcatenateRowsProxyModel* cpuListModel();
    KConcatenateRowsProxyModel* registerListModel();

private:
    BaseProfile* m_activeProfile = nullptr;
    QPluginLoader* m_activePluginLoader = nullptr;
    ProfileListModel m_profileListModel;
};

#endif // PROFILEMANAGER_H
