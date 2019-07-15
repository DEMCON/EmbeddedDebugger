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
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace EmbeddedDebugger.View.CustomControls
{
    public class EmbeddedTerminal : TextBox
    {
        #region fields
        private StringBuilder log;
        private bool logEndsWithNewline;
        private StringBuilder inputBuffer;
        private readonly string prefixTo = "To:\t";
        private readonly string prefixFrom = "From:\t";
        private bool ctrlPressed = false;
        #endregion

        #region EventHandlers
        public event EventHandler<string> NewMessage = delegate { };
        #endregion

        public EmbeddedTerminal()
        {
            SetProperties();
            log = new StringBuilder();
            log.Append(prefixFrom);
            logEndsWithNewline = false;
            inputBuffer = new StringBuilder();

            UndoLimit = 1;

            KeyUp += EmbeddedTerminal_KeyUp;
            KeyDown += EmbeddedTerminal_KeyDown;
            MouseWheel += EmbeddedTerminal_MouseWheel;
            PreviewKeyDown += EmbeddedTerminal_PreviewKeyDown;
            PreviewTextInput += EmbeddedTerminal_PreviewTextInput;

            Refresh();
        }

        static readonly private int LogMaxLength = 1000000;

        public void AddResponse(string input)
        {
            input = input.Replace("\n", "\n\t");

            lock (log)
            {
                log.Append(input);

                if (log.Length > LogMaxLength)
                    log.Remove(0, log.Length - LogMaxLength);

                logEndsWithNewline = false;
            }

            DoRefresh();
        }

        private bool doRefresh = false;
        private async void DoRefresh()
        {
            if (doRefresh)
                return;

            doRefresh = true;

            await Task.Delay(100);
            if (doRefresh)
            {
                doRefresh = false;
                Dispatcher.Invoke(Refresh);
            }
        }

        public void Refresh()
        {
            string text;
            lock(log)
            {
                text = log.ToString();
            }

            if (!logEndsWithNewline)
                text += "\n";

            text += prefixTo + inputBuffer.ToString();

            Text = text;
            SelectionStart = text.Length;
            ScrollToEnd();
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

        #region EventHandlers
        private void EmbeddedTerminal_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            inputBuffer.Append(e.Text);
        }

        private void EmbeddedTerminal_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Back)
            {
                if (inputBuffer.Length > 0)
                {
                    inputBuffer.Remove(inputBuffer.Length - 1, 1);
                }
                e.Handled = true;
            }
            else if (e.Key == Key.Enter)
            {
                if (inputBuffer.Length == 0)
                {
                    e.Handled = true;
                    return;
                }

                string input = inputBuffer.ToString();

                lock (log)
                {
                    if (!logEndsWithNewline)
                        log.Append("\n");

                    log.Append(prefixTo + input + "\n" + prefixFrom);
                    logEndsWithNewline = false;
                }

                NewMessage(this, input);
                inputBuffer.Clear();
                e.Handled = true;
            }
            else if (e.Key == Key.Delete)
            {
                e.Handled = true;
            }
            else if (e.Key == Key.Space)
            {
                inputBuffer.Append(' ');
            }
            Refresh();
        }

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
