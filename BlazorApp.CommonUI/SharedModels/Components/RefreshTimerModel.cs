using Microsoft.AspNetCore.Components;
using System;
using System.Timers;

namespace BlazorApp.CommonUI.SharedModels.Components
{
    public class RefreshTimerModel : ComponentBase, IDisposable
    {
        [Parameter] public ElapsedEventHandler ElapsedEvent { get; set; }

        [Parameter] public string Label { get; set; } = "Refresh Interval: ";

        [Parameter] public string Text { get; set; } = "Seconds";

        [Parameter] public double Interval { get; set; } = 5;

        [Parameter] public bool StartEnabled { get; set; } = false;

        protected Timer TimerInterval { get; set; } = new Timer();

        protected override void OnInitialized()
        {

            TimerInterval.Enabled = StartEnabled;
            TimerInterval.Interval = Interval * 1000;
            TimerInterval.Elapsed += ElapsedEvent;
            base.OnInitialized();
        }

        protected void UpdateInterval(double newInterval)
        {
            Interval = newInterval;
            TimerInterval.Interval = newInterval * 1000;
        }

        public void Dispose()
        {
            TimerInterval.Elapsed -= ElapsedEvent;
            TimerInterval.Dispose();
        }
    }
}