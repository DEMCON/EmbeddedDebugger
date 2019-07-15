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
using System.Drawing;
using System.Windows.Forms;

namespace EmbeddedDebugger.View.CustomComponents
{
    partial class EmbeddedTerminal
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            this.Dock = DockStyle.Fill;
            this.AcceptsReturn = true;
            this.AcceptsTab = false;
            this.BackColor = Color.Black;
            this.ForeColor = Color.White;
            this.Multiline = true;
            this.ScrollBars = ScrollBars.Vertical;
            this.KeyDown += EmbeddedTerminal_KeyDown;
            this.KeyUp += EmbeddedTerminal_KeyUp;
            this.MouseWheel += EmbeddedTerminal_MouseWheel;
            this.KeyPress += EmbeddedTerminal_KeyPress;
        }
        #endregion
    }
}
