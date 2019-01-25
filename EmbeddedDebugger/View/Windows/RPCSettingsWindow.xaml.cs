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
    /// Interaction logic for RPCSettingsWindow.xaml
    /// </summary>
    public partial class RPCSettingsWindow : Window
    {
        #region fields
        private uint port;
        #endregion

        #region Properties
        public int Port { get => (int)port; set => port = (uint)value; }
        #endregion

        public RPCSettingsWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Cancel the configuration, leaving all changes to be discarded
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">Arguments</param>
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        /// <summary>
        /// Apply the given configuration
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">Arguments</param>
        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            if (!uint.TryParse(PortTextbox.Text, out port))
            {
                System.Windows.Forms.MessageBox.Show("Please enter a valid port");
                return;
            }
            DialogResult = true;
        }


        /// <summary>
        /// Upon loading of the form
        /// </summary>
        /// <param name="sender">Sender of the event</param>
        /// <param name="e">Arguments</param>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Set the textboxes to defined or default values
            PortTextbox.Text = "" + (port <= 0 ? 59283 : port);
        }
    }
}
