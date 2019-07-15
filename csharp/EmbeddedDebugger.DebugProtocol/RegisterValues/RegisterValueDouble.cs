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
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmbeddedDebugger.DebugProtocol.RegisterValues
{
    /// <summary>
    /// Extends the RegisterValue class, specifically created for a double value
    /// </summary>
    public class RegisterValueDouble : RegisterValue
    {
        private double doubleValue;
        public override int ArraySize => sizeof(double);
        public override object Value { get => doubleValue; }
        public override byte[] ValueByteArray
        {
            get => BitConverter.GetBytes(doubleValue);
            set
            {
                if (value == null) return;
                doubleValue = BitConverter.ToDouble(ApplyPadding(value), 0);
            }
        }
        public override bool ValueByteArrayFromObject(object x, out byte[] output)
        {
            output = null;
            if (!(x is double)) return false;
            doubleValue = (double)x;
            output = ValueByteArray;
            return true;
        }
        public override bool ValueByteArrayFromString(string x, out byte[] output, ValueDisplayFormat vdf = ValueDisplayFormat.Default)
        {
            double value;
            if (ValueFromString(x, out value))
            {
                doubleValue = value;
                output = ValueByteArray;
                return true;
            }
            else
            {
                output = null;
                return false;
            }
        }
    }
}
