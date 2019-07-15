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

#ifndef GENERICPROFILE_H
#define GENERICPROFILE_H

#include <QObject>
#include "Medium/Medium.h"
#include "kconcatenaterowsproxymodel.h"

/**
 * @brief Base for every profile that will be created
 */
class BaseProfile : public QObject
{
    Q_OBJECT
public:
    /**
     * @brief BaseProfile constructor
     * @param parent of this class
     */
    explicit BaseProfile(QObject *parent = nullptr) :
        QObject(parent){}

    /**
     * @brief Deconstructor of BaseProfile
     * Removes all media in m_mediumList.
     */
    virtual ~BaseProfile()
    {
        for(auto medium : qAsConst(m_mediumList))
        {
            medium->deleteLater();
        }
        m_mediumList.clear();
    }

    /**
     * @brief addMedium to m_mediumList.
     * Adds cpuList from newMedium to m_combinedCpuList.
     * Adds registerList from newMedium to m_combinedRegisterList.
     * @param newMedium to append. If nullptr medium will not be appended.
     */
    void addMedium(Medium* newMedium)
    {
        if (newMedium != nullptr)
        {
            m_mediumList.append(newMedium);
            m_combinedCpuList.addSourceModel(&newMedium->cpuListModel());
            m_combinedRegisterList.addSourceModel(&newMedium->registerListModel());
        }
    }

    /**
     * @brief cpuList
     * @return combined list of all the Cpu`s from different media from m_mediumList.
     */
    KConcatenateRowsProxyModel& cpuList() {return m_combinedCpuList;}

    /**
     * @brief registerList
     * @return combined list of all the registers from different media from m_mediumList.
     */
    KConcatenateRowsProxyModel& registerList() {return m_combinedRegisterList;}


    /**
     * @brief connect each medium that is added to m_mediumList
     */
    void connect(){
       for(auto Medium : qAsConst(m_mediumList))
       {
           Medium->connect();
       }
    }
    /**
     * @brief disconnect each medium that is added to m_mediumList
     */
    void disconnect(){
       for(auto Medium : qAsConst(m_mediumList))
       {
           Medium->disconnect();
       }
    }
    /**
     * @brief showSettings from each medium that is added to m_mediumList;
     */
    void showSettings(){
        for(auto Medium : qAsConst(m_mediumList))
        {
            Medium->showSettings();
        }
    }

    /**
     * @brief get mediumList
     * @return QList containing all the media that is added to this baseProfile.
     */
    QList<Medium*>& mediumList() {return m_mediumList;}

private:
    QList<Medium*> m_mediumList; /**< QList containing pointers to Medium */
    KConcatenateRowsProxyModel m_combinedCpuList; /**< Proxy model containing all the cpu`s */
    KConcatenateRowsProxyModel m_combinedRegisterList; /**< Proxy model containing all the registers */
};

#define ProfileInterface_iid "DEMCON.EmbeddedDebugger.ProfileInterface"
Q_DECLARE_INTERFACE(BaseProfile, ProfileInterface_iid)

#endif // GENERICPROFILE_H
