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
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EmbeddedDebugger.DebugProtocol;

namespace EmbeddedDebugger.Model.Logging
{
    public class ValueLogger
    {
        #region Fields
        private bool isLogging;
        private FileType fileType;
        private string directory;
        private string fileName;
        private string fileNameTemplate;
        private bool separateFilePerCpuNode;
        private TimeStampUsage timeStampUsage;
        private string separator;
        #region txt
        private bool txtAddVersionInfoToHeader;
        #endregion
        #region csv
        private bool csvUseHeader;
        #endregion
        private StreamWriter fileWriter;
        private List<Register> logRegisters;
        private int counter;
        private Dictionary<byte, StreamWriter> fileWriters;
        #endregion

        #region Properties
        public bool IsLogging { get => isLogging; set => isLogging = value; }
        public FileType FileType { get => fileType; set => fileType = value; }
        public string Directory { get => directory; set => directory = value; }
        public string FileName { get => fileName; set => fileName = value; }
        public string FileNameTemplate { get => fileNameTemplate; set => fileNameTemplate = value; }
        public bool SeparateFilePerCpuNode { get => separateFilePerCpuNode; set => separateFilePerCpuNode = value; }
        public TimeStampUsage TimeStampUsage { get => timeStampUsage; set => timeStampUsage = value; }
        #region txt
        public bool TxtAddVersionInfoToHeader { get => txtAddVersionInfoToHeader; set => txtAddVersionInfoToHeader = value; }
        #endregion
        #region csv
        public string Separator { get => separator; set => separator = value; }
        public bool CsvUseHeader { get => csvUseHeader; set => csvUseHeader = value; }
        #endregion
        public List<Register> LogRegisters { get => logRegisters; set => logRegisters = value; }
        #endregion

        #region EventHandlers
        public event EventHandler StartLogging;
        public event EventHandler StopLogging;
        #endregion

        public ValueLogger()
        {
            logRegisters = new List<Register>();
        }

        public bool Start()
        {
            if (logRegisters == null || logRegisters.Count == 0) throw new ArgumentException("LogRegisters was empty");
            counter = 1;
            if (separateFilePerCpuNode)
            {
                fileWriters = new Dictionary<byte, StreamWriter>();
                try
                {
                    System.IO.Directory.CreateDirectory(directory);
                }
                catch (Exception e)
                {
#if DEBUG
                    Console.WriteLine(e);
#endif
                    throw;
                }
                StreamWriter writer;
                foreach (CpuNode node in logRegisters.Select(x => x.CpuNode).Distinct())
                {
                    SetNewFileName();
                    fileName = fileName.Replace("{CPU}", node.Name);
                    fileName = fileName.Replace("{Serial}", node.SerialNumber);
                    fileName = fileName.Replace("{CPUID}", node.Id.ToString());
                    try
                    {
                        writer = new StreamWriter($"{directory}\\{fileName}", true);
                    }
                    catch (Exception e)
                    {
#if DEBUG
                        Console.WriteLine(e);
#endif
                        throw;
                    }
                    fileWriters.Add(node.Id, writer);
                    writer.Write(GetHeader(node));
                }
            }
            else
            {
                SetNewFileName();
                try
                {
                    System.IO.Directory.CreateDirectory(directory);
                    fileWriter = new StreamWriter($"{directory}\\{fileName}", true);
                }
                catch (Exception e)
                {
#if DEBUG
                    Console.WriteLine(e);
#endif
                    throw;
                }
                fileWriter.Write(GetHeader());
            }
            isLogging = true;
            StartLogging(this, new EventArgs());
            return true;
        }

        public void Stop()
        {
            if (separateFilePerCpuNode)
            {
                foreach (StreamWriter writer in fileWriters.Values)
                {
                    writer.WriteLine(GetFooter());
                    writer.Close();
                }
                fileWriters.Clear();
            }
            else
            {
                try
                {
                    fileWriter.WriteLine(GetFooter());
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
                fileWriter.Close();
            }
            StopLogging(this, new EventArgs());
            isLogging = false;
        }

        public void NewValuesReceived(object sender, long timeStamp)
        {
            if (!isLogging) return;
            if (separateFilePerCpuNode)
            {
                foreach (StreamWriter writer in fileWriters.Values)
                {
                    switch (timeStampUsage)
                    {
                        case TimeStampUsage.None:
                            break;
                        case TimeStampUsage.Relative:
                            writer.Write(timeStamp.ToString());
                            break;
                        case TimeStampUsage.Absolute:
                            writer.Write(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffff"));
                            break;
                    }
                }
                foreach (Register r in logRegisters)
                {
                    fileWriters[r.CpuId].Write($"{separator}{r.Value}");
                }
                foreach (StreamWriter writer in fileWriters.Values)
                {
                    writer.WriteLine();
                }
            }
            else
            {
                StringBuilder line = new StringBuilder();
                switch (fileType)
                {
                    case FileType.txt:
                    case FileType.csv:
                        switch (timeStampUsage)
                        {
                            case TimeStampUsage.None:
                                break;
                            case TimeStampUsage.Relative:
                                line.Append(timeStamp.ToString());
                                break;
                            case TimeStampUsage.Absolute:
                                line.Append(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffff"));
                                break;
                        }
                        foreach (Register r in logRegisters)
                        {
                            line.Append($"{separator}{r.Value}");
                        }
                        break;
                }
                fileWriter.WriteLine(line.ToString());
            }
        }

        public void RegisterLoggingChanged(object sender, Register r)
        {
            //if (r.Log && !logRegisters.Any(x => x.ID == r.ID && x.FullName == r.FullName && x.CpuID == r.CpuID))
            //{
             //   logRegisters.Add(r);
            //}
            //else if (!r.Log && logRegisters.Any(x => x.ID == r.ID && x.FullName == r.FullName && x.CpuID == r.CpuID))
            //{
              //  logRegisters.Remove(r);
            //}
        }

        private void SetNewFileName()
        {
            string output = fileNameTemplate;
            output = output.Replace("{yyyy}", DateTime.Now.Year.ToString());
            output = output.Replace("{MM}", DateTime.Now.Month.ToString());
            output = output.Replace("{dd}", DateTime.Now.Day.ToString());
            output = output.Replace("{HH}", DateTime.Now.Hour.ToString());
            output = output.Replace("{mm}", DateTime.Now.Minute.ToString());
            output = output.Replace("{ss}", DateTime.Now.Second.ToString());
            output = output.Replace("{Counter}", counter.ToString());
            output += $".{FileType.ToString()}";
            fileName = output;
        }

        private string GetHeader(CpuNode node = null)
        {
            StringBuilder header = new StringBuilder();
            switch (fileType)
            {
                case FileType.txt:
                    header.AppendLine("------------------------------");
                    header.AppendLine($"Logging started at: {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffff")}");
                    if (node != null && txtAddVersionInfoToHeader)
                    {
                        header.AppendLine();
                        header.AppendLine($"Concerning CPU: {node}");
                    }
                    header.AppendLine("------------------------------");
                    if (timeStampUsage == TimeStampUsage.Absolute || timeStampUsage == TimeStampUsage.Relative)
                    {
                        header.Append($"TimeStamp");
                    }
                    foreach (Register r in separateFilePerCpuNode ? logRegisters.Where(x => x.CpuNode == node) : logRegisters)
                    {
                        header.Append($"{separator}{r.FullName}");
                    }
                    break;
                case FileType.csv:
                    if (!csvUseHeader) return "";
                    if (timeStampUsage == TimeStampUsage.Absolute || timeStampUsage == TimeStampUsage.Relative)
                    {
                        header.Append($"TimeStamp");
                    }
                    foreach (Register r in separateFilePerCpuNode ? logRegisters.Where(x => x.CpuNode == node) : logRegisters)
                    {
                        header.Append($"{separator}");
                        header.Append(separateFilePerCpuNode ? $"{r.FullName}" : $"[{r.CpuNode.Name}:{r.CpuId}]{r.FullName}");
                    }
                    break;
            }
            header.AppendLine();
            return header.ToString();
        }

        private string GetFooter()
        {
            StringBuilder footer = new StringBuilder();
            switch (fileType)
            {
                case FileType.txt:
                    footer.AppendLine("------------------------------");
                    footer.AppendLine($"Logging stopped at: {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffff")}");
                    footer.AppendLine("------------------------------");
                    break;
                case FileType.csv:
                    break;
            }
            return footer.ToString();
        }
    }
}
