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
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EmbeddedDebugger.View.CustomComponents
{
    public partial class EmbeddedTerminal : TextBox
    {
        private StringBuilder buffer;
        private string prefix = "To:\t";

        private bool ctrlPressed = false;
        private StringBuilder theRest;

        public event EventHandler<string> NewMessage = delegate { };

        public EmbeddedTerminal(IContainer container)
        {
            container.Add(this);
            InitializeComponent();
            buffer = new StringBuilder();
            theRest = new StringBuilder();
            Refresh();
        }

        public void AddResponse(string input)
        {
            theRest.Append("From:\t");
            theRest.Append(input);
            theRest.Append("\r\n");
            if (InvokeRequired)
            {
                Invoke(new MethodInvoker(() => Refresh()));
            }
            else
            {
                Refresh();
            }
        }

        private void EmbeddedTerminal_KeyUp(object sender, KeyEventArgs e)
        {
            ctrlPressed = e.Control;
        }

        private void EmbeddedTerminal_KeyDown(object sender, KeyEventArgs e)
        {
            ctrlPressed = e.Control;
            if (e.KeyCode == Keys.Delete)
            {
                e.Handled = true;
            }
        }

        private void EmbeddedTerminal_MouseWheel(object sender, MouseEventArgs e)
        {
            if (ctrlPressed)
            {
                if (e.Delta > 0)
                {
                    Font = new Font(Font.FontFamily, Font.SizeInPoints + 1);
                }
                else
                {
                    if (Font.SizeInPoints - 1 > 0)
                    {
                        Font = new Font(Font.FontFamily, Font.SizeInPoints - 1);
                    }
                    else
                    {
                        Font = new Font(Font.FontFamily, Font.SizeInPoints);
                    }
                }
            }
        }

        private void EmbeddedTerminal_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Back)
            {
                if (buffer.Length == 0)
                {
                    e.Handled = true;
                }
                else
                {
                    buffer.Remove(buffer.Length - 1, 1);
                    e.Handled = true;
                }
            }
            else if (e.KeyChar == (char)Keys.Enter)
            {
                if (buffer.Length == 0)
                {
                    e.Handled = true;
                    return;
                }
                theRest.Append(prefix);
                theRest.Append(buffer.ToString());
                theRest.Append("\r\n");

                NewMessage(this, Regex.Unescape(buffer.ToString()));
                buffer.Clear();
                e.Handled = true;
            }
            else if (e.KeyChar == (char)Keys.Delete)
            {

                e.Handled = true;
            }
            else
            {
                buffer.Append(e.KeyChar);
                e.Handled = true;
            }

            Refresh();
        }

        public override void Refresh()
        {
            Text = theRest.ToString() + prefix + (buffer.Length == 0 ? "" : buffer.ToString());
            SelectionStart = Text.Length;
            base.Refresh();
            ScrollToCaret();
        }
    }
}
