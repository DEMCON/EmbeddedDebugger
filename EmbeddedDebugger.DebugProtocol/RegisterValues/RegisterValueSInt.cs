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
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace EmbeddedDebugger.DebugProtocol.RegisterValues
{
    /// <summary>
    /// Extends the RegisterValue class, specifically created for a signed integer value
    /// </summary>
    public class RegisterValueSInt : RegisterValue
    {
        private int intValue;
        public override int ArraySize => sizeof(int);
        public override object Value { get => intValue; }
        public override byte[] ValueByteArray
        {
            get => BitConverter.GetBytes(intValue);
            set
            {
                if (value == null) return;
                intValue = BitConverter.ToInt32(ApplyPadding(value), 0);
            }
        }
        public override bool ValueByteArrayFromObject(object x, out byte[] output)
        {
            output = null;
            if (!(x is int)) return false;
            intValue = (int)x;
            output = ValueByteArray;
            return true;
        }
        public override bool ValueByteArrayFromString(string x, out byte[] output, ValueDisplayFormat vdf = ValueDisplayFormat.Default)
        {
            BigInteger value;
            if (ValueFromString(x, out value))
            {
                intValue = (int)value;
                output = ValueByteArray;
                return true;
            }
            else
            {
                output = null;
                return false;
            }
        }

        //public override string ValueAsFormattedString(ValueDisplayFormat valueDisplayFormat = ValueDisplayFormat.Dec)
        //{
        //    switch (valueDisplayFormat)
        //    {
        //        case ValueDisplayFormat.Dec:
        //            return String.Format("{0:D10}", (int)Value);
        //        case ValueDisplayFormat.Bin:
        //            //string bin = Convert.ToString((int)Value, 2).ToString();
        //            //while (bin.Length < 32) bin = bin.Insert(0, "0");
        //            //bin = bin.Insert(28, " ");
        //            //bin = bin.Insert(24, " ");
        //            //bin = bin.Insert(20, " ");
        //            //bin = bin.Insert(16, " ");
        //            //bin = bin.Insert(12, " ");
        //            //bin = bin.Insert(8, " ");
        //            //bin = bin.Insert(4, " ");
        //            //return bin;
        //            return ByteArrayToBinaryString(ValueByteArray);
        //        case ValueDisplayFormat.Hex:
        //            return String.Format("{0:X8}", (int)Value);
        //        case ValueDisplayFormat.Char:
        //            return ((char)(int)Value).ToString();
        //        case ValueDisplayFormat.UTF8:
        //            return Encoding.UTF8.GetString(ValueByteArray);
        //    }
        //    return Value.ToString();
        //}
    }
}
