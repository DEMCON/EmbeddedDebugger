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
using OxyPlot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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

        private List<Register> plotRegisters;
        public List<Register> PlotRegisters { get => plotRegisters; }
        public PlotUserControl()
        {
            InitializeComponent();
            plotRegisters = new List<Register>();
            plotModel = new PlotModel { Title = "" };
            Plot.Model = plotModel;
            AutoScaleCheckBox.IsChecked = true;
            Register.EnablePlotUpdate = !paused;
        }

        public void AddRegisterToPlot(Register register)
        {
            if (register.Line != null && plotModel.Series.Contains(register.Line)) return;
            if (!register.Plot) register.Plot = true;
            register.PlotModel = plotModel;
            plotModel.Series.Add(register.Line);
            register.NumberOfSeconds = double.Parse(RangeXTextBox.Text);
            plotRegisters.Add(register);
        }

        public void RemoveRegisterFromPlot(Register register)
        {
            if (register.Line == null) return;
            if (register.Plot) register.Plot = false;
            register.PlotModel = null;
            plotModel.Series.Remove(register.Line);
            PlotRegisters.Remove(register);
        }

        private void AutoScaleCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            ManualGroupBox.IsEnabled = !(bool)AutoScaleCheckBox.IsChecked;
            if (AutoScaleCheckBox.IsChecked.HasValue && (bool)AutoScaleCheckBox.IsChecked)
            {
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
        }

        private void ResetAxisButton_Click(object sender, RoutedEventArgs e)
        {
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
            Register.EnablePlotUpdate = !paused;
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

        private void MinYScaleTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if(!double.TryParse(e.Text, out double result) && !e.Text.Equals("."))
            {
                e.Handled =true;
            }
        }

        private void MaxYScaleTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (!double.TryParse(e.Text, out double result) && !e.Text.Equals("."))
            {
                e.Handled = true;
            }
        }

        private void RangeXTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (!double.TryParse(RangeXTextBox.Text, out double result))
            {
                e.Handled = true;
            }
        }

        private void RangeXTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if(plotRegisters == null || !double.TryParse(RangeXTextBox.Text, out double result)) return;
            foreach (Register r in plotRegisters)
            {
                r.NumberOfSeconds = result;
            }
        }

        private void RangeYTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(MaxYScaleTextBox.Text)) return;
            if (!(bool)AutoScaleCheckBox.IsChecked)
            {
                ElementCollection<OxyPlot.Axes.Axis> ec = plotModel.Axes;
                plotModel.Axes[1].Maximum = Double.Parse(MaxYScaleTextBox.Text);
                ElementCollection<OxyPlot.Axes.Axis> ec2 = plotModel.Axes;
            }
        }
    }
}
