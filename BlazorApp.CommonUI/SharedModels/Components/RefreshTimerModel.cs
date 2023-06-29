using System;
using System.Timers;
using Microsoft.AspNetCore.Components;

namespace BlazorApp.CommonUI.SharedModels.Components;

public class RefreshTimerModel : ComponentBase, IDisposable
{
    [Parameter] public ElapsedEventHandler ElapsedEvent { get; set; }

    [Parameter] public string Label { get; set; } = "Refresh Interval: ";

    [Parameter] public string Text { get; set; } = "Seconds";

    [Parameter] public double Interval { get; set; } = 5;

    [Parameter] public bool StartEnabled { get; set; }

    protected Timer TimerInterval { get; set; } = new();

    public void Dispose()
    {
        TimerInterval.Elapsed -= ElapsedEvent;
        TimerInterval.Dispose();
    }

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
}