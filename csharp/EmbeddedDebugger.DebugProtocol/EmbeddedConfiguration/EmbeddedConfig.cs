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
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace EmbeddedDebugger.DebugProtocol.EmbeddedConfiguration
{
    /// <summary>
    /// This class is used to store the configuration of an embedded system
    /// </summary>
    public class EmbeddedConfig
    {
        #region Properties
        private List<Register> registers;
        private Dictionary<uint, Register> registerMap = new Dictionary<uint, Register>();
        public IList<Register> Registers {
            get
            {
                //return registers.AsReadOnly();
                return registers;
            }
        }

        public int RegisterCount { get => CountRegisters(registers); }

        private CpuNode cpu;
        public CpuNode Cpu { get => cpu; set => cpu = value; }

        private string cpuName;
        public string CpuName { get => cpuName; set => cpuName = value; }

        private string protocolVersion;
        public string ProtocolVersion { get => protocolVersion; set => protocolVersion = value; }

        private string applicationVersion;
        public string ApplicationVersion { get => applicationVersion; set => applicationVersion = value; }

        private uint numberOfRegistersExpected;
        public uint NumberOfRegistersExpected { get => numberOfRegistersExpected; set => numberOfRegistersExpected = value; }

        private bool configCompletelyLoaded;
        public bool ConfigCompletelyLoaded { get => configCompletelyLoaded; set => configCompletelyLoaded = value; }
        #endregion

        public EmbeddedConfig()
        {
            registers = new List<Register>();
        }

        private int CountRegisters(List<Register> registers)
        {
            int count = 0;
            foreach(Register reg in registers)
            {
                count++;
                count += CountRegisters(reg.ChildRegisters);
            }
            return count;
        }

        public Register GetRegister(uint offset, ReadWrite readWrite)
        {
            Register reg;
            if (registerMap.TryGetValue(offset, out reg) && reg.Offset == offset)
                return reg.ReadWrite == readWrite ? reg : null;

            // not found, regenerate map
            registerMap.Clear();
            foreach (Register r in registers)
            {
                if (r.HasChildren)
                {
                    foreach (Register c in r.ChildRegisters)
                        registerMap.Add(c.Offset, c);
                }
                else
                {
                    registerMap.Add(r.Offset, r);
                }
            }

            // try again
            if (registerMap.TryGetValue(offset, out reg) && reg.Offset == offset && reg.ReadWrite == readWrite)
                return reg;
            else
                return null;
        }

        public void SetRegisters(IList<Register> regs = null)
        {
            if (regs == null)
                registers.Clear();
            else
                registers = regs.ToList();

            registerMap.Clear();
        }
        public void AddRegisters(IList<Register> regs)
        {
            foreach (Register r in regs)
                AddRegister(r);
        }
        public void AddRegister(Register r)
        {
            registers.Add(r);
        }
    }
}
