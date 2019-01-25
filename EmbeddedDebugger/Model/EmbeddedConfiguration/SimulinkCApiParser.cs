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

namespace EmbeddedDebugger.Model.EmbeddedConfiguration
{
    /// <summary>
    /// This class parses a simulink C api to an Embedded Configuration
    /// Does not work yet!
    /// At this time it only parses it to a neat string
    /// </summary>
    public static class SimulinkCApiParser
    {
        public static EmbeddedConfig ParseCApi(string fileName)
        {
            EmbeddedConfig returnable = new EmbeddedConfig();
            List<string> lines = File.ReadAllLines(fileName).ToList();
            string simulinkCoderVersion = lines.First(x => x.Contains("Simulink Coder version"));
            simulinkCoderVersion = simulinkCoderVersion.Substring(simulinkCoderVersion.IndexOf(':') + 1);
            simulinkCoderVersion = simulinkCoderVersion.Remove(simulinkCoderVersion.IndexOf('(')).Trim();
            if (Convert.ToDouble(simulinkCoderVersion) == 0)
            {
                throw new FileLoadException("Version not found!", fileName);
            }
            List<List<string>> maps = new List<List<string>>();

            foreach (string line in lines.Where(x => x.Contains("[]")))
            {
                List<string> map = new List<string>();
                for (int i = lines.IndexOf(line); i < lines.Count; i++)
                {
                    map.Add(lines[i]);
                    if (lines[i].Contains(";"))
                    {
                        break;
                    }
                }
                maps.Add(map);
            }
            List<string> dataTypeMap = maps.First(x => x[0].Contains("DataTypeMap"));
            if (dataTypeMap == null) throw new FileLoadException("No DataTypeMap found!", fileName);
            dataTypeMap = ParseMap(dataTypeMap);

            List<string> elementMap = ParseMap(maps.First(x => x[0].Contains("ElementMap")));

            List<string> blockSignalMap = ParseMap(maps.First(x => x[0].Contains("rtBlockSignals")));
            List<string> blockParameterMap = ParseMap(maps.First(x => x[0].Contains("rtBlockParameters")));
            List<string> blockStateMap = ParseMap(maps.First(x => x[0].Contains("rtBlockStates")));
            List<string> rootInputMap = ParseMap(maps.First(x => x[0].Contains("rtRootInputs")));
            List<string> rootOutputMap = ParseMap(maps.First(x => x[0].Contains("rtRootOutputs")));
            foreach (string bs in blockSignalMap)
            {
                foreach (string s in ParseToString(elementMap, dataTypeMap, "sig", bs, 5, 2, 3, 0, true))
                {
                    Console.WriteLine(s);
                }
            }
            foreach (string bs in blockParameterMap)
            {
                foreach (string s in ParseToString(elementMap, dataTypeMap, "bpar", bs, 3, 1, 2, 0, true))
                {
                    Console.WriteLine(s);
                }
            }
            foreach (string bs in blockStateMap)
            {
                foreach (string s in ParseToString(elementMap, dataTypeMap, "state", bs, 6, 2, 3, 0, true))
                {
                    Console.WriteLine(s);
                }
            }
            foreach (string bs in rootInputMap)
            {
                foreach (string s in ParseToString(elementMap, dataTypeMap, "in", bs, 5, 2, 3, 0, false))
                {
                    Console.WriteLine(s);
                }
            }
            foreach (string bs in rootOutputMap)
            {
                foreach (string s in ParseToString(elementMap, dataTypeMap, "out", bs, 5, 2, 3, 0, false))
                {
                    Console.WriteLine(s);
                }
            }
            return returnable;
        }

        private static IEnumerable<string> ParseToString(List<string> elementMap, List<string> dataTypeMap, string simulinkType, string line, int typeIndex, int name1Index, int name2Index, int addressIndex, bool nosyncprio)
        {
            string[] entries = line.Split(',');
            string[] myDataType = dataTypeMap[int.Parse(entries[typeIndex])].Split(',');
            string ctype = MakeNeat(myDataType[0]);
            if (ctype.Replace("\"", "").Equals("struct"))
            {
                string dataType = "";
                string fieldCType = "";
                string fieldName = "";
                string modelName = MakeNeat(entries[name1Index].Substring(entries[name1Index].IndexOf('"'), entries[name1Index].IndexOf('/') - entries[name1Index].IndexOf('"'))).Replace("\"", "");
                string address = MakeNeat(entries[addressIndex]);
                string blockPath = MakeNeat(entries[name1Index].Substring(entries[name1Index].IndexOf('(') + 1, entries[name1Index].IndexOf(')') - (entries[name1Index].IndexOf('(') + 2)));
                string paramName = MakeNeat(entries[name2Index].Substring(entries[name2Index].IndexOf('(') + 2, entries[name2Index].IndexOf(')') - (entries[name2Index].IndexOf('(') + 2))).Replace("\"","");
                paramName = string.IsNullOrEmpty(paramName) || paramName.Equals("\"") ? "\"" : $"/{paramName}";
                for (int i = 0; i < int.Parse(myDataType[2]); i++)
                {
                    dataType = MakeNeat(dataTypeMap[int.Parse(elementMap[int.Parse(myDataType[3]) + i - 1].Split(',')[3])].Split(',')[1]).Replace("\"", "");
                    fieldCType = MakeNeat(dataTypeMap[int.Parse(elementMap[int.Parse(myDataType[3]) + i - 1].Split(',')[3])].Split(',')[0]);
                    fieldName = MakeNeat(elementMap[int.Parse(myDataType[3]) + i - 1].Split(',')[0]).Insert(1, ".");
                    ctype = MakeNeat(dataTypeMap[int.Parse(entries[typeIndex])].Split(',')[1]);
                    string extra = nosyncprio ? ",nosync,prio=debug" : "";
                    yield return $"" +
                        $"{dataType} " +
                        $"{modelName}_{simulinkType}{address}_{i} " +
                        $"(simulink,address={address}," +
                        $"ctype={ctype},field={fieldName},fieldctype={fieldCType}," +
                        $"name={blockPath}{paramName.Remove(paramName.Length-1)}{fieldName.Substring(1)}{extra});";
                }
            }
            else
            {
                string dataType = MakeNeat(dataTypeMap[int.Parse(entries[typeIndex])].Split(',')[1]).Replace("\"", "");
                string modelName = MakeNeat(entries[name1Index].Substring(entries[name1Index].IndexOf('"'), entries[name1Index].IndexOf('/') - entries[name1Index].IndexOf('"'))).Replace("\"", "");
                string address = MakeNeat(entries[addressIndex]);

                string blockPath = MakeNeat(entries[name1Index].Substring(entries[name1Index].IndexOf('(') + 1, entries[name1Index].IndexOf(')') - (entries[name1Index].IndexOf('(') + 2)));
                string paramName = MakeNeat(entries[name2Index].Substring(entries[name2Index].IndexOf('(') + 2, entries[name2Index].IndexOf(')') - (entries[name2Index].IndexOf('(') + 2)));
                paramName = string.IsNullOrEmpty(paramName) || paramName.Equals("\"") ? "\"" : $"/{paramName}";
                string extra = nosyncprio ? ",nosync,prio=debug" : "";
                yield return $"" +
                        $"{dataType} " +
                        $"{modelName}_{simulinkType}{address} " +
                        $"(simulink,address={address}," +
                        $"ctype={ctype}," +
                        $"name={blockPath}{paramName}{extra});";
            }
        }

        public static string MakeNeat(string input)
        {
            return input.Replace("\\\"", "\"").Replace(@"\n", " ").Trim();
        }

        private static List<string> ParseMap(List<string> input)
        {
            List<string> returnable = new List<string>();
            input.RemoveAt(0);
            input.RemoveAt(input.Count - 1);
            StringBuilder builder = new StringBuilder();
            string myline;
            bool add = false;
            foreach (string line in input)
            {
                if (line.Contains('{'))
                {
                    add = true;
                }
                if (line.Contains('}'))
                {
                    add = false;
                    builder.Append(line);

                    myline = builder.ToString();
                    if (!myline.Contains("(NULL)"))
                    {
                        myline = myline.Substring(myline.IndexOf('{') + 1);
                        myline = myline.Remove(myline.IndexOf('}'));
                        myline = myline.Trim();

                        returnable.Add(myline);
                    }
                    builder.Clear();
                    continue;
                }
                if (add)
                {
                    builder.Append(line);
                }
            }
            return returnable;
        }
    }
}
