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

#ifndef APPLICATIONLAYERV0_H
#define APPLICATIONLAYERV0_H

#include "../BaseInterface/ApplicationLayerBase.h"
#include "PresentationLayerV0.h"

class ApplicationLayerV0 : public ApplicationLayerBase
{
    Q_OBJECT
public:
    explicit ApplicationLayerV0(PresentationLayerV0& presentationLayerV0, QObject *parent = nullptr);

public slots:
    /**
    * @copydoc ApplicationLayerBase::scanForCpu()
    */
    void scanForCpu() override;

    /**
    * @copydoc ApplicationLayerBase::queryRegister()
    */
    void queryRegister(const Register& registerToRead) override;

    /**
    * @copydoc ApplicationLayerBase::writeRegister()
    */
    void writeRegister(const Register& registerToWrite) override;

    /**
    * @copydoc ApplicationLayerBase::resetTime()
    */
    void resetTime(const Cpu& cpu) override;

    /**
    * @copydoc ApplicationLayerBase::configDebugChannel()
    */
    void configDebugChannel(Register& registerToConfigDebugChannel) override;

    /**
    * @copydoc ApplicationLayerBase::getDecimation()
    */
    void getDecimation(const Cpu& cpu) override;

    /**
    * @copydoc ApplicationLayerBase::setDecimation()
    */
    void setDecimation(const Cpu& cpu) override;

private:
    PresentationLayerV0& m_presentationLayer; /**< Reference to PresentationLayerV0 for easy access this class*/
};

#endif // APPLICATIONLAYERV0_H
