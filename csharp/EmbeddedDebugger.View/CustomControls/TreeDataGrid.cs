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
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
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
using System.Text.RegularExpressions;
using EmbeddedDebugger.View.Converters;

namespace EmbeddedDebugger.View.CustomControls
{
    public class TreeDataGrid : DataGrid
    {
        private IList<Register> mySource;
        private string searchValue = "";

        public IList<Register> TreeItemsDataSource
        {
            get
            {
                return mySource;
            }
            set
            {
                mySource = value.ToList();
                Dispatcher.Invoke(delegate { ItemsSource = value; });
                SearchThrough(searchValue);
            }
        }

        public string SearchValue
        {
            get => searchValue;
            set
            {
                searchValue = value;
                SearchThrough(searchValue);
            }
        }

        public TreeDataGrid()
        {
            DataGridTemplateColumn col = new DataGridTemplateColumn();
            DataTemplate dataTemplate = new DataTemplate();
            var buttonBlockFactory = new FrameworkElementFactory(typeof(Button));
            dataTemplate.VisualTree = buttonBlockFactory;
            buttonBlockFactory.SetBinding(Button.VisibilityProperty, new Binding("HasChildren") { Converter = new System.Windows.Controls.BooleanToVisibilityConverter() });
            buttonBlockFactory.SetBinding(Button.ContentProperty, new Binding("IsCollapsed") { Converter = new BooleanToCollapsedStringConverter() });
            buttonBlockFactory.SetValue(Button.BackgroundProperty, Brushes.Transparent);
            buttonBlockFactory.SetValue(Button.BorderThicknessProperty, new Thickness(0));
            buttonBlockFactory.AddHandler(Button.ClickEvent, new RoutedEventHandler(ButtonClicked));
            col.CellTemplate = dataTemplate;
            Columns.Insert(0, col);
        }

        public void CollapseAll()
        {
            List<Register> searchRegs = new List<Register>();
            foreach (Register reg in mySource.Cast<Register>())
            {
                if (reg.Parent == null)
                {
                    searchRegs.Add(reg);
                }
            }
            foreach (Register reg in searchRegs)
            {
                reg.IsCollapsed = true;
            }
            mySource = searchRegs;
            SearchThrough(searchValue);
        }

        public void ExpandAll()
        {
            List<Register> searchRegs = new List<Register>();
            foreach (Register reg in mySource.Cast<Register>())
            {
                if (reg.IsCollapsed)
                {
                    searchRegs.AddRange(GetAllChildNodes(reg));
                }
                else
                {
                    searchRegs.Add(reg);
                }
            }
            foreach (Register reg in searchRegs)
            {
                reg.IsCollapsed = false;
            }
            mySource = searchRegs;
            SearchThrough(searchValue);
        }

        private void SearchThrough(string searchValue)
        {
            try
            {
                var myRegex = new Regex(searchValue);
                IEnumerable<Register> enabled = mySource.Where(x => x.ChannelMode != ChannelMode.Off);
                IEnumerable<Register> filtered = mySource.Where(x => myRegex.IsMatch(x.FullName));
                ItemsSource = enabled.Union(filtered);
                Items.Refresh();
            }
            catch
            { }
        }

        private void ButtonClicked(object sender, RoutedEventArgs e)
        {
            Register register = (Register)CurrentItem;
            register.IsCollapsed = !register.IsCollapsed;
            List<Register> regs = ItemsSource.Cast<Register>().ToList();
            if (register.IsCollapsed)
            {
                foreach (Register reg in register.ChildRegisters)
                {
                    regs.Remove(reg);
                }
            }
            else
            {
                int counter = 1;
                foreach (Register reg in register.ChildRegisters)
                {
                    regs.Insert(regs.IndexOf(register) + counter++, reg);
                }
            }
            mySource = regs.ToList();
            ItemsSource = null;
            ItemsSource = regs;
            SearchThrough(searchValue);
        }

        private List<Register> GetAllChildNodes(Register parent)
        {
            List<Register> returnable = new List<Register>
            {
                parent
            };
            foreach (Register child in parent.ChildRegisters)
            {
                returnable.AddRange(GetAllChildNodes(child));
            }
            return returnable;
        }

        public void RemoveItem(Register register)
        {
            foreach (Register reg in register.ChildRegisters)
            {
                RemoveItem(reg);
            }
            mySource.Remove(register);
            SearchThrough(searchValue);
        }
    }
}
