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

#ifndef REGISTER_H
#define REGISTER_H

#include <QVariant>
#include <QObject>
#include <QPair>
class Cpu;

class Register : public QObject
{
    Q_OBJECT
public:
    enum class ReadWrite{
        Unknown,
        Read,
        Write,
        ReadWrite,
    };

    enum class ChannelMode{
        Off = 0,
        OnChange = 0x1,
        LowSpeed = 0x2,
        Once = 0x03
    };

    enum class Source{
        HandWrittenOffset = 0,
        HandWrittenIndex = 0x10,
        SimulinkCApiOffset = 0x40,
        SimulinkCApiIndex = 0x50,
        AbsoluteAddress = 0x70,
        Unknown,
    };

    enum class VariableType
    {
        MemoryAlignment = 0x0, // memory alignment (given in size_n; typically 1 or 4; example: memory alignment = 4  addresses are a multiple of 4)
        Pointer = 0x1,
        Bool = 0x2,
        Char = 0x3,
        Short = 0x4,
        Int = 0x5,
        Long = 0x6,
        Float = 0x7,
        Double = 0x8,
        LongDouble = 0x9,
        TimeStamp = 0xA,        // time-stamp units in μs (uses 4 bytes for size_n!)
        Unknown,

    };

    Register(uint id, QString name, Register::ReadWrite readWrite, Register::VariableType variableType, Register::Source source, uint derefDepth, uint offset, Cpu& cpu);

    uint id() const {return m_id;}
    QString name() const {return m_name;}
    Register::ReadWrite readWrite() const {return m_readWrite;}
    Register::ChannelMode channelMode() const {return m_channelMode;}
    Register::Source source() const {return m_source;}
    Register::VariableType variableType() const {return m_variableType;}
    int getVariableTypeSize() const;
    uint derefDepth() const {return m_derefDepth;}
    uint32_t offset() const {return m_offset;}
    uint timeStampUnits() const {return m_timeStampUnits;}
    QVariant value() const {return m_registerValue;}
    uint timeStamp() const {return m_lastRegisterValueTimestamp;}
    Cpu& cpu() const {return m_cpu;}
    void configDebugChannel(ChannelMode newChannelMode);
    void setValue(const QVariant &value);
    void queryRegister();

    static Register::ReadWrite ReadWritefromString(const QString& enumString);
    static Register::Source SourcefromString(const QString&  enumString);
    static Register::VariableType variableTypeFromString(const QString&  enumString);
    static QString variableTypeToString(const Register::VariableType&  variableType);



public slots:
    void receivedNewRegisterValue(QVariant newRegisterValue);
    void receivedNewRegisterValue(QVariant newRegisterValue, uint timeStamp);

signals:
    void configDebugChannel(Register& Register);
    void writeRegister(Register& Register);
    void queryRegister(Register& Register);
    void registerDataChanged(Register& Register);

private:
    uint m_id;
    QString m_name;
    Register::ReadWrite m_readWrite;
    Register::VariableType m_variableType;
    Register::ChannelMode m_channelMode = Register::ChannelMode::Off;
    Register::Source m_source;
    uint m_derefDepth = 0;
    uint32_t m_offset = 0;
    uint m_timeStampUnits = 0;
    QVariant m_registerValue;
    uint m_lastRegisterValueTimestamp = 0;
    Cpu& m_cpu;
};

#endif // REGISTER_H
