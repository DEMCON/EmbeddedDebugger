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
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace EmbeddedDebugger.DebugProtocol.RegisterValues
{
    /// <summary>
    /// This class is used to store a value and a timestamp for a Register
    /// </summary>
    public class RegisterValue
    {
        #region fields
        private uint? timeStamp;
        #endregion

        #region Properties
        public virtual int ArraySize { get; }
        public virtual object Value { get; }
        public virtual byte[] ValueByteArray { get; set; }
        public uint? TimeStamp { get => timeStamp; set => timeStamp = value; }
        #endregion

        public virtual bool ValueByteArrayFromObject(object x, out byte[] output)
        {
            output = null;
            return false;
        }

        protected static string PreformatStringValue(string x)
        {
            // strip internal spaces and _
            x = Regex.Replace(x, @"[ _]", "");
            // replace decimal , by . for InvariantCulture parsing
            x = Regex.Replace(x, @",", ".");
            // make hex/bin prefix lower case
            x = Regex.Replace(x, @"^0X", "0x");
            x = Regex.Replace(x, @"^0B", "0b");
            return x;
        }

        protected static string PostformatStringValue(string x)
        {
            const int afterCommaWidth = 20;

            if (x.Contains('.') || x.Contains(','))
                return x.PadRight(afterCommaWidth + x.LastIndexOfAny(new[] { '.', ',' }), ' ');
            else
                return x.PadRight(x.Length + afterCommaWidth, ' ');
        }

        public static bool ValueFromString(string x, out BigInteger output)
        {
            x = PreformatStringValue(x);
            try
            {
                if (x.StartsWith("0x"))
                {
                    bool res = BigInteger.TryParse(x.Substring(2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out output);
                    if (res && output < 0)
                        output += 0x100000000u;
                    return res;
                }
                else if (Regex.IsMatch(x,@"^0b[01]*$"))
                {
                    output = 0;
                    while (x.Length > 0)
                    {
                        output *= 2;
                        char nibble = x.ElementAt(0);
                        if (nibble == '1')
                            output += 1;
                        x = x.Substring(1);
                    }
                    return true;
                }
                else
                {
                    return BigInteger.TryParse(x.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out output);
                }
            }
            catch
            {
                output = 0;
                return false;
            }
        }
        public static bool ValueFromString(string x, out double output)
        {
            x = PreformatStringValue(x);
            try
            {
                if (x.StartsWith("0x") || x.StartsWith("0b"))
                {
                    BigInteger value;
                    if (!ValueFromString(x, out value))
                    {
                        output = double.NaN;
                        return false;
                    }
                    else
                    {
                        output = (double)value;
                        return true;
                    }
                }
                else
                    return double.TryParse(x, NumberStyles.Any, CultureInfo.InvariantCulture, out output);
            }
            catch
            {
                output = double.NaN;
                return false;
            }
        }

        public virtual bool ValueByteArrayFromString(string x, out byte[] output, ValueDisplayFormat valueDisplayFormat = ValueDisplayFormat.Default)
        {
            switch (valueDisplayFormat)
            {
                case ValueDisplayFormat.Bin:
                    output = BinaryStringToByteArray(x);
                    return true;
                case ValueDisplayFormat.Dec:
                    output = DecStringToByteArray(x);
                    return true;
                case ValueDisplayFormat.Hex:
                    output = HexStringToByteArray(x);
                    return true;
                case ValueDisplayFormat.Char:
                    output = Encoding.ASCII.GetBytes(x);
                    return true;
                case ValueDisplayFormat.UTF8:
                    output = Encoding.UTF8.GetBytes(x);
                    return true;
            }
            output = null;
            return false;
        }

        public string ValueAsFormattedString(ValueDisplayFormat valueDisplayFormat = ValueDisplayFormat.Default)
        {
            string s;
            switch (valueDisplayFormat)
            {
                case ValueDisplayFormat.Bin:
                    s = ByteArrayToBinaryString(ValueByteArray);
                    break;
                case ValueDisplayFormat.Dec:
                    s = ByteArrayToDecString(ValueByteArray);
                    break;
                case ValueDisplayFormat.Hex:
                    s = ByteArrayToHexString(ValueByteArray);
                    break;
                case ValueDisplayFormat.Char:
                    s = Encoding.ASCII.GetString(ValueByteArray);
                    break;
                case ValueDisplayFormat.UTF8:
                    s = Encoding.UTF8.GetString(ValueByteArray);
                    break;
                default:
                    s = Value == null ? "" : Value.ToString();
                    break;
            }
            return PostformatStringValue(s);
        }

        public static RegisterValue GetRegisterValueByVariableType(VariableType varType, byte[] valueArray = null, uint? timeStamp = null)
        {
            switch (varType)
            {
                case VariableType.Bool:
                    return new RegisterValueBool() { ValueByteArray = valueArray, TimeStamp = timeStamp };
                case VariableType.Char:
                    return new RegisterValueChar() { ValueByteArray = valueArray, TimeStamp = timeStamp };
                case VariableType.Double:
                    return new RegisterValueDouble() { ValueByteArray = valueArray, TimeStamp = timeStamp };
                case VariableType.Float:
                    return new RegisterValueFloat() { ValueByteArray = valueArray, TimeStamp = timeStamp };
                case VariableType.Int:
                    return new RegisterValueSInt() { ValueByteArray = valueArray, TimeStamp = timeStamp };
                case VariableType.Long:
                    return new RegisterValueSLong() { ValueByteArray = valueArray, TimeStamp = timeStamp };
                case VariableType.LongDouble:
                    return new RegisterValueSLong() { ValueByteArray = valueArray, TimeStamp = timeStamp };
                case VariableType.SChar:
                    return new RegisterValueSChar() { ValueByteArray = valueArray, TimeStamp = timeStamp };
                case VariableType.Short:
                    return new RegisterValueSShort() { ValueByteArray = valueArray, TimeStamp = timeStamp };
                case VariableType.UChar:
                    return new RegisterValueUChar() { ValueByteArray = valueArray, TimeStamp = timeStamp };
                case VariableType.UInt:
                    return new RegisterValueUInt() { ValueByteArray = valueArray, TimeStamp = timeStamp };
                case VariableType.ULong:
                    return new RegisterValueULong() { ValueByteArray = valueArray, TimeStamp = timeStamp };
                case VariableType.UShort:
                    return new RegisterValueUShort() { ValueByteArray = valueArray, TimeStamp = timeStamp };
                case VariableType.String:
                    return new RegisterValueString() { ValueByteArray = valueArray, TimeStamp = timeStamp };
                case VariableType.Blob:
                    return new RegisterValueBlob() { ValueByteArray = valueArray, TimeStamp = timeStamp };
            }
            return new RegisterValue() { ValueByteArray = valueArray, TimeStamp = timeStamp };
        }

        protected static string ByteArrayToBinaryString(byte[] valueArray)
        {
            if (valueArray.Length == 0)
            {
                return "0b0";
            }
            else
            {
                StringBuilder stringBuilder = new StringBuilder();
                foreach (byte b in valueArray)
                {
                    stringBuilder.Insert(0, " ");
                    stringBuilder.Insert(0, ByteToString(b, 2));
                }
                stringBuilder.Remove(stringBuilder.Length - 1, 1);
                return "0b" + stringBuilder.ToString().TrimStart(new[] { '0', ' ' });
            }
        }
        protected static string ByteArrayToDecString(byte[] valueArray)
        {
            return new BigInteger(valueArray).ToString();
        }

        protected static string ByteArrayToHexString(byte[] valueArray)
        {
            if (valueArray.Length == 0)
            {
                return "0x0";
            }
            else
            {
                StringBuilder stringBuilder = new StringBuilder();
                foreach (byte b in valueArray)
                {
                    stringBuilder.Insert(0, ByteToString(b, 16));
                }
                stringBuilder.Insert(0, "0x");
                return stringBuilder.ToString();
            }
        }

        protected static byte[] BinaryStringToByteArray(string bin)
        {
            bin = bin.Replace(" ", "").Replace("-", "");
            if (bin.Length % 8 > 0) throw new ArgumentException("Binary string number of characters not supported");
            byte[] bytes = new byte[bin.Length / 8];
            for (int i = 0; i < bytes.Length; i++)
            {
                bytes[bytes.Length - 1 - i] = Convert.ToByte(bin.Substring(8 * i, 8), 2);
            }
            return bytes;
        }

        protected static byte[] DecStringToByteArray(string bin)
        {
            bin = bin.Replace(" ", "").Replace("-", "");
            if (!BigInteger.TryParse(bin, out BigInteger result)) throw new ArgumentException("Decimal number not supported");
            return result.ToByteArray();
        }

        protected static byte[] HexStringToByteArray(string bin)
        {
            bin = bin.Replace(" ", "").Replace("-", "");
            if (bin.Length % 2 > 0) bin = "0" + bin;
            byte[] bytes = new byte[bin.Length / 2];
            for (int i = 0; i < bytes.Length; i++)
            {
                bytes[bytes.Length - 1 - i] = Convert.ToByte(bin.Substring(2 * i, 2), 16);
            }
            return bytes;
        }

        protected byte[] ApplyPadding(byte[] myArray)
        {
            if (ArraySize == 0 || myArray.Length >= ArraySize) return myArray;
            List<byte> byteList = myArray.ToList();
            byteList.AddRange(new byte[ArraySize - myArray.Length]);
            return byteList.ToArray();
        }

        public static string ByteToString(byte byteValue, int baseNumber)
        {
            switch (baseNumber)
            {
                case 2:
                    return ByteToBinString(byteValue);
                case 16:
                    return ByteToHexString(byteValue);
            }
            throw new ArgumentException($"BaseNumber: {baseNumber} is not supported");
        }
        private static string ByteToBinString(byte byteValue)
        {
            StringBuilder stringBuilder = new StringBuilder();
            for (int i = 7; i >= 0; i--)
            {
                stringBuilder.Append((byteValue >> i & 0b0000_0001) == 1 ? "1" : "0");
            }
            stringBuilder.Insert(4, " ");
            return stringBuilder.ToString();
        }

        private static string ByteToHexString(byte byteValue)
        {
            return byteValue.ToString("X2");
        }
    }
}