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
    /// Extends the RegisterValue class, specifically created for a signed char value
    /// </summary>
    public class RegisterValueSChar : RegisterValue
    {
        private sbyte sByteValue;
        public override object Value { get => sByteValue; }
        public override byte[] ValueByteArray
        {
            // There is no overload for GetBytes for the sbyte, therefore it is cast to a short
            // For this reason, the new byte[] is required!
            get => new byte[] { BitConverter.GetBytes(sByteValue)[0] };
            set
            {
                if (value == null) return;
                sByteValue = (sbyte)value[0];
            }
        }
        public override bool ValueByteArrayFromObject(object x, out byte[] output)
        {
            output = null;
            if (!(x is sbyte)) return false;
            sByteValue = (sbyte)x;
            output = ValueByteArray;
            return true;
        }
        public override bool ValueByteArrayFromString(string x, out byte[] output, ValueDisplayFormat vdf = ValueDisplayFormat.Default)
        {
            if (vdf == ValueDisplayFormat.Default)
            {
                output = null;
                if (!sbyte.TryParse(x, out sbyte result)) return false;
                sByteValue = result;
                output = ValueByteArray;
                return true;
            }
            else
            {
                return base.ValueByteArrayFromString(x, out output, vdf);
            }
        }

        //public override string ValueAsFormattedString(ValueDisplayFormat valueDisplayFormat = ValueDisplayFormat.Dec)
        //{
        //    switch (valueDisplayFormat)
        //    {
        //        case ValueDisplayFormat.Dec:
        //            return String.Format("{0:D3}", (sbyte)Value);
        //        case ValueDisplayFormat.Bin:
        //            string bin = Convert.ToString((sbyte)Value, 2).ToString();
        //            if (bin.Length > 8) bin = bin.Substring(bin.Length - 8);
        //            while (bin.Length < 8) bin = bin.Insert(0, "0");
        //            bin = bin.Insert(4, " ");
        //            return bin;
        //        case ValueDisplayFormat.Hex:
        //            return String.Format("{0:X2}", (sbyte)Value);
        //        case ValueDisplayFormat.Char:
        //            return ((char)(sbyte)Value).ToString();
        //        case ValueDisplayFormat.UTF8:
        //            return Encoding.UTF8.GetString(ValueByteArray);
        //            // If oct is ever needed: implement this
        //            //case ValueDisplayFormat.Oct:
        //            //    string oct = Convert.ToString((sbyte)Value, 8);
        //            //    if ((sbyte)Value < 0)
        //            //    {
        //            //        oct = (-178 + (Int32.Parse(oct) - 177600)).ToString("000");
        //            //    }
        //            //    else
        //            //    {
        //            //        oct = (Int32.Parse(oct)).ToString("000");
        //            //    }
        //            //    return oct;
        //    }
        //    return Value.ToString();
        //}
    }
}
