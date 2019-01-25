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
using EmbeddedDebugger.Model.EmbeddedConfiguration;
using EmbeddedDebugger.View.Windows;
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
    /// Interaction logic for EditXMLUserControl.xaml
    /// </summary>
    public partial class EditXMLUserControl : UserControl
    {
        #region fields
        private EmbeddedConfig embeddedConfig;
        private XmlConfigurationParser parser;
        private string fileName;
        #endregion

        #region Properties
        public EmbeddedConfig EmbeddedConfig
        {
            get => embeddedConfig;
            set
            {
                embeddedConfig = value;
                RegisterDataGrid.Dispatcher.Invoke(delegate { RegisterDataGrid.TreeItemsDataSource = embeddedConfig.Registers; });
            }
        }
        public List<object> Connectors
        {
            get => ConnectorComboBox.Items.Cast<object>().ToList();
            set
            {
                ConnectorComboBox.ItemsSource = value.OrderBy(x => x.ToString());
                if (value.Count > 0)
                {
                    ConnectorComboBox.SelectedIndex = 0;
                }
            }
        }
        public string Connector { get => Dispatcher.Invoke(delegate { return ConnectorComboBox.SelectedItem.ToString(); }); }
        public string ID { get => Dispatcher.Invoke(delegate { return IDTextBox.Text; }); }
        public string CPUName { get => Dispatcher.Invoke(delegate { return CpuNameTextBox.Text; }); }
        public string FileName { get => fileName; set => fileName = value; }
        #endregion


        public EditXMLUserControl()
        {
            InitializeComponent();
            parser = new XmlConfigurationParser();
            parser.PercentageChanged += Parser_PercentageChanged;
        }

        private void Parser_PercentageChanged(object sender, EventArgs e)
        {
            ProgressBar.Dispatcher.Invoke(delegate
            {
                ProgressBar.Value = (int)parser.Percentage;
            });
        }

        private void RemoveButton_Click(object sender, RoutedEventArgs e)
        {
            Register reg = (Register)RegisterDataGrid.CurrentItem;
            if (reg == null) return;
            RegisterDataGrid.RemoveItem(reg);
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            RegisterDataGrid.SearchValue = SearchTextBox.Text;
        }

        private void GenerateButton_Click(object sender, RoutedEventArgs e)
        {
            GenerateButton.IsEnabled = false;
            LoadXMLButton.IsEnabled = false;
            RegisterGroupBox.IsEnabled = false;
            embeddedConfig = new EmbeddedConfig
            {
                CpuName = CpuNameTextBox.Text,
                ApplicationVersion = $"{int.Parse(VersionMajorTextBox.Text).ToString("D2")}_{int.Parse(VersionMinorTextBox.Text).ToString("D2")}_{int.Parse(VersionRevisionTextBox.Text).ToString("D4")}",
            };
            embeddedConfig.SetRegisters(RegisterDataGrid.Items.Cast<Register>().ToList());

            // Do this in a separate thread, to make sure the GUI doesn't freeze
            Thread t = new Thread(GenerateFile)
            {
                IsBackground = true
            };
            t.Start();
        }

        private void GenerateFile()
        {
            parser.ToFile($"{Properties.Settings.Default.ConfigurationPath}\\{Connector}\\{CPUName}\\cpu{ID}-V{embeddedConfig.ApplicationVersion}.xml", embeddedConfig);
            Dispatcher.Invoke(delegate
            {
                GenerateButton.IsEnabled = true;
                LoadXMLButton.IsEnabled = true;
                RegisterGroupBox.IsEnabled = true;
            });
        }

        private void LoadXMLButton_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog ofd = new System.Windows.Forms.OpenFileDialog
            {
                Title = "Open XML File",
                Filter = "XML files|*.xml",
                InitialDirectory = @"C:\"
            };
            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK && ofd.CheckFileExists)
            {
                fileName = ofd.FileName;
                GenerateButton.IsEnabled = false;
                LoadXMLButton.IsEnabled = false;
                // Do this in a separate thread, to make sure the GUI doesn't freeze
                Thread t = new Thread(LoadFile)
                {
                    IsBackground = true,
                };
                t.Start();
            }
        }

        private void LoadFile()
        {
            embeddedConfig = parser.FromFile(fileName);
            RegisterDataGrid.TreeItemsDataSource = embeddedConfig.Registers;
            GenerateButton.Dispatcher.Invoke(delegate
            {
                GenerateButton.IsEnabled = true;
                LoadXMLButton.IsEnabled = true;
                if (embeddedConfig.CpuName != null)
                {
                    CpuNameTextBox.Text = embeddedConfig.CpuName;
                    string[] strings = embeddedConfig.ApplicationVersion.Split('_');
                    VersionMajorTextBox.Text = strings[0];
                    VersionMinorTextBox.Text = strings[1];
                    VersionRevisionTextBox.Text = strings[2];
                    string conn = fileName.Remove(fileName.Remove(fileName.LastIndexOf('\\')).LastIndexOf('\\'));
                    conn = conn.Substring(conn.LastIndexOf('\\') + 1);
                    if (ConnectorComboBox.Items.Cast<object>().Any(x => x.ToString().Equals(conn)))
                    {
                        ConnectorComboBox.SelectedItem = ConnectorComboBox.Items.Cast<object>().First(x => x.ToString().Equals(conn));
                    }
                    string ID = fileName.Substring(fileName.LastIndexOf('\\')+1);
                    ID = ID.Remove(ID.LastIndexOf('-')).Replace("cpu","");
                    IDTextBox.Text = ID;
                }
            });
        }

        private void VersionMajorTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (!int.TryParse(e.Text, out int result))
            {
                e.Handled = true;
            }
        }

        private void VersionMinorTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (!int.TryParse(e.Text, out int result))
            {
                e.Handled = true;
            }
        }

        private void VersionRevisionTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (!int.TryParse(e.Text, out int result))
            {
                e.Handled = true;
            }
        }

        private void ExpandAllButton_Click(object sender, RoutedEventArgs e)
        {
            RegisterDataGrid.ExpandAll();
        }

        private void CollapseAllButton_Click(object sender, RoutedEventArgs e)
        {
            RegisterDataGrid.CollapseAll();
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            Register reg = ((Register)RegisterDataGrid.CurrentItem);
            if (reg == null || !(bool)reg.Show) return;
            CheckUncheckChildren(reg.ChildRegisters, true);
            CheckUncheckParents(reg.Parent);
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            Register reg = ((Register)RegisterDataGrid.CurrentItem);
            if (reg == null || (bool)reg.Show) return;
            CheckUncheckChildren(reg.ChildRegisters, false);
            CheckUncheckParents(reg.Parent);
        }

        private void CheckUncheckChildren(List<Register> registers, bool isChecked)
        {
            foreach (Register creg in registers)
            {
                creg.Show = isChecked;
                CheckUncheckChildren(creg.ChildRegisters, isChecked);
            }
        }

        private void CheckUncheckParents(Register parent)
        {
            if (parent == null) return;
            if (parent.ChildRegisters.All(x => x.Show == true))
            {
                parent.Show = true;
                CheckUncheckParents(parent.Parent);
            }
            else if (parent.ChildRegisters.All(x => x.Show == false))
            {
                parent.Show = false;
                CheckUncheckParents(parent.Parent);
            }
            else
            {
                parent.Show = null;
                CheckUncheckParents(parent.Parent);
            }

        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Register reg = (Register)RegisterDataGrid.CurrentItem;
            if (reg == null) return;
            SetReadWriteChildren(reg, reg.ReadWrite);
        }

        private void SetReadWriteChildren(Register reg, ReadWrite readWrite)
        {
            if (reg.HasChildren)
            {
                foreach (Register r in reg.ChildRegisters)
                {
                    SetReadWriteChildren(r, readWrite);
                }
            }
            else
            {
                reg.ReadWrite = readWrite;
            }
        }

        private void CustomRegisterButton_Click(object sender, RoutedEventArgs e)
        {
            EditRegisterWindow edw = new EditRegisterWindow
            {
                Register = new Register()
            };
            if (edw.ShowDialog() == true)
            {
                embeddedConfig.Registers.Add(edw.Register);
            }
            RegisterDataGrid.Items.Refresh();
        }

        private void Edit_Click(object sender, RoutedEventArgs e)
        {
            EditRegisterWindow edw = new EditRegisterWindow
            {
                Register = (Register)RegisterDataGrid.CurrentItem,
                Registers = embeddedConfig.Registers,
            };
            edw.ShowDialog();
            RegisterDataGrid.Items.Refresh();
        }

        private void NewConfigButton_Click(object sender, RoutedEventArgs e)
        {
            EmbeddedConfig = new EmbeddedConfig();
        }
    }
}
