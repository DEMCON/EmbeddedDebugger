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
using EmbeddedDebugger.DebugProtocol.RegisterValues;
using EmbeddedDebugger.Properties;
using OxyPlot;
using OxyPlot.Series;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace EmbeddedDebugger.Model
{
    public class Register : INotifyPropertyChanged
    {
        #region Fields
        private RegisterValue registerValue;
        private List<Register> childRegisters;
        private Register parent;
        private uint id;
        private string name;
        private string fullName;
        private ReadWrite readWrite;
        private VariableType variableType;
        private string variableTypeName;
        private bool plot;
        private LineSeries myLine;
        private PlotModel plotModel;
        private int maxNumberOfValues;
        private double numberOfSeconds;
        private int size;
        private uint offset;
        private int derefDepth;
        private bool? show;
        private Source source;
        private int timeStampUnits = 1;
        private byte? debugChannel;
        private ChannelMode channelMode;
        private bool log;
        private Color lineColor;
        private CpuNode cpuNode;
        private ValueDisplayFormat valueDisplayFormat;
        private bool isCollapsed = true;
        #endregion

        #region Properties
        public RegisterValue RegisterValue { get => registerValue; set => registerValue = value; }
        public string Value
        {
            get
            {
                string s = registerValue != null ? registerValue.ValueAsFormattedString(valueDisplayFormat) : "";
                if (!enableValueUpdates)
                    s = s.Trim();
                return s;
            }
            set
            {
                RegisterValue regValue = RegisterValue.GetRegisterValueByVariableType(variableType);
                try
                {
                    if (regValue.ValueByteArrayFromString(value, out byte[] output, valueDisplayFormat))
                    {
                        regValue.ValueByteArray = output;
                        registerValue = regValue;
                        ValueChanged(this, regValue);
                        if (EnableValueUpdates)
                            PropertyChanged(this, new PropertyChangedEventArgs("Value"));
                    }
                }
                catch (Exception e)
                {
                    MessageBox.Show($"Error: {e.Message}");
                }
            }
        }
        public string ValuePlain { get { return this.Value.Trim(); } }
        public List<Register> ChildRegisters { get => childRegisters; set => childRegisters = value; }
        public bool HasChildren { get => childRegisters.Count > 0; }
        public Register Parent { get => parent; set => parent = value; }
        [DisplayName("ID")]
        public uint ID { get => id; set => id = value; }
        [DisplayName("Name")]
        public string Name { get => name ?? fullName; set => name = value; }
        public string FullName { get => fullName ?? name; set => fullName = value; }
        public ReadWrite ReadWrite
        {
            get => readWrite;
            set
            {
                readWrite = value;
                PropertyChanged(this, new PropertyChangedEventArgs("ReadWrite"));
            }
        }

        private bool enableValueUpdates = true;
        public bool EnableValueUpdates
        {
            get => enableValueUpdates;
            set
            {
                bool doPropertyChanged = value != enableValueUpdates;
                enableValueUpdates = value;
                if (doPropertyChanged)
                    PropertyChanged(this, new PropertyChangedEventArgs("Value"));
            }
        }
        public VariableType VariableType { get => variableType; set => variableType = value; }
        public string VariableTypeString
        {
            get
            {
                if (variableType == VariableType.Unknown)
                {
                    return $"\"{variableTypeName}\"";
                }
                if (Settings.Default.CPPVariableTypes)
                {
                    return GetCPPVariableTypes(variableType);
                }
                else
                {
                    return variableType.ToString().ToLower();
                }

            }
        }
        public string VariableTypeName
        {
            get
            {
                if (variableType == VariableType.Unknown)
                {
                    return variableTypeName;
                }
                return variableType.ToString().ToLower();
            }
            set
            {
                variableTypeName = value;
            }
        }
        public bool Plot
        {
            get => plot;
            set
            {
                plot = value;
                if (value)
                {
                    if (lineColor.IsEmpty)
                    {
                        KnownColor[] colors = GetLineColors().ToArray();
                        lineColor = Color.FromKnownColor(colors[new Random(Guid.NewGuid().GetHashCode()).Next(0, colors.Length - 1)]);
                        PropertyChanged(this, new PropertyChangedEventArgs("LineColor"));
                        PropertyChanged(this, new PropertyChangedEventArgs("LineKnownColor"));
                    }
                    myLine = new LineSeries
                    {
                        Title = fullName,
                        Color = OxyColor.FromArgb(lineColor.A, lineColor.R, lineColor.G, lineColor.B)
                    };
                }
                PropertyChanged(this, new PropertyChangedEventArgs("Plot"));
            }
        }

        public KnownColor LineKnownColor
        {
            get
            {
                return LineColor.ToKnownColor();
            }
            set
            {
                LineColor = Color.FromKnownColor(value);
                PropertyChanged(this, new PropertyChangedEventArgs("LineColor"));
                PropertyChanged(this, new PropertyChangedEventArgs("LineKnownColor"));
            }
        }

        public Color LineColor
        {
            get => lineColor.IsEmpty ? Color.Red : lineColor;
            set
            {
                lineColor = value;
                if (myLine == null)
                {
                    return;
                }
                myLine.Color = OxyColor.FromArgb(value.A, value.R, value.G, value.B);
                PropertyChanged(this, new PropertyChangedEventArgs("LineColor"));
                PropertyChanged(this, new PropertyChangedEventArgs("LineKnownColor"));
            }
        }
        public LineSeries Line { get => myLine; }
        private object plotModelLock = new object();
        public PlotModel PlotModel { get => plotModel; set { lock (plotModelLock) { plotModel = value; } } }
        public int MaxNumberOfValues { get => maxNumberOfValues; set => maxNumberOfValues = value; }
        public double NumberOfSeconds { get => numberOfSeconds; set => numberOfSeconds = value; }
        public int Size { get => size; set => size = value; }
        public uint Offset { get => offset; set => offset = value; }
        public int DerefDepth { get => derefDepth; set => derefDepth = value; }
        public bool? Show
        {
            get => show;
            set
            {
                show = value;
                PropertyChanged(this, new PropertyChangedEventArgs("Show"));
            }
        }
        public Source Source
        {
            get => source;
            set
            {
                source = value;
                PropertyChanged(this, new PropertyChangedEventArgs("Source"));
            }
        }
        public char ReadWriteChar
        {
            get => readWrite == ReadWrite.Read ? '←' : '→';
        }
        public int TimeStampUnits { get => timeStampUnits; set => timeStampUnits = value; }
        public bool IsDebugChannel { get => debugChannel.HasValue; }
        public byte? DebugChannel { get => debugChannel; set => debugChannel = value; }
        public ChannelMode ChannelMode
        {
            get => channelMode;
            set
            {
                try
                {
                    channelMode = value;
                    ChannelModeUpdated(this, new EventArgs());
                }
                catch(ArgumentException)
                {
                    channelMode = ChannelMode.Off;
                    try
                    {
                        ChannelModeUpdated(this, new EventArgs());
                    }
                    catch (ArgumentException) { }
                }
                PropertyChanged(this, new PropertyChangedEventArgs("ChannelMode"));
            }
        }
        public bool Log
        {
            get => log;
            set
            {
                log = value;
                LoggingChanged(this, new EventArgs());
                PropertyChanged(this, new PropertyChangedEventArgs("Log"));
            }
        }
        public CpuNode CpuNode { get => cpuNode; set => cpuNode = value; }
        public byte CpuID { get => cpuNode.ID; }
        public bool IsReadable { get => ((byte)readWrite & 0b0000_0010) >> 1 == 1; }
        public bool IsWritable { get => ((byte)readWrite & 0b0000_0001) == 1; }
        public ValueDisplayFormat ValueDisplayFormat
        {
            get => valueDisplayFormat;
            set
            {
                valueDisplayFormat = value;
                if(EnableValueUpdates)
                    PropertyChanged(this, new PropertyChangedEventArgs("Value"));
            }
        }
        public bool IsIntegralValue
        {
            get
            {
                switch (variableType)
                {
                    case VariableType.Bool:
                    case VariableType.Char:
                    case VariableType.Short:
                    case VariableType.Int:
                    case VariableType.Long:
                    case VariableType.Float:
                    case VariableType.Double:
                    case VariableType.LongDouble:
                    case VariableType.SChar:
                    case VariableType.UChar:
                    case VariableType.UShort:
                    case VariableType.UInt:
                    case VariableType.ULong:
                        return true;
                }
                return false;
            }
        }

        public bool IsVariableSize
        {
            get
            {
                switch (variableType)
                {
                    case VariableType.String:
                    case VariableType.Blob:
                        return true;
                }
                return false;
            }
        }

        public bool IsCollapsed { get => isCollapsed; set => isCollapsed = value; }
        #endregion

        #region Events
        public event EventHandler NewValueAdded = delegate { };
        public event EventHandler QueryValue = delegate { };
        public event EventHandler ChannelModeUpdated = delegate { };
        public event EventHandler<RegisterValue> ValueChanged = delegate { };
        public event EventHandler LoggingChanged = delegate { };
        public event PropertyChangedEventHandler PropertyChanged = delegate { };
        #endregion

        #region Constructors
        public Register()
        {
            childRegisters = new List<Register>();
        }
        #endregion

        public static IEnumerable<KnownColor> GetLineColors()
        {
            yield return KnownColor.Black;
            yield return KnownColor.Blue;
            yield return KnownColor.BlueViolet;
            yield return KnownColor.Brown;
            yield return KnownColor.CadetBlue;
            yield return KnownColor.Chartreuse;
            yield return KnownColor.Chocolate;
            yield return KnownColor.CornflowerBlue;
            yield return KnownColor.Crimson;
            yield return KnownColor.DarkBlue;
            yield return KnownColor.DarkGreen;
            yield return KnownColor.DarkMagenta;
            yield return KnownColor.DarkRed;
            yield return KnownColor.DarkViolet;
            yield return KnownColor.ForestGreen;
            yield return KnownColor.Green;
            yield return KnownColor.Indigo;
            yield return KnownColor.MediumBlue;
            yield return KnownColor.Navy;
            yield return KnownColor.Purple;
            yield return KnownColor.Red;
        }
        /// <summary>
        /// Add a value to the register, also adds it to the plotmodel, this is used for plotting
        /// TODO: check if there is a more elegant way.
        /// </summary>
        /// <param name="regValue">The value which is to be added to the Register</param>
        public void AddValue(RegisterValue regValue)
        {
            if (regValue.GetType() == typeof(RegisterValue))
            {
                regValue = RegisterValue.GetRegisterValueByVariableType(variableType, regValue.ValueByteArray, regValue.TimeStamp);
            }
            registerValue = regValue;
            if (plot && plotModel != null && regValue.TimeStamp.HasValue)
            {
                double time = (double)regValue.TimeStamp / (uint)timeStampUnits;
                double theValue;
                try
                {
                    theValue = Convert.ToDouble(regValue.Value);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    Plot = false;
                    return;
                }
                lock (plotModel.SyncRoot)
                {
                    myLine.Points.Add(new DataPoint(time, theValue));
                    if (EnablePlotUpdate)
                    {
                        if (myLine.Points.Last().X < myLine.Points[0].X)
                        {
                            myLine.Points.RemoveRange(0, myLine.Points.Count - 2);
                        }
                        if (myLine.Points[0].X < myLine.Points.Last().X - numberOfSeconds)
                        {
                            DataPoint dp = myLine.Points.First(x => x.X >= myLine.Points.Last().X - numberOfSeconds);
                            myLine.Points.RemoveRange(0, myLine.Points.IndexOf(dp));
                        }
                    }
                }
                UpdatePlot();

            }
            NewValueAdded(this, new EventArgs());
            if (EnableValueUpdates)
                PropertyChanged(this, new PropertyChangedEventArgs("Value"));
        }

        private static bool updatePlotRequest = false;
        public static bool EnablePlotUpdate = true;
        private async void UpdatePlot()
        {
            if (updatePlotRequest)
                return;

            updatePlotRequest = true;
            await Task.Delay(100);
            updatePlotRequest = false;

            lock (plotModelLock)
            {
                if(plotModel != null && EnablePlotUpdate)
                    plotModel.InvalidatePlot(true);
            }
        }

        public void RequestNewValue()
        {
            QueryValue(this, new EventArgs());
        }

        public string GetCPPVariableTypes(VariableType varType)
        {
            switch (varType)
            {
                case VariableType.Bool:
                    return "boolean";
                case VariableType.SChar:
                    return "int8_t";
                case VariableType.Short:
                    return "int16_t";
                case VariableType.Int:
                    return "int32_t";
                case VariableType.Long:
                    return "int64_t";
                case VariableType.UChar:
                    return "uint8_t";
                case VariableType.UShort:
                    return "uint16_t";
                case VariableType.UInt:
                    return "uint32_t";
                case VariableType.ULong:
                    return "uint64_t";
            }
            return varType.ToString().ToLower();
        }

        /// <summary>
        /// This method copies all values of the old register to a new and standalone register
        /// </summary>
        /// <param name="old">The Register to be copied/cloned</param>
        /// <returns>The new Register</returns>
        public static Register GetCopy(Register old)
        {
            return new Register()
            {
                ChildRegisters = old.ChildRegisters.ToList(),
                Parent = old.Parent == null ? null : GetCopy(old.Parent),
                ID = Convert.ToUInt32(old.ID),
                Name = old.Name.ToString(),
                FullName = old.FullName.ToString(),
                ReadWrite = (ReadWrite)Enum.Parse(typeof(ReadWrite), old.ReadWrite.ToString()),
                VariableType = (VariableType)Enum.Parse(typeof(VariableType), old.VariableType.ToString()),
                VariableTypeName = old.VariableTypeName.ToString(),
                Plot = Convert.ToBoolean(old.Plot),
                MaxNumberOfValues = Convert.ToInt32(old.MaxNumberOfValues),
                NumberOfSeconds = Convert.ToDouble(old.NumberOfSeconds),
                Size = Convert.ToInt32(old.Size),
                Offset = Convert.ToUInt32(old.Offset),
                DerefDepth = Convert.ToInt32(old.DerefDepth),
                Show = Convert.ToBoolean(old.Show),
                Source = (Source)Enum.Parse(typeof(Source), old.Source.ToString()),
                TimeStampUnits = Convert.ToInt32(old.TimeStampUnits),
                DebugChannel = null,
                Log = Convert.ToBoolean(old.Plot),
                CpuNode = old.CpuNode,
            };
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Register)) return false;
            Register reg = (Register)obj;
            return
                reg.ID == ID &&
                reg.FullName == FullName &&
                reg.ReadWrite == ReadWrite &&
                reg.VariableType == VariableType &&
                reg.Size == Size &&
                reg.Offset == Offset &&
                reg.DerefDepth == DerefDepth &&
                reg.Source == Source &&
                reg.CpuNode == CpuNode;
        }

        public override int GetHashCode()
        {
            var hashCode = 1671609247;
            hashCode = hashCode * -1521134295 + EqualityComparer<List<Register>>.Default.GetHashCode(childRegisters);
            hashCode = hashCode * -1521134295 + EqualityComparer<Register>.Default.GetHashCode(parent);
            hashCode = hashCode * -1521134295 + id.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(name);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(fullName);
            hashCode = hashCode * -1521134295 + readWrite.GetHashCode();
            hashCode = hashCode * -1521134295 + variableType.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(variableTypeName);
            hashCode = hashCode * -1521134295 + plot.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<LineSeries>.Default.GetHashCode(myLine);
            hashCode = hashCode * -1521134295 + EqualityComparer<PlotModel>.Default.GetHashCode(plotModel);
            hashCode = hashCode * -1521134295 + maxNumberOfValues.GetHashCode();
            hashCode = hashCode * -1521134295 + numberOfSeconds.GetHashCode();
            hashCode = hashCode * -1521134295 + size.GetHashCode();
            hashCode = hashCode * -1521134295 + offset.GetHashCode();
            hashCode = hashCode * -1521134295 + derefDepth.GetHashCode();
            hashCode = hashCode * -1521134295 + show.GetHashCode();
            hashCode = hashCode * -1521134295 + source.GetHashCode();
            hashCode = hashCode * -1521134295 + timeStampUnits.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<byte?>.Default.GetHashCode(debugChannel);
            hashCode = hashCode * -1521134295 + channelMode.GetHashCode();
            hashCode = hashCode * -1521134295 + log.GetHashCode();
            return hashCode;
        }
    }
}
