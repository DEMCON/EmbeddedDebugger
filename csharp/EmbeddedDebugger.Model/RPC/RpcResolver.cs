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
using CookComputing.XmlRpc;
using EmbeddedDebugger.Connectors.Serial;
using EmbeddedDebugger.Connectors.TCP;
using EmbeddedDebugger.DebugProtocol.Enums;
using EmbeddedDebugger.Model.Logging;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmbeddedDebugger.Model.RPC
{
    public class RpcResolver : ListenerService
    {
        private DebugProtocol dp;
        private ModelManager mm;
        public RpcResolver(ModelManager mm, DebugProtocol dp)
        {
            this.mm = mm;
            this.dp = dp;
        }

        [XmlRpcMethod("GetConnected")]
        public bool GetConnected()
        {
            return dp.IsConnected;
        }

        [XmlRpcMethod("Connect")]
        public void Connect()
        {
            dp.Connect();
        }

        [XmlRpcMethod("ConnectTCP")]
        public void ConnectTCP(string hostname, int port)
        {
            TcpConnector con = (TcpConnector)dp.Connectors.First(x => x is TcpConnector);
            con.HostName = hostname;
            con.Port = port;
            dp.Connector = con;
            Connect();
        }

        [XmlRpcMethod("ConnectSerial")]
        public void ConnectSerial(string portName, int baudRate)
        {
            SerialConnector con = (SerialConnector)dp.Connectors.First(x => x is SerialConnector);
            con.PortName = portName;
            con.BaudRate = baudRate;
            con.Parity = con.Parity <= 0 ? Parity.None : con.Parity;
            con.DataBits = con.DataBits <= 0 ? 8 : con.DataBits;
            con.StopBits = con.StopBits <= 0 ? StopBits.One : con.StopBits;
            con.Hanshake = con.Hanshake <= 0 ? Handshake.None : con.Hanshake;
            con.ReadTimeout = con.ReadTimeout <= 0 ? 500 : con.ReadTimeout;
            con.WriteTimeout = con.WriteTimeout <= 0 ? 500 : con.WriteTimeout;
            dp.Connector = con;
            Connect();
        }

        [XmlRpcMethod("ConnectSerialExtended")]
        public void ConnectSerial(string portName, int baudRate, Parity parity, int databits, StopBits stopbits, Handshake handshake, int readTimeout, int writeTimeout)
        {
            SerialConnector con = (SerialConnector)dp.Connectors.First(x => x is SerialConnector);
            con.PortName = portName;
            con.BaudRate = baudRate;
            con.Parity = parity;
            con.DataBits = databits;
            con.StopBits = stopbits;
            con.Hanshake = handshake;
            con.ReadTimeout = readTimeout;
            con.WriteTimeout = writeTimeout;
            dp.Connector = con;
            Connect();
        }

        [XmlRpcMethod("Disconnect")]
        public void Disconnect()
        {
            dp.Disconnect();
        }

        [XmlRpcMethod("GetRegisterValue")]
        public string GetRegisterValue(int nodeID, int registerID)
        {
            foreach (CpuNode node in mm.Nodes)
            {
                if (node.ID != nodeID) continue;
                if (node.Registers.Any(x => x.ID == registerID))
                {
                    return node.Registers.First(x => x.ID == registerID).Value;
                }
            }
            return "No such value found";
        }

        [XmlRpcMethod("SetRegisterValue")]
        public bool SetRegisterValue(int nodeID, int registerID, string value)
        {
            foreach (CpuNode node in mm.Nodes)
            {
                if (node.ID != nodeID) continue;
                if (node.Registers.Any(x => x.ID == registerID))
                {
                    node.Registers.First(x => x.ID == registerID).Value = value;
                    return true;
                }
            }
            return false;
        }

        [XmlRpcMethod("SetDebugChannel")]
        public void SetDebugChannel(int nodeID, int registerID, ChannelMode mode)
        {
            if (!mm.Nodes.Any(x => x.ID == nodeID)) return;
            CpuNode node = mm.Nodes.First(x => x.ID == nodeID);
            if (!node.Registers.Any(x => x.ID == registerID)) return;
            node.Registers.First(x => x.ID == registerID).ChannelMode = mode;
        }

        [XmlRpcMethod("SetPlotting")]
        public void SetPlotting(int nodeID, int registerID, bool plotting)
        {
            if (!mm.Nodes.Any(x => x.ID == nodeID)) return;
            CpuNode node = mm.Nodes.First(x => x.ID == nodeID);
            if (!node.Registers.Any(x => x.ID == registerID)) return;
            //node.Registers.First(x => x.ID == registerID).Plot = plotting;
        }

        [XmlRpcMethod("SetLogging")]
        public void SetLogging(int nodeID, int registerID, bool logging)
        {
            if (!mm.Nodes.Any(x => x.ID == nodeID)) return;
            CpuNode node = mm.Nodes.First(x => x.ID == nodeID);
            if (!node.Registers.Any(x => x.ID == registerID)) return;
            node.Registers.First(x => x.ID == registerID).Log = logging;
        }

        [XmlRpcMethod("StartLogging")]
        public void StartLogging(string directory, string fileName, FileType extention, bool seperateFile, string separator, TimeStampUsage timestamp)
        {
            ValueLogger logger = mm.Logger;
            logger.Directory = directory;
            logger.FileNameTemplate = fileName;
            logger.FileType = extention;
            logger.SeparateFilePerCpuNode = seperateFile;
            logger.Separator = separator;
            logger.TimeStampUsage = timestamp;
            logger.Start();
        }

        [XmlRpcMethod("StartLoggingCsv")]
        public void StartLoggingCsv(string directory, string fileName, bool separateFile, string separator, TimeStampUsage timeStamp, bool header)
        {
            mm.Logger.CsvUseHeader = header;
            StartLogging(directory, fileName, FileType.csv, separateFile, separator, timeStamp);
        }

        [XmlRpcMethod("StartLoggingTxt")]
        public void StartLoggingTxt(string directory, string fileName, bool separateFile, string separator, TimeStampUsage timeStamp, bool header)
        {
            mm.Logger.TxtAddVersionInfoToHeader = header;
            StartLogging(directory, fileName, FileType.csv, separateFile, separator, timeStamp);
        }

        [XmlRpcMethod("StopLogging")]
        public void StopLogging()
        {
            ValueLogger logger = mm.Logger;
            logger.Stop();
        }

        [XmlRpcMethod("SendDebugString")]
        public void SendDebugString(int nodeID, string debugString)
        {
            dp.SendDebugString((byte)nodeID, debugString);
        }
    }
}
