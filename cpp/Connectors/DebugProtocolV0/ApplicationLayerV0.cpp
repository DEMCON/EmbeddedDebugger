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

#include "ApplicationLayerV0.h"

ApplicationLayerV0::ApplicationLayerV0(PresentationLayerV0& presentationLayerV0, QObject *parent) :
    ApplicationLayerBase(parent),
    m_presentationLayer(presentationLayerV0)
{
}

void ApplicationLayerV0::scanForCpu()
{
    m_presentationLayer.scanForCpu();
}

void ApplicationLayerV0::queryRegister(const Register &registerToRead)
{
    m_presentationLayer.queryRegister(registerToRead);
}

void ApplicationLayerV0::writeRegister(const Register &registerToWrite)
{
    m_presentationLayer.writeRegister(registerToWrite);
}

void ApplicationLayerV0::resetTime(const Cpu& cpu)
{
    m_presentationLayer.resetTime(cpu.id());
}

void ApplicationLayerV0::configDebugChannel(Register &registerToConfigDebugChannel)
{
    m_presentationLayer.configDebugChannel(registerToConfigDebugChannel);
}

void ApplicationLayerV0::getDecimation(const Cpu& cpu)
{
    m_presentationLayer.getDecimation(cpu.id());
}

void ApplicationLayerV0::setDecimation(const Cpu& cpu)
{
    m_presentationLayer.setDecimation(cpu.id(),cpu.decimation());
}
