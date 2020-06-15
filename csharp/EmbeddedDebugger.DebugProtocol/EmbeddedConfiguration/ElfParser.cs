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
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace EmbeddedDebugger.DebugProtocol.EmbeddedConfiguration
{
    /// <summary>
    /// This class parses an .elf file to an Embedded Configuration
    /// </summary>
    public class ElfParser
    {
        #region Percentage
        // The percentage is to indicate to the outside world how far along the process of parsing a file is
        private int percentage = 0;
        public int Percentage { get => percentage; }
        public event EventHandler PercentageUpped = delegate { };
        #endregion

        /// <summary>
        /// This method parses a .elf file into an Embedded Configuration
        /// </summary>
        /// <param name="filePath">The path of the .elf file</param>
        /// <returns>The parserd Embedded Configuration</returns>
        public EmbeddedConfig ParseFile(string filePath)
        {
            if (!File.Exists(filePath)) throw new FileNotFoundException("Not found", filePath);
            EmbeddedConfig ec = new EmbeddedConfig();
            percentage = 10;
            PercentageUpped(this, new EventArgs());
            // Start the fromelf.exe which actually does the parsing of the .elf to text
            File.Delete("config.txt");
            var proc = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "Utilities/fromelf.exe",
                    Arguments = $"\"{filePath}\" --tool_variant=mdk_lite -a --expandarrays -w --output=config.txt",
                    //Arguments = $"\"{filePath}\" --tool_variant=mdk_lite -a -w --output=config.txt",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true,
                    Verb = "runas",
                }
            };
            proc.Start();
            string line;
            while (!proc.StandardOutput.EndOfStream)
            {
                line = proc.StandardOutput.ReadLine();

            }
            //while(proc.StandardOutput.ReadLineAsync(delegate { }))
            //while (!proc.StandardOutput.EndOfStream)
            //{
            //    proc.Refresh();
            //    Thread.Sleep(100);
            //}
            // Get the lines of the output of fromelf.exe
            string[] output = File.ReadAllLines("config.txt");// proc.StandardOutput.ReadToEnd().Replace("\r", "").Split('\n');
            string[] values;

            Register r;
            uint id = 0;
            StringBuilder name = new StringBuilder();
            StringBuilder type = new StringBuilder();
            uint address;
            int size = -1;
            int derefAmount = 0;

            // Go through all of the lines
            for (int j = 0; j < output.Length; j++)
            {
                // Determine how far we are with parsing the output
                percentage = 10 + (j * 90 / output.Length);
                PercentageUpped(this, new EventArgs());

                // Get the four values from the line
                values = output[j].Split(' ').Where(x => !string.IsNullOrEmpty(x.Trim())).ToArray();
                // If the address cannot be parsed, this is not a proper line
                try
                {
                    address = Convert.ToUInt32(values[0].Substring(2), 16);
                }
                catch (Exception e)
                {
                    // When caught, this exception is not interesting, all it explains is that it is not a valid line
                    Debug.WriteLine(e);
                    continue;
                }
                // Go through all of the "words" in the line
                for (int i = 1; i < values.Length; i++)
                {
                    if (string.IsNullOrEmpty(values[i])) continue;
                    if (size == -1)
                    {
                        size = Convert.ToInt32(new string(values[i].Substring(2).TakeWhile(c => IsHex(c)).ToArray()), 16);
                    }
                    // fromelf.exe adds a * to the name of the variable, indicating that it is a pointer
                    else if (values[i].Equals("*") && string.IsNullOrEmpty(name.ToString().Replace("*", "")))
                    {
                        derefAmount++;
                    }
                    else if (name.Length == 0 || string.IsNullOrEmpty(name.ToString().Replace("*", "")))
                    {
                        name.Append(values[i]);
                    }
                    else
                    {
                        type.Append(values[i]);
                        type.Append(" ");
                    }
                }
                // Create a new register from the gathered info
                r = new Register()
                {
                    FullName = name.ToString(),
                    Name = name.ToString().Substring(name.ToString().LastIndexOf('.') + 1),
                    Offset = address,
                    Size = size,
                    DerefDepth = derefAmount,
                    Source = Source.ElfParsed,
                    ReadWrite = ReadWrite.ReadWrite,
                    Show = false,
                    Id = id++,
                };
                if (Enum.TryParse(type.ToString(), out VariableType varType))
                {
                    r.VariableType = varType;
                }
                else
                {
                    r.VariableType = MessageCodec.FindVariableType(type.ToString());
                    r.VariableTypeName = type.ToString();
                }

                // Determine where the Register is supposed to be placed
                Register parent = null;
                if (name.ToString().Contains('[') && name.ToString().Contains(']') && (!name.ToString().Contains('.') || name.ToString().LastIndexOf('.') < name.ToString().LastIndexOf(']')))
                {
                    parent = GetParent(ec.Registers, name.ToString().Remove(name.ToString().LastIndexOf('[')), null);
                    parent.ChildRegisters.Add(r);
                }
                else if (name.ToString().Contains('.'))
                {
                    parent = GetParent(ec.Registers, name.ToString().Remove(name.ToString().LastIndexOf('.')), null);
                    parent.ChildRegisters.Add(r);
                }
                else
                {
                    ec.Registers.Add(r);
                }
                r.Parent = parent;
                size = -1;
                name.Clear();
                type.Clear();
                derefAmount = 0;
            }
            percentage = 0;
            PercentageUpped(this, new EventArgs());
            return ec;
        }

        #region HelperMethods
        /// <summary>
        /// This method is used to find the parent of a node
        /// This node can be part of a large tree and this method finds the lowest parent for that new node
        /// Example:
        /// SomeStruct.ArrayOfStructs[0].ArrayOfIntegers[1]
        /// This is taken down part by part
        /// Until finally, ArrayOfIntegers as array is returned as the parent of ArrayOfIntegers[1]
        /// Yes, recursion is used here.
        /// "To understand recursion, you must first understand recursion"
        /// </summary>
        /// <param name="toSearch">The list to search the parent in</param>
        /// <param name="parentName">The "path" to the lowest parent, if the parent is not in the child registers, return this</param>
        /// <param name="parent">The previous parent</param>
        /// <returns>The parent register</returns>
        private Register GetParent(IList<Register> toSearch, string parentName, Register parent)
        {
            int firstIndexOfDot = parentName.IndexOf('.');
            int firstIndexOfBracket = parentName.IndexOf('[');
            Register reg = null;

            // If the complete parent string is present in the list, return that one
            if (toSearch.Any(x => x.Name.Equals(parentName)))
            {
                reg = toSearch.First(x => x.Name.Equals(parentName));
            }
            // If the parent string contains a period character, remove all characters after that and search deeper
            else if (firstIndexOfDot >= 0 && toSearch.Any(x => x.Name.Equals(parentName.Remove(firstIndexOfDot))))
            {
                parent = toSearch.First(x => x.Name.Equals(parentName.Remove(firstIndexOfDot)));
                reg = GetParent(parent.ChildRegisters, parentName.Substring(firstIndexOfDot + 1), parent);
            }
            // If the parent string contains a chevron, remove all characters after that and search deeper
            else if (toSearch.Any(x => x.Name.Equals(parentName.Remove(firstIndexOfBracket))))
            {
                parent = toSearch.First(x => x.Name.Equals(parentName.Remove(firstIndexOfBracket)));
                reg = GetParent(parent.ChildRegisters, parentName, parent);
            }
            // The parent cannot be found, therefore return null
            else
            {
                return null;
            }
            if (reg != null)
            {
                return reg;
            }
            return parent;
        }

        /// <summary>
        /// This method is used to determine whether a character is a hex value
        /// </summary>
        /// <param name="c">The character</param>
        /// <returns>If it is a hex value</returns>
        private bool IsHex(char c)
        {
            return (c >= '0' && c <= '9') ||
                     (c >= 'a' && c <= 'f') ||
                     (c >= 'A' && c <= 'F');
        }
        #endregion
    }
}
