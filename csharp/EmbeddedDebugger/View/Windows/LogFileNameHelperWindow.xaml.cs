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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace EmbeddedDebugger.View.Windows
{
    /// <summary>
    /// Interaction logic for LogFileNameHelperWindow.xaml
    /// </summary>
    public partial class LogFileNameHelperWindow : Window
    {
        private int counter;
        public string FileName { get => InputTextBox.Text; set => InputTextBox.Text = value; }
        public string FileExtention
        {
            get
            {
                if ((bool)TxtRadioButton.IsChecked)
                {
                    return ".txt";
                }
                else if ((bool)CsvRadioButton.IsChecked)
                {
                    return ".csv";
                }
                else
                {
                    return "";
                }
            }
            set
            {
                switch (value)
                {
                    case ".txt":
                        TxtRadioButton.IsChecked = true;
                        break;
                    case ".csv":
                        CsvRadioButton.IsChecked = true;
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }
        }

        public LogFileNameHelperWindow()
        {
            InitializeComponent();
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void GenerateButton_Click(object sender, RoutedEventArgs e)
        {
            counter = 1;
            Fill();
        }

        private void Fill()
        {
            string output = InputTextBox.Text;
            NextFileButton.IsEnabled = (bool)MultipleFilesTextBox.IsChecked;
            output = output.Replace("{yyyy}", DateTime.Now.Year.ToString());
            output = output.Replace("{MM}", DateTime.Now.Month.ToString());
            output = output.Replace("{dd}", DateTime.Now.Day.ToString());
            output = output.Replace("{HH}", DateTime.Now.Hour.ToString());
            output = output.Replace("{mm}", DateTime.Now.Minute.ToString());
            output = output.Replace("{ss}", DateTime.Now.Second.ToString());
            output = output.Replace("{CPU}", CpuTextBox.Text);
            output = output.Replace("{Serial}", SerialTextBox.Text);
            output = output.Replace("{Counter}", counter.ToString());
            output = output.Replace("{ID}", 1.ToString());
            if ((bool)TxtRadioButton.IsChecked)
            {
                output += ".txt";
            }
            else if ((bool)CsvRadioButton.IsChecked)
            {
                output += ".csv";
            }
            OutputTextBox.Text = output;
        }

        private void NextFileButton_Click(object sender, RoutedEventArgs e)
        {
            counter++;
            Fill();
        }
    }
}
