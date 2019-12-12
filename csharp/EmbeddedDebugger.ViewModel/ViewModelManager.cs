﻿using System;
using System.Timers;
using EmbeddedDebugger.Model;

namespace EmbeddedDebugger.ViewModel
{
    public class ViewModelManager
    {
        private System.Timers.Timer refreshTimer;
        private int mediumCounter, lowCounter;

        // TODO: Needs to be set to private, modelmanager should never ever be seen from the View
        public ModelManager ModelManager { get; }

        public SystemViewModel SystemViewModel { get; }

        public event EventHandler RefreshHigh = delegate { };
        public event EventHandler RefreshMedium = delegate { };
        public event EventHandler RefreshLow = delegate { };


        public ViewModelManager()
        {
            ModelManager = new ModelManager();
            SystemViewModel = new SystemViewModel(ModelManager);
            this.refreshTimer = new Timer
            {
                Interval = 100
            };
            this.refreshTimer.Elapsed += this.RefreshTimer_Elapsed;
            this.mediumCounter = 0;
            this.lowCounter = 0;
            this.refreshTimer.Start();
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
