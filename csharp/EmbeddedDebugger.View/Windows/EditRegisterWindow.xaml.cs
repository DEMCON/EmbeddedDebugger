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

namespace EmbeddedDebugger.View.Windows
{
    /// <summary>
    /// Interaction logic for EditRegisterWindow.xaml
    /// </summary>
    public partial class EditRegisterWindow : Window
    {
        #region fields
        private Register register;
        private Register parentRegister;
        private IList<Register> registers;
        #endregion

        #region Properties
        public Register Register
        {
            get => register;
            set
            {
                register = value;
                NameTextbox.Text = register.FullName;
                TypeComboBox.SelectedItem = register.VariableType;
                SizeNumericUpDown.Value = register.Size;
                ShowCheckBox.IsChecked = register.Show;
                OffsetNumericUpDown.Value = (int)register.Offset;
                SourceComboBox.SelectedItem = register.Source;
                DerefDepthNumericUpDown.Value = register.DerefDepth;
                ReadWriteComboBox.SelectedItem = register.ReadWrite;
                ParentTextBlock.Text = register.Parent == null ? "None" : $"{register.Parent.Id}. {register.Parent.Name}";
            }
        }
        public IList<Register> Registers { get => registers; set => registers = value; }
        #endregion

        public EditRegisterWindow()
        {
            InitializeComponent();
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(NameTextbox.Text))
            {
                MessageBox.Show("Please fill in a name");
                return;
            }
            try
            {
                register.Offset = (uint)OffsetNumericUpDown.Value;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }
            register.FullName = NameTextbox.Text;
            register.VariableType = (VariableType)TypeComboBox.SelectedItem;
            register.Size = SizeNumericUpDown.Value;
            register.Show = ShowCheckBox.IsChecked;
            register.Source = (Source)SourceComboBox.SelectedItem;
            register.DerefDepth = DerefDepthNumericUpDown.Value;
            register.ReadWrite = (ReadWrite)ReadWriteComboBox.SelectedItem;
            if (parentRegister == null && register.Parent == null)
            {
                DialogResult = true;
                return;
            }
            if ((parentRegister == null && register.Parent != null)
                || (parentRegister != null && register.Parent == null)
                || !register.Parent.Equals(parentRegister))
            {
                if (register.Parent == null)
                {
                    registers.Remove(register);
                }
                else
                {
                    register.Parent.ChildRegisters.Remove(register);
                }
                register.Parent = parentRegister;
                if (parentRegister != null)
                {
                    parentRegister.ChildRegisters.Add(register);
                }
                else
                {
                    registers.Add(register);
                }
            }

            DialogResult = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void ParentEditButton_Click(object sender, RoutedEventArgs e)
        {
            RegisterChooserWindow rcw = new RegisterChooserWindow
            {
                Registers = registers,
                OriginalRegister = register,
            };
            if (rcw.ShowDialog() == true)
            {
                parentRegister = rcw.SelectedRegister;
                ParentTextBlock.Text = parentRegister == null ? "None" : $"{parentRegister.Id}. {parentRegister.Name}";
            }
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            OffsetNumericUpDown.UseHex = true;
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            OffsetNumericUpDown.UseHex = false;
        }
    }
}
