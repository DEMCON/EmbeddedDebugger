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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EmbeddedDebugger.DebugProtocol.Enums;

namespace EmbeddedDebugger.DebugProtocol.RegisterValues
{
    public class RegisterValueString : RegisterValue
    {
        private string stringValue;
        public override object Value { get => stringValue; }
        public override byte[] ValueByteArray
        {
            get => Encoding.UTF8.GetBytes(stringValue);
            set
            {
                if (value == null) return;
                int lastIndex = Array.FindLastIndex(value, b => b != 0);

                Array.Resize(ref value, lastIndex + 1);

                stringValue = Encoding.UTF8.GetString(value);
            }
        }
        public override bool ValueByteArrayFromObject(object x, out byte[] output)
        {
            output = null;
            if (x == null) return false;
            output = Encoding.UTF8.GetBytes(x.ToString());
            return true;
        }
        public override bool ValueByteArrayFromString(string x, out byte[] output, ValueDisplayFormat vdf = ValueDisplayFormat.Default)
        {
            if (vdf == ValueDisplayFormat.Default)
            {
                output = null;
                if (x == null) return false;
                output = Encoding.UTF8.GetBytes(x);
                return true;
            }else
            {
                return base.ValueByteArrayFromString(x, out output, vdf);
            }
        }

        //public override string ValueAsFormattedString(ValueDisplayFormat valueDisplayFormat = ValueDisplayFormat.Dec)
        //{
        //    return stringValue;
        //}
    }
}
