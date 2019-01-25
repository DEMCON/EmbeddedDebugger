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
using System.ComponentModel;
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

namespace EmbeddedDebugger.View.CustomControls
{
    public class NumericUpDown : Grid
    {
        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register("Value", typeof(double), typeof(NumericUpDown));

        #region fields
        private readonly TextBox textBox;
        private readonly Button upButton;
        private readonly Button downButton;

        private bool useHex;
        private bool allowDecimals;
        private bool allowNegatives;
        #endregion

        #region Properties
        public double FontSize { get => textBox.FontSize; set => textBox.FontSize = value; }
        [Bindable(true)]
        public int Value
        {
            get
            {
                try
                {
                    return string.IsNullOrEmpty(textBox.Text) ? 0 : int.Parse(textBox.Text, UseHex ? NumberStyles.HexNumber : NumberStyles.Number);
                }
                catch (Exception)
                {
                    throw;
                }
            }
            set => textBox.Text = allowDecimals ? value.ToString() : ((int)value).ToString();
        }
        public bool AllowDecimals { get => allowDecimals; set => allowDecimals = value; }
        public bool AllowNegatives { get => allowNegatives; set => allowNegatives = value; }
        public bool UseHex
        {
            get => useHex;
            set
            {
                useHex = value;
                if (value)
                {
                    if (!int.TryParse(textBox.Text, out int result)) return;
                    textBox.Text = result.ToString("X");
                }
                else
                {
                    if (!int.TryParse(textBox.Text, NumberStyles.HexNumber, new CultureInfo("en-US"), out int result)) return;
                    textBox.Text = result.ToString();
                }
            }
        }
        #endregion

        public NumericUpDown()
        {
            ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(20, GridUnitType.Star) });
            ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(20) });
            RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
            RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
            HorizontalAlignment = HorizontalAlignment.Stretch;

            upButton = new Button
            {
                Content = "▲",
                FontSize = 4,
            };
            upButton.Click += UpButton_Click;
            SetRow(upButton, 0);
            SetColumn(upButton, 1);
            Children.Add(upButton);

            downButton = new Button
            {
                Content = "▼",
                FontSize = 4,
            };
            downButton.Click += DownButton_Click;
            SetRow(downButton, 1);
            SetColumn(downButton, 1);
            Children.Add(downButton);

            textBox = new TextBox
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Center,
                Text = "0",
            };
            textBox.PreviewTextInput += TextBox_PreviewTextInput;
            SetRow(textBox, 0);
            SetColumn(textBox, 0);
            SetRowSpan(textBox, 2);
            Children.Add(textBox);
        }

        private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if ((e.Text.Equals(".") && !textBox.Text.Contains(".") && allowDecimals)
                || IsDigit(e.Text) && (!allowNegatives || !textBox.Text.Contains("-"))
                || (e.Text.Equals("-") && allowNegatives && (textBox.Text.Equals("") || textBox.CaretIndex == 0) && !textBox.Text.Contains("-"))
                || IsDigit(e.Text) && allowNegatives && textBox.Text.Contains("-") && textBox.CaretIndex != 0
                ) { }
            else
            {
                e.Handled = true;
            }
        }

        private bool IsDigit(string x)
        {
            if (x.Length > 1) return false;
            if (useHex)
            {
                return int.TryParse(x, NumberStyles.HexNumber, new CultureInfo("en-US"), out int result);
            }
            else
            {
                return int.TryParse(x, out int result2);
            }
        }

        private void UpButton_Click(object sender, RoutedEventArgs e)
        {
            decimal oldValue = allowDecimals ? decimal.Parse(textBox.Text) : long.Parse(textBox.Text);
            textBox.Text = (oldValue + 1).ToString();
        }

        private void DownButton_Click(object sender, RoutedEventArgs e)
        {
            decimal oldValue = allowDecimals ? decimal.Parse(textBox.Text) : long.Parse(textBox.Text);
            textBox.Text = allowNegatives ? (oldValue - 1).ToString() : oldValue - 1 < 0 ? oldValue.ToString() : (oldValue - 1).ToString();
        }
    }
}
