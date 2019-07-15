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

#include "ProfileManager.h"
#include <QCoreApplication>
#include <QDir>
#include <QPluginLoader>

ProfileManager::ProfileManager(QObject *parent) : QObject(parent)
{

}

void ProfileManager::searchProfiles()
{
    QDir pluginsDir(qApp->applicationDirPath());
#if defined(Q_OS_WIN)
    if (pluginsDir.dirName().toLower() == "debug" || pluginsDir.dirName().toLower() == "release")
        pluginsDir.cdUp();
    pluginsDir.cdUp();
#elif defined(Q_OS_MAC)
    if (pluginsDir.dirName() == "MacOS") {
        pluginsDir.cdUp();
        pluginsDir.cdUp();
        pluginsDir.cdUp();
    }
#elif defined(Q_OS_LINUX)
    pluginsDir.cdUp();
#endif
    pluginsDir.cd("plugins");
    foreach (QString fileName, pluginsDir.entryList(QDir::Files)) {
        QPluginLoader* newPlugin = new QPluginLoader(pluginsDir.absoluteFilePath(fileName));
        auto tempInterface = qobject_cast<BaseProfile*>(newPlugin->instance());
        if (tempInterface != nullptr)
        {
            newPlugin->unload();
            m_profileListModel.append(newPlugin);
        }
        else {
            newPlugin->deleteLater();
        }
    }
}

void ProfileManager::setActiveConnector(int indexOfConnectorListModel)
{
    if (indexOfConnectorListModel >= 0 && indexOfConnectorListModel < m_profileListModel.rowCount())
    {
        if (m_activePluginLoader != nullptr)
        {
            m_activePluginLoader->unload();
            m_activeProfile = nullptr;
            m_activePluginLoader = nullptr;
        }

        m_activePluginLoader = m_profileListModel.at(indexOfConnectorListModel);
        m_activeProfile = qobject_cast<BaseProfile*>(m_activePluginLoader->instance());
    }
}

KConcatenateRowsProxyModel* ProfileManager::cpuListModel()
{
    if (m_activeProfile != nullptr)
    {
        return &m_activeProfile->cpuList();
    }
    return nullptr;
}

KConcatenateRowsProxyModel *ProfileManager::registerListModel()
{
    if (m_activeProfile != nullptr)
    {
        return &m_activeProfile->registerList();
    }
    return nullptr;
}
