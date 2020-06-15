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
using EmbeddedDebugger.DebugProtocol.Enums;
using EmbeddedDebugger.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;

namespace EmbeddedDebugger.DebugProtocol.EmbeddedConfiguration
{
    /// <summary>
    /// This class is used to parse an Embedded Configuration to an XML file and back
    /// </summary>
    public class XmlConfigurationParser
    {
        private static readonly Version currentVersion = new Version(0, 0, 0, 2);
        public static Version CurrentVersion { get => currentVersion; }

        #region Percentage
        // The percentage is to indicate to the outside world how far along the process of parsing a file is
        public uint Percentage
        {
            get
            {
                if (totalNumOfNodes == 0) return 0;
                uint per = currentNodeNumber * 100 / totalNumOfNodes;
                return per > 100 ? 100 : per;
            }
        }
        public event EventHandler PercentageChanged = delegate { };
        private uint totalNumOfNodes = 1;
        private uint currentNodeNumber = 0;
        #endregion

        #region ParseToFile
        /// <summary>
        /// Send an Embedded Configuration to an XML file
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="config"></param>
        public void ToFile(string filePath, EmbeddedConfig config)
        {
            // Set the basic parts for the percentage
            totalNumOfNodes = 10 + config.Registers.Count != 0 ? config.Registers.Last().Id : (uint)(config.Registers.Count);
            currentNodeNumber = 0;

            XmlDocument doc = new XmlDocument();
            doc.AppendChild(doc.CreateXmlDeclaration("1.0", "UTF-8", null));

            // Add the root node for the XML document
            XmlElement root = doc.CreateElement("EmbeddedDebugger");
            root.SetAttribute("version", currentVersion.ToString());
            doc.AppendChild(root);
            currentNodeNumber++;
            PercentageChanged(this, new EventArgs());

            XmlElement header = doc.CreateElement("Header");
            header.InnerText = "Embedded debugger, (C) DEMCON";
            root.AppendChild(header);
            currentNodeNumber++;
            PercentageChanged(this, new EventArgs());

            // If there is information about the CPU present, add that
            if (config.Cpu != null)
            {
                XmlElement cpu = doc.CreateElement("CPU");
                root.AppendChild(cpu);
                currentNodeNumber++;
                PercentageChanged(this, new EventArgs());

                XmlElement cpuName = doc.CreateElement("Name");
                cpuName.InnerText = config.Cpu.Name ?? "";
                cpu.AppendChild(cpuName);
                currentNodeNumber++;
                PercentageChanged(this, new EventArgs());

                XmlElement version = doc.CreateElement("Version");
                cpu.AppendChild(version);
                currentNodeNumber++;
                PercentageChanged(this, new EventArgs());

                XmlElement protVersion = doc.CreateElement("ProtocolVersion");
                protVersion.InnerText = config.Cpu.ProtocolVersionString ?? "";
                version.AppendChild(protVersion);
                currentNodeNumber++;
                PercentageChanged(this, new EventArgs());

                XmlElement applicationVersion = doc.CreateElement("ApplicationVersion");
                applicationVersion.InnerText = config.Cpu.ApplicationVersionString ?? "";
                version.AppendChild(applicationVersion);
                currentNodeNumber++;
                PercentageChanged(this, new EventArgs());
            }
            else if (config.CpuName != null)
            {
                XmlElement cpu = doc.CreateElement("CPU");
                root.AppendChild(cpu);
                currentNodeNumber++;
                PercentageChanged(this, new EventArgs());

                XmlElement cpuName = doc.CreateElement("Name");
                cpuName.InnerText = config.CpuName;
                cpu.AppendChild(cpuName);
                currentNodeNumber++;
                PercentageChanged(this, new EventArgs());

                XmlElement version = doc.CreateElement("Version");
                cpu.AppendChild(version);
                currentNodeNumber++;
                PercentageChanged(this, new EventArgs());

                XmlElement protVersion = doc.CreateElement("ProtocolVersion");
                protVersion.InnerText = config.ProtocolVersion ?? "";
                version.AppendChild(protVersion);
                currentNodeNumber++;
                PercentageChanged(this, new EventArgs());

                XmlElement applicationVersion = doc.CreateElement("ApplicationVersion");
                applicationVersion.InnerText = config.ApplicationVersion ?? "";
                version.AppendChild(applicationVersion);
                currentNodeNumber++;
                PercentageChanged(this, new EventArgs());
            }

            // Start with the registers
            XmlElement registersAvailable = doc.CreateElement("RegistersAvailable");
            root.AppendChild(registersAvailable);
            currentNodeNumber++;
            PercentageChanged(this, new EventArgs());

            foreach (Register reg in config.Registers)
            {
                registersAvailable.AppendChild(GetXMLFromRegister(reg, doc, (uint)config.Registers.IndexOf(reg)));
            }


            currentNodeNumber = 0;
            PercentageChanged(this, new EventArgs());
            Directory.CreateDirectory(Path.GetDirectoryName(filePath));
            doc.Save(filePath);
        }

        /// <summary>
        /// Retrieve XML for a Register
        /// </summary>
        /// <param name="reg">The register</param>
        /// <param name="doc">The document</param>
        /// <param name="id">If no ID is present, this can be used</param>
        /// <returns>The generated XML element</returns>
        private XmlElement GetXMLFromRegister(Register reg, XmlDocument doc, uint id)
        {
            currentNodeNumber = reg.Id;
            PercentageChanged(this, new EventArgs());

            // Create the XML node and add all information
            XmlElement register = doc.CreateElement("Register");
            register.SetAttribute("id", (reg.Id == 0 ? id : reg.Id).ToString());
            register.SetAttribute("name", reg.Name);
            register.SetAttribute("fullName", reg.FullName);
            register.SetAttribute("type", reg.VariableType.ToString().ToLower());
            if (reg.VariableType == VariableType.Unknown)
            {
                register.SetAttribute("unknownType", reg.VariableTypeName);
            }
            register.SetAttribute("size", reg.Size.ToString());
            register.SetAttribute("offset", reg.Offset.ToString());
            register.SetAttribute("source", reg.Source.ToString());
            register.SetAttribute("derefDepth", reg.DerefDepth.ToString());
            register.SetAttribute("show", Convert.ToInt32(reg.Show).ToString());
            register.SetAttribute("readWrite", reg.ReadWrite.ToString());

            // Add all child XML nodes
            foreach (Register childReg in reg.ChildRegisters)
            {
                register.AppendChild(GetXMLFromRegister(childReg, doc, (uint)reg.ChildRegisters.IndexOf(childReg)));
            }
            return register;
        }
        #endregion

        #region ParseFromFile
        /// <summary>
        /// This method parses an XML file to an Embedded Configuration
        /// </summary>
        /// <param name="filePath">The XML file</param>
        /// <returns>The parsed Embedded Configuration</returns>
        public EmbeddedConfig FromFile(string filePath, CpuNode node = null)
        {
            if (!File.Exists(filePath))
            {
                throw new ArgumentException($"The file does not exist! ({filePath})", "filePath");
            }
            totalNumOfNodes = (uint)File.ReadAllLines(filePath).Length;

            XmlDocument doc = new XmlDocument();
            doc.Load(filePath);

            XmlNode embeddedDebugger = doc.DocumentElement.SelectSingleNode("/EmbeddedDebugger");
            Version version = new Version(embeddedDebugger.Attributes["version"] != null ? embeddedDebugger.Attributes["version"].Value : "0.0.0.0");
            //Check if the version is not newer than this one
            if (CurrentVersion.CompareTo(version) < 0)
            {
                throw new NotSupportedException($"Version {version} is not supported!");
            }
            return ParseRegister(version, doc, node);
        }

        //private Dictionary<Version, Action> EmbeddedConfigurationXmlVersions = new Dictionary<Version, Action>()
        //{
        //    {new Version(),  VersionNone}
        //};

        /// <summary>
        /// This method is used to get the corrent method for parsing the XML file
        /// </summary>
        /// <param name="version">The version of the XML file</param>
        /// <param name="doc">The XML document</param>
        /// <returns>The parsed Embedded Configuration</returns>
        private EmbeddedConfig ParseRegister(Version version, XmlDocument doc, CpuNode node = null)
        {
            // This switch cases multiple times, have not been able to find a better way yet!!!
            // https://stackoverflow.com/questions/49409656/parse-file-differently-upon-different-version
            switch (version.Major)
            {
                case 0:
                    switch (version.Minor)
                    {
                        case 0:
                            switch (version.Build)
                            {
                                case 0:
                                    switch (version.Revision)
                                    {
                                        case 0:
                                            return VersionNone(doc, node);
                                        case 1:
                                            return Version0x0x0x1(doc, node);
                                        case 2:
                                            return Version0x0x0x2(doc, node);
                                    }
                                    break;
                            }
                            break;
                    }
                    break;
            }
            throw new NotImplementedException();
        }

        #region Version0x0x0x0
        /// <summary>
        /// Method for parsing the initial version of the XML documents
        /// </summary>
        /// <param name="doc">The XML document</param>
        /// <returns>The parsed Embedded Configuration</returns>
        private EmbeddedConfig VersionNone(XmlDocument doc, CpuNode node)
        {
            EmbeddedConfig returnable = new EmbeddedConfig();
            XmlNode readNode;
            XmlNode writeNode;
            try
            {
                readNode = doc.DocumentElement.SelectSingleNode("/EmbeddedDebugger/RegistersAvailable/Read");
                writeNode = doc.DocumentElement.SelectSingleNode("/EmbeddedDebugger/RegistersAvailable/Write");
            }
            catch (Exception e)
            {
#if DEBUG
                Console.WriteLine(e);
#endif
                throw new InvalidDataException($"Invalid XML file: {e.Message}");
            }
            Register r;
            uint counter = 0;
            foreach (XmlNode singleReadNode in readNode.ChildNodes)
            {
                currentNodeNumber = counter;
                PercentageChanged(this, new EventArgs());
                try
                {
                    r = new Register()
                    {
                        Id = counter++,
                        FullName = singleReadNode.Attributes["fullName"].Value,
                        Name = singleReadNode.Attributes["fullName"].Value.Substring(singleReadNode.Attributes["fullName"].Value.IndexOf(':') + 1).Trim(),
                        VariableType = (VariableType)Enum.Parse(typeof(VariableType), singleReadNode.Attributes["type"].Value.First().ToString().ToUpper() + singleReadNode.Attributes["type"].Value.Substring(1)),
                        Offset = Convert.ToUInt32(singleReadNode.Attributes["offset"].Value),
                        Size = Convert.ToInt32(singleReadNode.Attributes["size"].Value),
                        Source = (Source)Enum.Parse(typeof(Source), singleReadNode.Attributes["source"].Value),
                        Show = Convert.ToInt32(singleReadNode.Attributes["show"].Value) > 0,
                        ReadWrite = ReadWrite.Read,
                        CpuNode = node,
                    };
                }
                catch (ArgumentException ae)
                {
#if DEBUG
                    Console.WriteLine(ae);
#endif
                    throw;
                }
                returnable.Registers.Add(r);
            }
            counter = 0;
            foreach (XmlNode singleWriteNode in writeNode.ChildNodes)
            {
                currentNodeNumber = counter;
                PercentageChanged(this, new EventArgs());
                try
                {
                    r = new Register()
                    {
                        Id = counter++,
                        FullName = singleWriteNode.Attributes["fullName"].Value,
                        Name = singleWriteNode.Attributes["fullName"].Value.Substring(singleWriteNode.Attributes["fullName"].Value.IndexOf(':') + 1).Trim(),
                        VariableType = (VariableType)Enum.Parse(typeof(VariableType), singleWriteNode.Attributes["type"].Value.First().ToString().ToUpper() + singleWriteNode.Attributes["type"].Value.Substring(1)),
                        Offset = Convert.ToUInt32(singleWriteNode.Attributes["offset"].Value),
                        Size = int.Parse(singleWriteNode.Attributes["size"].Value),
                        Source = (Source)Enum.Parse(typeof(Source), singleWriteNode.Attributes["source"].Value),
                        Show = int.Parse(singleWriteNode.Attributes["show"].Value) > 0,
                        ReadWrite = ReadWrite.Write,
                        CpuNode = node,
                    };
                }
                catch (ArgumentException ae)
                {
#if DEBUG
                    Console.WriteLine(ae);
#endif
                    throw;
                }
                returnable.Registers.Add(r);
            }
            currentNodeNumber = 0;
            PercentageChanged(this, new EventArgs());
            return returnable;
        }
        #endregion

        #region Version0x0x0x1
        /// <summary>
        /// This method parses the XML based on version 0.0.0.1
        /// Which contains a bit more information, namely on which CPU it is written for
        /// </summary>
        /// <param name="doc"></param>
        /// <returns></returns>
        private EmbeddedConfig Version0x0x0x1(XmlDocument doc, CpuNode node)
        {
            EmbeddedConfig returnable = new EmbeddedConfig();
            XmlNode cpuNode;
            XmlNode version;
            try
            {
                cpuNode = doc.DocumentElement.SelectSingleNode("/EmbeddedDebugger/CPU");
                returnable.CpuName = cpuNode.ChildNodes[0].InnerText;
                version = cpuNode.ChildNodes[1];
                returnable.ProtocolVersion = version.ChildNodes[0].InnerText;
                returnable.ApplicationVersion = version.ChildNodes[1].InnerText;
            }
            catch (Exception e)
            {
#if DEBUG
                Console.WriteLine(e);
#endif
            }

            XmlNode registers;
            try
            {
                registers = doc.DocumentElement.SelectSingleNode("/EmbeddedDebugger/RegistersAvailable");
            }
            catch (Exception e)
            {
#if DEBUG
                Console.WriteLine(e);
#endif
                throw new InvalidDataException("Invalid XML file");
            }
            Register r;
            foreach (XmlNode singleReadNode in registers.ChildNodes)
            {
                try
                {
                    r = GetRegisterFromXML0x0x0x1((XmlElement)singleReadNode, ReadWrite.Read, null, node);
                }
                catch (ArgumentException ae)
                {
#if DEBUG
                    Console.WriteLine(ae);
#endif
                    throw;
                }
                returnable.Registers.Add(r);
            }
            currentNodeNumber = 0;
            PercentageChanged(this, new EventArgs());
            return returnable;
        }

        /// <summary>
        /// This method is used by version 0.0.0.1 (at least)
        /// It parses one register and is recursively used to parse the child nodes
        /// </summary>
        /// <param name="element">The register element</param>
        /// <param name="readWrite">Definition of read or write</param>
        /// <param name="parent">The parent, so that it can find it's way up the tree</param>
        /// <returns>The register</returns>
        private Register GetRegisterFromXML0x0x0x1(XmlElement element, ReadWrite readWrite, Register parent, CpuNode node)
        {
            Register r = new Register()
            {
                Id = Convert.ToUInt32(element.Attributes["id"].Value),
                FullName = element.Attributes["fullName"].Value,
                Name = element.Attributes["name"] != null ? element.Attributes["name"].Value : element.Attributes["fullName"].Value.Substring(element.Attributes["fullName"].Value.LastIndexOf('.') + 1).Trim(),
                VariableType = (VariableType)Enum.Parse(typeof(VariableType), element.Attributes["type"].Value.First().ToString().ToUpper() + element.Attributes["type"].Value.Substring(1), true),
                Offset = Convert.ToUInt32(element.Attributes["offset"].Value),
                Size = int.Parse(element.Attributes["size"].Value),
                Source = (Source)Enum.Parse(typeof(Source), element.Attributes["source"].Value),
                Show = int.Parse(element.Attributes["show"].Value) > 0,
                DerefDepth = Convert.ToInt32(element.Attributes["derefDepth"].Value),
                ReadWrite = readWrite,
                Parent = parent,
                CpuNode = node,
            };
            currentNodeNumber = r.Id;
            PercentageChanged(this, new EventArgs());
            if (r.VariableType == VariableType.Unknown)
            {
                r.VariableTypeName = element.Attributes["unknownType"].Value;
            }
            if (element.HasChildNodes)
            {
                foreach (XmlElement e in element.ChildNodes)
                {
                    r.ChildRegisters.Add(GetRegisterFromXML0x0x0x1(e, readWrite, r, node));
                }
            }
            return r;
        }
        #endregion

        #region Version0x0x0x2
        /// <summary>
        /// This method parses the XML based on version 0.0.0.2
        /// Which contains a bit more information, namely on which CPU it is written for
        /// </summary>
        /// <param name="doc"></param>
        /// <returns></returns>
        private EmbeddedConfig Version0x0x0x2(XmlDocument doc, CpuNode node)
        {
            EmbeddedConfig returnable = new EmbeddedConfig();
            XmlNode cpuNode;
            XmlNode version;
            try
            {
                cpuNode = doc.DocumentElement.SelectSingleNode("/EmbeddedDebugger/CPU");
                returnable.CpuName = cpuNode.ChildNodes[0].InnerText;
                version = cpuNode.ChildNodes[1];
                returnable.ProtocolVersion = version.ChildNodes[0].InnerText;
                returnable.ApplicationVersion = version.ChildNodes[1].InnerText;
            }
            catch (Exception e)
            {
#if DEBUG
                Console.WriteLine(e);
#endif
            }

            XmlNode registers;
            try
            {
                registers = doc.DocumentElement.SelectSingleNode("/EmbeddedDebugger/RegistersAvailable");
            }
            catch (Exception e)
            {
#if DEBUG
                Console.WriteLine(e);
#endif
                throw new InvalidDataException("Invalid XML file");
            }
            Register r;
            foreach (XmlNode singleReadNode in registers.ChildNodes)
            {
                // Make sure we do not add any comments...
                if (singleReadNode is XmlComment) continue;
                try
                {
                    r = GetRegisterFromXML0x0x0x2((XmlElement)singleReadNode, null, node);
                }
                catch (ArgumentException ae)
                {
#if DEBUG
                    Console.WriteLine(ae);
#endif
                    throw;
                }

                // Try to add the register as a child
                if (r.Name.Contains(".") && AddToChildRegister(returnable.Registers, r, r.FullName))
                { }
                else
                {
                    // Other wise, just add it to the list
                    returnable.Registers.Add(r);
                }
            }
            currentNodeNumber = 0;
            PercentageChanged(this, new EventArgs());
            return returnable;
        }

        private bool AddToChildRegister(IEnumerable<Register> registers, Register r, string name)
        {
            bool returnable = false;
            if (name.Contains(".") && registers != null)
            {
                if (registers.FirstOrDefault(x => x.Name.EndsWith(name.Substring(0, name.IndexOf(".", StringComparison.Ordinal)))) is Register parent)
                {
                    if (!this.AddToChildRegister(parent.ChildRegisters, r, name.Substring(name.IndexOf(".", StringComparison.Ordinal) + 1)))
                    {
                        parent.ChildRegisters.Add(r);
                    }
                    returnable = true;
                }

            }
            return returnable;
        }

        /// <summary>
        /// This method is used by version 0.0.0.1 (at least)
        /// It parses one register and is recursively used to parse the child nodes
        /// </summary>
        /// <param name="element">The register element</param>
        /// <param name="readWrite">Definition of read or write</param>
        /// <param name="parent">The parent, so that it can find it's way up the tree</param>
        /// <returns>The register</returns>
        private Register GetRegisterFromXML0x0x0x2(XmlElement element, Register parent, CpuNode node)
        {
            Register r = new Register()
            {
                Id = Convert.ToUInt32(element.Attributes["id"].Value),
                FullName = element.Attributes["fullName"].Value,
                Name = element.Attributes["name"] != null ? element.Attributes["name"].Value : element.Attributes["fullName"].Value.Substring(element.Attributes["fullName"].Value.LastIndexOf('.') + 1).Trim(),
                VariableType = (VariableType)Enum.Parse(typeof(VariableType), element.Attributes["type"].Value.First().ToString().ToUpper() + element.Attributes["type"].Value.Substring(1), true),
                Offset = Convert.ToUInt32(element.Attributes["offset"].Value),
                Size = int.Parse(element.Attributes["size"].Value),
                Source = (Source)Enum.Parse(typeof(Source), element.Attributes["source"].Value),
                Show = int.Parse(element.Attributes["show"].Value) > 0,
                DerefDepth = Convert.ToInt32(element.Attributes["derefDepth"].Value),
                ReadWrite = (ReadWrite)Enum.Parse(typeof(ReadWrite), element.Attributes["readWrite"].Value),
                Parent = parent,
                CpuNode = node,
            };
            currentNodeNumber = r.Id;
            PercentageChanged(this, new EventArgs());
            if (r.VariableType == VariableType.Unknown)
            {
                r.VariableTypeName = element.Attributes["unknownType"].Value;
            }
            if (element.HasChildNodes)
            {
                foreach (XmlElement e in element.ChildNodes)
                {
                    r.ChildRegisters.Add(GetRegisterFromXML0x0x0x2(e, r, node));
                }
            }
            return r;
            #endregion
            #endregion
        }
    }
}
