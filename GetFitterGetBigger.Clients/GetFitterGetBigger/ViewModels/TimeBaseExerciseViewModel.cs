using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using Olimpo;
using Olimpo.NavigationManager;
using GetFitterGetBigger.Events;
using GetFitterGetBigger.Model;

namespace GetFitterGetBigger.ViewModels;

public partial class TimeBaseExerciseViewModel :
    ViewModelBase,
    ILoadableViewModel,
    ISkipableExercise
{
    private readonly WorkoutStep _workoutStep;
    private readonly IEventAggregator _eventAggregator;
    private IDisposable _timerSubscription;

    [ObservableProperty]
    private string _workoutName = string.Empty;

    [ObservableProperty]
    private string _roundInfo = string.Empty;

    [ObservableProperty]
    private string _setInfo = string.Empty;

    [ObservableProperty]
    private string _exerciseInfo = string.Empty;

    [ObservableProperty]
    private string _time = string.Empty;

    public TimeBaseExerciseViewModel(WorkoutStep workoutStep, IEventAggregator eventAggregator)
    {
        this._workoutStep = workoutStep;
        this._eventAggregator = eventAggregator;

        this.WorkoutName = this._workoutStep.WorkoutCaption;
        this.RoundInfo = this._workoutStep.RoundInfo;
        this.SetInfo = this._workoutStep.SetInfo;
        this.ExerciseInfo = this._workoutStep.ExerciseInfo;

        this.Time = string.Empty;
    }

    public Task LoadAsync(IDictionary<string, object>? parameters = null)
    {
        var timeInSeconds = ((TimeBaseExerciseWorkoutRound)this._workoutStep.Exercise).TimeInSeconds;
        var currentTime = timeInSeconds;

        var minutes = currentTime / 60;
        var seconds = currentTime % 60;
        Time = string.Format("{0:00}:{1:00}", minutes, seconds);

        this._timerSubscription = Observable.Interval(TimeSpan.FromSeconds(1))
            .TakeWhile(_ => currentTime >= 0)
            .Subscribe(async _ =>
            {
                var minutes = currentTime / 60;
                var seconds = currentTime % 60;
                Time = string.Format("{0:00}:{1:00}", minutes, seconds);
                currentTime--;

                if (currentTime < 0)
                {
                    await this._eventAggregator.PublishAsync(new TimedSetFinishedEvent());
                    this._timerSubscription.Dispose();
                }
            });

        return Task.CompletedTask;
    }

    public Task SkipIt()
    {
        this._timerSubscription.Dispose();
        return Task.CompletedTask;
    }
}
