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
using EmbeddedDebugger.Model.Logging;
using EmbeddedDebugger.View.Windows;
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
    /// Interaction logic for LoggingUserControl.xaml
    /// </summary>
    public partial class LoggingUserControl : UserControl
    {
        #region fields
        private bool logging;
        private ValueLogger logger;
        #endregion

        #region Properties
        public bool Logging { get => logging; set => logging = value; }
        public ValueLogger Logger { get => logger; set => logger = value; }
        #endregion

        public LoggingUserControl()
        {
            InitializeComponent();
            DirectoryTextBox.Text = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\Logs";
            TxtRadioButton.IsChecked = true;
            SetAdditionalOptionsUserControl();
        }

        private void SetAdditionalOptionsUserControl()
        {
            List<Control> groupBoxes = new List<Control>()
            {
                TxtAdditionalOptionsGroupBox,
                CsvAdditionalOptionsGroupBox,
            };
            foreach (Control c in groupBoxes)
            {
                c.Visibility = Visibility.Hidden;
            }

            if ((bool)TxtRadioButton.IsChecked)
            {
                TxtAdditionalOptionsGroupBox.Visibility = Visibility.Visible;
            }
            else if ((bool)CsvRadioButton.IsChecked)
            {
                CsvAdditionalOptionsGroupBox.Visibility = Visibility.Visible;
            }
        }

        private void TxtRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            SetAdditionalOptionsUserControl();
        }

        private void CsvRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            SetAdditionalOptionsUserControl();
        }

        private void StartLoggingButton_Click(object sender, RoutedEventArgs e)
        {
            if (!logger.IsLogging)
            {
                logger.Directory = DirectoryTextBox.Text.Replace(@"\", @"\\");
                logger.FileNameTemplate = FileNameTextBox.Text.Replace(@"\", @"\\");
                logger.FileType = GetFileType();
                logger.Separator = SeparatorTextBox.Text.Replace("\\t", "\t").Replace("\\n", "\n").Replace("\\r", "\r");
                logger.TimeStampUsage = GetTimeStampSelected();
                logger.SeparateFilePerCpuNode = (bool)UseSeparatePerCPUNodeCheckBox.IsChecked;
                switch (logger.FileType)
                {
                    case FileType.txt:
                        logger.TxtAddVersionInfoToHeader = (bool)TxtAddVersionHeaderCheckBox.IsChecked;
                        break;
                    case FileType.csv:
                        logger.CsvUseHeader = (bool)CsvHeaderCheckBox.IsChecked;
                        break;
                }
                try
                {
                    logger.Start();
                }
                catch (Exception ex)
                {
                    System.Windows.Forms.MessageBox.Show(ex.Message);
                    return;
                }
                StartLoggingButton.Content = "Stop Logging";
            }
            else
            {
                logger.Stop();
                StartLoggingButton.Content = "Start Logging";
            }
        }

        private void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog fbd = new System.Windows.Forms.FolderBrowserDialog();
            if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                DirectoryTextBox.Text = fbd.SelectedPath;
            }
        }

        private void HelpButton_Click(object sender, RoutedEventArgs e)
        {
            LogFileNameHelperWindow lfnhw = new LogFileNameHelperWindow()
            {
                FileName = FileNameTextBox.Text
            };
            if ((bool)TxtRadioButton.IsChecked)
            {
                lfnhw.FileExtention = ".txt";
            }
            else if ((bool)CsvRadioButton.IsChecked)
            {
                lfnhw.FileExtention = ".csv";
            }
            if ((bool)lfnhw.ShowDialog())
            {
                FileNameTextBox.Text = lfnhw.FileName;
                switch (lfnhw.FileExtention)
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

        private FileType GetFileType()
        {
            if ((bool)TxtRadioButton.IsChecked)
            {
                return FileType.txt;
            }
            if ((bool)CsvRadioButton.IsChecked)
            {
                return FileType.csv;
            }
            throw new ArgumentException("No filetype selected");
        }

        private TimeStampUsage GetTimeStampSelected()
        {
            if ((bool)NoneTimeStampRadioButton.IsChecked)
            {
                return TimeStampUsage.None;
            }
            if ((bool)RelativeTimeStampRadioButton.IsChecked)
            {
                return TimeStampUsage.Relative;
            }
            if ((bool)AbsoluteTimeStampRadioButton.IsChecked)
            {
                return TimeStampUsage.Absolute;
            }
            throw new ArgumentException("No Timestamp usage selected");
        }
    }
}
