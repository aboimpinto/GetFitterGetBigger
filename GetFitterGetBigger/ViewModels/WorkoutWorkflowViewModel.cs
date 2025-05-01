using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using Olimpo;
using Olimpo.NavigationManager;
using ReactiveUI;

namespace GetFitterGetBigger.ViewModels;

public partial class WorkoutWorkflowViewModel : ViewModelBase, ILoadableViewModel
{
    private int _seconds = 0;
    private int _minutes = 0;
    private int _hour = 0;

    [ObservableProperty]
    private string _workoutTime = string.Empty;

    public Task LoadAsync(IDictionary<string, object>? parameters = null)
    {
        this.WorkoutTime = $"{this._minutes.ToString("00")}:{this._seconds.ToString("00")}";

        var loop = Observable
            .Interval(TimeSpan.FromSeconds(1))
            .ObserveOn(RxApp.MainThreadScheduler);

        loop.Subscribe(x => 
        {
            this._seconds ++;

            if (this._seconds > 59)
            {
                this._seconds = 0;
                this._minutes ++;
            }

            if (this._minutes > 59)
            {
                this._minutes = 0;
                this._hour ++;
            }

            this.WorkoutTime = this._hour switch
            {
                0 => $"{this._minutes.ToString("00")}:{this._seconds.ToString("00")}",
                _ => $"{this._hour.ToString("00")}:{this._minutes.ToString("00")}:{this._seconds.ToString("00")}"
            };
        });

        return Task.CompletedTask;
    }
}
