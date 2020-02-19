using EmbeddedDebugger.Model;
using System;
using System.Timers;

namespace EmbeddedDebugger.ViewModel
{
    public class ViewModelManager
    {
        private int mediumCounter, lowCounter;

        //TODO: Remove public access
        public ModelManager ModelManager { get; }

        public SystemViewModel SystemViewModel { get; }

        public PlottingViewModel PlottingViewModel { get; }

        public event EventHandler RefreshHigh = delegate { };
        public event EventHandler RefreshMedium = delegate { };
        public event EventHandler RefreshLow = delegate { };


        public ViewModelManager()
        {
            this.ModelManager = new ModelManager();
            this.SystemViewModel = new SystemViewModel(this.ModelManager);
            this.PlottingViewModel = new PlottingViewModel(this.ModelManager);
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
