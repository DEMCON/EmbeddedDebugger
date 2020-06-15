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
using EmbeddedDebugger.DebugProtocol.EmbeddedConfiguration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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
    /// Interaction logic for ParseUserControl.xaml
    /// </summary>
    public partial class ParseUserControl : UserControl
    {
        private string elfPath;
        private ElfParser elfParser;
        private EmbeddedConfig ec;
        public List<object> Connectors { get => EditXMLUserControl.Connectors; set => EditXMLUserControl.Connectors = value; }

        public ParseUserControl()
        {
            InitializeComponent();
            elfParser = new ElfParser();
        }

        #region Delegates
        private void HandWrittenCodeCheckBox_Click(object sender, RoutedEventArgs e)
        {
            HandWrittenCodeGroupBox.IsEnabled = (bool)HandWrittenCodeCheckBox.IsChecked;
        }

        private void SimulinkCodeCheckBox_Click(object sender, RoutedEventArgs e)
        {
            SimulinkCodeGroupBox.IsEnabled = (bool)SimulinkCodeCheckBox.IsChecked;
        }

        private void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog ofd = new System.Windows.Forms.OpenFileDialog
            {
                Title = "Open AXF/ELF File",
                Filter = "AXF/ELF files|*.axf;*.elf|AXF files|*.axf|ELF files|*.elf",
                InitialDirectory = @"C:\"
            };
            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK && ofd.CheckFileExists)
            {
                elfPath = ofd.FileName;
                LoadButton.IsEnabled = true;
                FileLocationTextBox.Text = elfPath;
            }
        }

        private void LoadButton_Click(object sender, RoutedEventArgs e)
        {
            BrowseButton.IsEnabled = false;
            LoadButton.IsEnabled = false;
            // Perform this task in a separate thread, to ensure the GUI doesn't freeze
            Thread t = new Thread(LoadFile);
            t.Start();
        }

        private void LoadFile()
        {
            elfParser.PercentageUpped += ElfParser_PercentageUpped;
            // Parse the file
            ec = elfParser.ParseFile(elfPath);

            EditXMLUserControl.Dispatcher.Invoke(delegate
           {
               EditXMLUserControl.EmbeddedConfig = ec;
           });
            elfParser.PercentageUpped -= ElfParser_PercentageUpped;
            BrowseButton.Dispatcher.Invoke(delegate
            {
                BrowseButton.IsEnabled = true;
                LoadButton.IsEnabled = true;
            });
        }

        private void ElfParser_PercentageUpped(object sender, EventArgs e)
        {
            ProgressBar.Dispatcher.Invoke(delegate
            {
                ProgressBar.Value = elfParser.Percentage;
            });
        }
        #endregion
    }
}
