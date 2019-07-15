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

#include "Settings.h"
#include "ui_Settings.h"

#include <QHostAddress>
#include <QMessageBox>
#include <QValidator>
#include <QSettings>

Settings::Settings(QWidget *parent) :
    QDialog(parent),
    ui(new Ui::Settings)
{
    ui->setupUi(this);

    m_settings.beginGroup("TCP");
    ui->IPAddressLineEdit->setText(m_settings.value(m_settingsIPAddress,"").toString());
    ui->PortLineEdit->setText(QString::number(m_settings.value(m_settingsIPPort, 0).toInt()));
    m_settings.endGroup();
}

Settings::~Settings()
{
    delete ui;
}

void Settings::on_buttonBox_accepted()
{
    //User Clicked OK.
    //Check if ip address & port are valid.
    QHostAddress hostAddr = QHostAddress(ui->IPAddressLineEdit->text());
    if (hostAddr.isNull())
    {
        //Check if port is valid
        QMessageBox IPInvalidMsgBox;

        if (!portValid(ui->PortLineEdit->text()))
        {
            IPInvalidMsgBox.setText("Invalid IP Address & Port");
        }
        else
        {
            IPInvalidMsgBox.setText("Invalid IP Address");
        }
        IPInvalidMsgBox.setIcon(QMessageBox::Information);
        IPInvalidMsgBox.exec();
    }
    else if (!portValid(ui->PortLineEdit->text()))
    {
        //Check if port is valid
        QMessageBox portInvalidMsgBox;
        portInvalidMsgBox.setText("Invalid IP Port");
        portInvalidMsgBox.setIcon(QMessageBox::Information);
        portInvalidMsgBox.exec();
    }
    else
    {
        //Ip Address & Port are valid
        m_settings.beginGroup("TCP");
        m_settings.setValue(m_settingsIPAddress, ui->IPAddressLineEdit->text());
        m_settings.setValue(m_settingsIPPort, ui->PortLineEdit->text().toInt());
        m_settings.endGroup();
    }
    close();
}

bool Settings::portValid(QString portString)
{
    bool conversionOK = false;
    int port = portString.toInt(&conversionOK);
    if (conversionOK && port > 0 && port < std::numeric_limits<uint16_t>::max())
    {
        return true;
    }
    return false;
}
