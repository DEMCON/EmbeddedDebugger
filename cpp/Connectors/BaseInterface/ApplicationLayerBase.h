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

#ifndef APPLICATIONLAYERBASE_H
#define APPLICATIONLAYERBASE_H

#include <QObject>
class Register;
class Cpu;

class ApplicationLayerBase : public QObject
{
    Q_OBJECT
public:
    /**
     * @brief Constructor of ApplicationLayerBase
     * @param parent of this class
     */
    explicit ApplicationLayerBase(QObject* parent = nullptr) :
        QObject(parent){}

public slots:

    /**
     * @brief Scan for Cpu`s
     */
    virtual void scanForCpu() = 0;

    /**
     * @brief Query the current value of a Register
     * @param Register you want to query
     */
    virtual void queryRegister(const Register& registerToRead) = 0;

    /**
     * @brief Write the Register with a new value
     * @param Register you want to write
     */
    virtual void writeRegister(const Register& registerToWrite) = 0;

    /**
     * @brief Reset the time of the CPU
     * @param Cpu you want to reset the time.
     */
    virtual void resetTime(const Cpu& cpu) = 0;

    /**
     * @brief Config debug channel for a Register.
     * @param Register you want to config the debug channel for.
     */
    virtual void configDebugChannel(Register& registerToConfigDebugChannel) = 0;

    /**
     * @brief Get the decimation of the cpu
     * @param Cpu of which you want the decemation from.
     */
    virtual void getDecimation(const Cpu& cpu) = 0;

    /**
     * @brief Set the decimation of a cpu
     * @param Cpu of which you want to set the decimation.
     */
    virtual void setDecimation(const Cpu& cpu) = 0;
};

#endif // APPLICATIONLAYERBASE_H
