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
using EmbeddedDebugger.DebugProtocol;

namespace EmbeddedDebugger.View.Windows
{
    /// <summary>
    /// Interaction logic for RegisterChooserWindow.xaml
    /// </summary>
    public partial class RegisterChooserWindow : Window
    {
        #region fields
        private Register originalRegister;
        #endregion
        #region Properties
        public Register OriginalRegister { get => originalRegister; set => originalRegister = value; }
        public Register SelectedRegister { get => (Register)RegisterDataGrid.SelectedItem; }
        public IList<Register> Registers { set => RegisterDataGrid.ItemsSource = value; }
        #endregion
        public RegisterChooserWindow()
        {
            InitializeComponent();
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            if (originalRegister.Equals(SelectedRegister))
            {
                MessageBox.Show("Please do not select the same register to be its parent...");
                return;
            }
            if (FindRegisterAsParent(originalRegister, SelectedRegister))
            {
                MessageBox.Show("Please make sure the register is not part of the registers parents");
                return;
            }
            DialogResult = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private bool FindRegisterAsParent(Register register, Register newParent)
        {
            if (newParent == null)
            {
                return false;
            }
            if (register.Equals(newParent))
            {
                return true;
            }
            if(newParent.Parent == null)
            {
                return false;
            }
            return FindRegisterAsParent(register, newParent.Parent);
        }
    }
}
