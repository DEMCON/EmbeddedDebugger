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
using EmbeddedDebugger.Model;
using EmbeddedDebugger.ViewModel;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using EmbeddedDebugger.DebugProtocol;

namespace EmbeddedDebugger.View.UserControls
{
    /// <summary>
    /// Interaction logic for PlotUserControl.xaml
    /// </summary>
    public partial class PlotUserControl : UserControl
    {
        private PlotModel plotModel;
        private bool paused;
        private bool DisplayMinMaxOn = true;
        private bool showSettings = true;
        private SystemViewModel systemViewModel;
        private PlottingViewModel plottingViewModel;

        private DateTimeAxis xAxis = new DateTimeAxis();
        private LinearAxis yAxis = new LinearAxis();

        private Dictionary<Register, LineSeries> plotSeries;

        private List<Register> plotRegisters;
        public List<Register> PlotRegisters { get => this.plottingViewModel.RegistersToPlot; }

        public double[] PlotXMinMax { get => this.plottingViewModel.xAxisMinMax; }
        public PlotUserControl()
        {
            InitializeComponent();
            plotRegisters = new List<Register>();
            plotModel = new PlotModel { Title = "" };
            Plot.Model = plotModel;
            YAutoScaleCheckBox.IsChecked = true;
            this.plotSeries = new Dictionary<Register, LineSeries>();

            #region set axis to current time
            DateTime dateTimeNow = DateTime.Now;
            var dateTimeOffset = new DateTimeOffset(dateTimeNow);
            var unixDateTime = dateTimeOffset.ToUnixTimeMilliseconds();
            var unixDateTime2 = dateTimeOffset.ToUnixTimeSeconds() + 10;


            DateTime startdate = dateTimeNow;
            DateTime startdate2 = dateTimeNow.AddMinutes(10);

            xAxis.Minimum = DateTimeAxis.ToDouble(startdate);
            xAxis.Maximum = DateTimeAxis.ToDouble(startdate2);

            plotModel.InvalidatePlot(true);
            #endregion

            #region Initialize axis
            //Set the number position to the correct position
            yAxis.Position = AxisPosition.Left;
            xAxis.Position = AxisPosition.Bottom;

            //Set lines in background
            xAxis.MajorGridlineStyle = LineStyle.Solid;
            yAxis.MajorGridlineStyle = LineStyle.Solid;

            //Disable the axis zoom
            yAxis.IsZoomEnabled = false;

            //Add axis to graph
            plotModel.Axes.Add(xAxis);
            plotModel.Axes.Add(yAxis);
            #endregion
        }

        private void ResetAxisButton_Click(object sender, RoutedEventArgs e)
        {
            LastComboBox.SelectedIndex = 0;

            xAxis.Minimum = DateTimeAxis.ToDouble(UnixToDateTime(PlotXMinMax[0]));
            xAxis.Maximum = DateTimeAxis.ToDouble(UnixToDateTime(PlotXMinMax[1] + 100));

            plotModel.InvalidatePlot(true);

            plotModel.ResetAllAxes();
        }

        private void PauseButton_Click(object sender, RoutedEventArgs e)
        {
            LastComboBox.SelectedIndex = 0; //folowing last x time off #todo fix
        }

        private void ShowSettingsButton_Click(object sender, RoutedEventArgs e)
        {
            showSettings = !showSettings;
            if (showSettings)
            {
                ShowSettingsButton.Content = ">";
                BackgroundGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(250) });
                GraphSettingsGroupBox.Visibility = Visibility.Visible;

            }
            else
            {
                ShowSettingsButton.Content = "<";
                BackgroundGrid.ColumnDefinitions.RemoveAt(1);
                GraphSettingsGroupBox.Visibility = Visibility.Hidden;
            }
        }

        private void PlotUserControl_OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue is ViewModelManager vmmOld)
            {
                vmmOld.RefreshViewModel.RefreshLow -= this.Refresh;
            }
            if (e.NewValue is ViewModelManager vmm)
            {
                this.systemViewModel = vmm.SystemViewModel;
                this.plottingViewModel = vmm.PlottingViewModel;
                vmm.RefreshViewModel.RefreshHigh += this.Refresh;
            }
        }

        private void Refresh(object sender, EventArgs e)
        {




            double xAxisWindowMinumum = ((DateTimeOffset)DateTime.FromOADate(xAxis.ActualMinimum)).ToUnixTimeMilliseconds(); //convert current x minimum in window to unixtimestamp double
            double xAxisWindowMaximum = ((DateTimeOffset)DateTime.FromOADate(xAxis.ActualMaximum)).ToUnixTimeMilliseconds();
            plottingViewModel.RefreshBtrees(xAxisWindowMinumum, xAxisWindowMaximum);

            if (plottingViewModel.BtreesToPlot != null)
            {


                LastTimeUpdate(); //set window to good size if lastXtime is not off

                int i = 0;
                //plotModel.Series.Clear(); //deletes all data points
                foreach (KeyValuePair<Register, List<NodeStatistics>> entry in plottingViewModel.BtreesToPlot)
                {
                    if (i == 0 && entry.Value != null)
                    {
                        plotModel.Series.Clear(); //deletes all data points
                    }
                    if (entry.Value != null)
                    {
                        plotModel.Series.Add(new AreaSeries() { Color = GetOxyColor(i + 1), Color2 = GetOxyColor(i + 1) });
                        plotModel.Series.Add(new LineSeries() { Title = entry.Key.FullName, Color = GetOxyColor(i), MarkerFill = GetOxyColor(i + 1) });
                        foreach (NodeStatistics nodePoint in entry.Value)
                        {
                            if (nodePoint.currentNodeLevel != 0) //above lowest level, add average and minmax
                            {
                                (plotModel.Series[i + 1] as LineSeries).Points.Add(new DataPoint(DateTimeAxis.ToDouble(UnixToDateTime(nodePoint.xAvg)), nodePoint.yAvg));
                                if (DisplayMinMaxOn == true) //minmax on/off checkbox
                                {
                                    (plotModel.Series[i] as AreaSeries).Points.Add(new DataPoint(DateTimeAxis.ToDouble(UnixToDateTime(nodePoint.xAvg)), nodePoint.yMin));
                                    (plotModel.Series[i] as AreaSeries).Points2.Add(new DataPoint(DateTimeAxis.ToDouble(UnixToDateTime(nodePoint.xAvg)), nodePoint.yMax));
                                }
                            }
                            else //lowest level, add only seperate points
                            {
                                double[,] leafPoints = nodePoint.leafPoints;
                                for (int j = 0; j < leafPoints.GetLength(0); j++)
                                {
                                    if (leafPoints[j, 0] != 0) //x should never be zero, if it's zero, that array point isn't filled yet
                                    {
                                        (plotModel.Series[i + 1] as LineSeries).Points.Add(new DataPoint(DateTimeAxis.ToDouble(UnixToDateTime(leafPoints[j, 0])), leafPoints[j, 1]));
                                    }
                                }
                            }
                        }

                        i = i + 2;
                    }
                }
                plotModel.InvalidatePlot(true);
            }
        }

        private void LastComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        public void LastTimeUpdate()
        {
            this.Dispatcher.Invoke(() =>
            {
                double btreeMax = PlotXMinMax[1];
                if (LastComboBox.SelectedIndex == 0 || PlotXMinMax == null) //off
                {
                    //do nothing
                }
                else
                {
                    DateTime datetimeLast = DateTime.FromOADate(xAxis.ActualMaximum);
                    DateTime datetimeMinusXTime = DateTime.FromOADate(xAxis.ActualMinimum);
                    if (LastComboBox.SelectedIndex == 1) //1 second
                    {
                        datetimeLast = UnixToDateTime(btreeMax);
                        datetimeMinusXTime = datetimeLast.AddSeconds(-1);
                    }
                    else if (LastComboBox.SelectedIndex == 2) //10 seconds
                    {
                        datetimeLast = UnixToDateTime(btreeMax);
                        datetimeMinusXTime = datetimeLast.AddSeconds(-10);
                    }
                    else if (LastComboBox.SelectedIndex == 3) //30 seconds
                    {
                        datetimeLast = UnixToDateTime(btreeMax);
                        datetimeMinusXTime = datetimeLast.AddSeconds(-30);
                    }
                    else if (LastComboBox.SelectedIndex == 4) //1 minute
                    {
                        datetimeLast = UnixToDateTime(btreeMax);
                        datetimeMinusXTime = datetimeLast.AddMinutes(-1);
                    }
                    else if (LastComboBox.SelectedIndex == 5) //10 minutes
                    {
                        datetimeLast = UnixToDateTime(btreeMax);
                        datetimeMinusXTime = datetimeLast.AddMinutes(-10);
                    }
                    else if (LastComboBox.SelectedIndex == 6) //1 hour
                    {
                        datetimeLast = UnixToDateTime(btreeMax);
                        datetimeMinusXTime = datetimeLast.AddHours(-1);
                    }
                    else if (LastComboBox.SelectedIndex == 7) //1 day
                    {
                        datetimeLast = UnixToDateTime(btreeMax);
                        datetimeMinusXTime = datetimeLast.AddDays(-1);
                    }
                    else if (LastComboBox.SelectedIndex == 8) //1 month
                    {
                        datetimeLast = UnixToDateTime(btreeMax);
                        datetimeMinusXTime = datetimeLast.AddMonths(-1);
                    }

                    plotModel.ResetAllAxes();
                    //xAxis.Reset(); //comment out if you want to stop autorest wel zooming or moving
                    xAxis.Maximum = DateTimeAxis.ToDouble(datetimeLast);
                    xAxis.Minimum = DateTimeAxis.ToDouble(datetimeMinusXTime);
                    plotModel.InvalidatePlot(true); //updates plot
                }
            });


        }

        private void ZoomFitCheckBox_CheckBoxChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void ZoomFitButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void DisplayMinMax_Checked(object sendern, RoutedEventArgs e)
        {
            DisplayMinMaxOn = true;
        }

        private void DisplayMinMax_Unchecked(object sendern, RoutedEventArgs e)
        {
            DisplayMinMaxOn = false;
        }

        private void YAutoScaleCheckBox_Checked(object sendern, RoutedEventArgs e)
        {
            //reset y axis
            yAxis.Reset();

            //Disable the y axis zoom
            yAxis.IsZoomEnabled = false;
        }

        private void YAutoScaleCheckBox_Unchecked(object sendern, RoutedEventArgs e)
        {
            //Enable the y axis zoom
            yAxis.IsZoomEnabled = true;
        }



        /// <summary>
        /// converts unixmillisecondstimestamp to datetime format
        /// </summary>
        /// <param name="unixTimestamp"></param>
        /// <returns></returns>
        public DateTime UnixToDateTime(double unixTimestamp)
        {
            long data = Convert.ToInt64(unixTimestamp);
            TimeSpan time = TimeSpan.FromMilliseconds(Convert.ToDouble(data));
            DateTime startdate = new DateTime(1970, 1, 1) + time;
            return startdate.ToLocalTime();
        }

        public OxyColor GetOxyColor(int plotCount)
        {
            int modulo = plotCount % 20;

            switch (modulo)
            {
                case 0:
                    return OxyColors.Blue;
                case 1:
                    return OxyColors.LightBlue;
                case 2:
                    return OxyColors.Green;
                case 3:
                    return OxyColors.LightGreen;
                case 4:
                    return OxyColors.Red;
                case 5:
                    return OxyColors.IndianRed;
                case 6:
                    return OxyColors.Cyan;
                case 7:
                    return OxyColors.LightCyan;
                case 8:
                    return OxyColors.HotPink;
                case 9:
                    return OxyColors.LightPink;
                case 10:
                    return OxyColors.Yellow;
                case 11:
                    return OxyColors.LightYellow;
                case 12:
                    return OxyColors.Purple;
                case 13:
                    return OxyColors.MediumPurple;
                case 14:
                    return OxyColors.Chocolate;
                case 15:
                    return OxyColors.BurlyWood;
                case 16:
                    return OxyColors.DarkOrange;
                case 17:
                    return OxyColors.Orange;
                case 18:
                    return OxyColors.MidnightBlue;
                case 19:
                    return OxyColors.DarkSlateBlue;
                default:
                    return OxyColors.Automatic;
            }


        }
    }
}
