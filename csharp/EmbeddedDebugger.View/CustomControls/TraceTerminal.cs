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
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using EmbeddedDebugger.DebugProtocol.Messages;

namespace EmbeddedDebugger.View.CustomControls
{
    public class TraceTerminal : TextBox
    {
        #region fields
        private bool ctrlPressed;
        private bool multipleNodes;
        #endregion

        #region Properties
        public bool MultipleNodes { get => multipleNodes; set => multipleNodes = value; }      
        #endregion

        public TraceTerminal()
        {
            SetProperties();
        }

        public void AddMessage(TraceMessage message)
        {
            Dispatcher.Invoke(delegate
            {
                StringBuilder builder = new StringBuilder();
                builder.Append(Text);
                if (multipleNodes)
                {
                    builder.Append($"{message.NodeID}\t");
                }
                builder.Append($"{message.TraceLevel.ToString()}\t{message.Message}\n");
                Text = builder.ToString();
                ScrollToEnd();
            });
        }

        public void SetMessages(List<TraceMessage> messages)
        {
            Text = "";
            foreach(TraceMessage msg in messages)
            {
                AddMessage(msg);
            }
        }

        private void SetProperties()
        {
            Background = Brushes.Black;
            Foreground = Brushes.White;
            Padding = new System.Windows.Thickness(3);
            AcceptsReturn = true;
            AllowDrop = false;
            HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled;
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
            TextWrapping = System.Windows.TextWrapping.Wrap;
        }

        #region Delegates
        private void EmbeddedTerminal_MouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            if (ctrlPressed)
            {
                if (e.Delta > 0)
                {
                    FontSize++;
                }
                else if (FontSize - 1 > 0)
                {
                    FontSize--;
                }
            }
        }

        private void EmbeddedTerminal_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.LeftCtrl || e.Key == Key.RightCtrl)
            {
                ctrlPressed = true;
            }
            if (e.Key == Key.Delete)
            {
                e.Handled = true;
            }
        }

        private void EmbeddedTerminal_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.LeftCtrl || e.Key == Key.RightCtrl)
            {
                ctrlPressed = false;
            }
        }
        #endregion
    }
}
