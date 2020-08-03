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
        private bool showSettings = true;
        private SystemViewModel systemViewModel;
        private PlottingViewModel plottingViewModel;

        private DateTimeAxis xAxis = new DateTimeAxis();
        private LinearAxis yAxis = new LinearAxis();

        private Dictionary<Register, LineSeries> plotSeries;

        private List<Register> plotRegisters;
        public List<Register> PlotRegisters { get => this.plottingViewModel.RegistersToPlot; }
        public PlotUserControl()
        {
            InitializeComponent();
            plotRegisters = new List<Register>();
            plotModel = new PlotModel { Title = "" };
            Plot.Model = plotModel;
            YAutoScaleCheckBox.IsChecked = true;
            this.plotSeries = new Dictionary<Register, LineSeries>();


            DateTime dateTimeNow = DateTime.Now;
            var dateTimeOffset = new DateTimeOffset(dateTimeNow);
            var unixDateTime = dateTimeOffset.ToUnixTimeMilliseconds();
            var unixDateTime2 = dateTimeOffset.ToUnixTimeSeconds() + 10;


            DateTime startdate = dateTimeNow;
            DateTime startdate2 = dateTimeNow.AddMinutes(10);

            xAxis.Minimum = DateTimeAxis.ToDouble(startdate);
            xAxis.Maximum = DateTimeAxis.ToDouble(startdate2);

            plotModel.InvalidatePlot(true);

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

            plotModel.ResetAllAxes();
            foreach (OxyPlot.Axes.Axis ax in plotModel.Axes)
            {
                ax.AbsoluteMaximum = double.MaxValue;
                ax.AbsoluteMinimum = double.MinValue;

                ax.Minimum = double.NaN;
                ax.Maximum = double.NaN;
                ax.MinorStep = double.NaN;
                ax.MajorStep = double.NaN;

                ax.MinimumPadding = 0.01;
                ax.MaximumPadding = 0.01;
                ax.MinimumRange = 0;

                ax.Reset();
            }
        }

        private void PauseButton_Click(object sender, RoutedEventArgs e)
        {
            paused = !paused;
            if (paused)
            {
                PauseButton.Content = "▶";
            }
            else
            {
                PauseButton.Content = "❚❚";
            }
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
            LastTimeUpdate(); //set window to good size if lastXtime is not off

            double xAxisWindowMinumum = ((DateTimeOffset)DateTime.FromOADate(xAxis.ActualMinimum)).ToUnixTimeMilliseconds(); //convert current x minimum in window to unixtimestamp double
            double xAxisWindowMaximum = ((DateTimeOffset)DateTime.FromOADate(xAxis.ActualMaximum)).ToUnixTimeMilliseconds();
            plottingViewModel.RefreshBtrees(xAxisWindowMinumum, xAxisWindowMaximum);

            if (plottingViewModel.BtreesToPlot != null)
            {
                int i = 0;
                foreach (KeyValuePair <Register, List<NodeStatistics>> entry in plottingViewModel.BtreesToPlot)
                {
                    if (entry.Value != null)
                    {
                        plotModel.Series.Clear(); //deletes all data points
                        plotModel.Series.Add(new AreaSeries() { Color = OxyColors.LightBlue, Color2 = OxyColors.LightBlue });
                        plotModel.Series.Add(new LineSeries() { Title = entry.Key.FullName, Color = OxyColors.DarkBlue, MarkerFill = OxyColors.Blue });



                        foreach (NodeStatistics nodePoint in entry.Value)
                        {
                            if (nodePoint.currentNodeLevel != 0) //above lowest level, add average and minmax
                            {
                                (plotModel.Series[i + 1] as LineSeries).Points.Add(new DataPoint(DateTimeAxis.ToDouble(UnixToDateTime(nodePoint.xAvg)), nodePoint.yAvg));
                                if (DisplayMinMax.IsChecked == true) //minmax on/off checkbox
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

        public void LastTimeUpdate ()
        {
            double btreeMax = plottingViewModel.xAxisMinMax[1];
            if (LastComboBox.SelectedIndex == 0 || plottingViewModel.xAxisMinMax == null) //off
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
        }

        private void ZoomFitCheckBox_CheckBoxChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void ZoomFitButton_Click(object sender, RoutedEventArgs e)
        {

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

        
    }
}
