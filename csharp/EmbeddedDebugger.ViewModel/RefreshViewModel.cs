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
using System.Timers;

namespace EmbeddedDebugger.ViewModel
{
    public class RefreshViewModel
    {       
        /// <summary>
        /// The logger for this class
        /// </summary>
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        private int mediumCounter, lowCounter;

        public event EventHandler RefreshHigh = delegate { };
        public event EventHandler RefreshMedium = delegate { };
        public event EventHandler RefreshLow = delegate { };

        public RefreshViewModel()
        {

            Timer refreshTimer = new Timer
            {
                Interval = 100
            };
            refreshTimer.Elapsed += this.RefreshTimer_Elapsed;
            this.mediumCounter = 0;
            this.lowCounter = 0;
            refreshTimer.Start();
        }

        private void RefreshTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            this.RefreshHigh(this, e);
            if (this.mediumCounter++ > 5)
            {
                this.mediumCounter = 0;
                this.RefreshMedium(this, e);
            }

            if (this.lowCounter++ > 10)
            {
                this.lowCounter = 0;
                this.RefreshLow(this, e);
            }
        }
    }
}
