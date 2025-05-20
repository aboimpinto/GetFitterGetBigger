using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using GetFitterGetBigger.Events;
using Olimpo;
using Olimpo.NavigationManager;

namespace GetFitterGetBigger.ViewModels;

public partial class CountdownViewModel(IEventAggregator eventAggregator) : 
    ViewModelBase,
    ILoadableViewModel
{
    private readonly IEventAggregator _eventAggregator = eventAggregator;
    private int _remainingSeconds = 4;
    private IDisposable _countdownSubscription;

    [ObservableProperty]
    private string _countdownString = string.Empty;

    [ObservableProperty]
    private bool _animationOn;

    public Task LoadAsync(IDictionary<string, object>? parameters = null)
    {
        this._remainingSeconds = 4;
        this.CountdownString = (this._remainingSeconds - 1).ToString();
        this.AnimationOn = true;

        this._countdownSubscription = Observable
            .Interval(TimeSpan.FromSeconds(1))
            .Subscribe(async _ =>
            {
                this._remainingSeconds--;
                if (this._remainingSeconds <= 0)
                {
                    this._countdownSubscription.Dispose();
                    this.CountdownString = string.Empty;

                    await this._eventAggregator.PublishAsync(new InitialCountDownFinishedEvent());
                }
                else if (this._remainingSeconds == 1)
                {
                    this.CountdownString = "GO!";
                }
                else
                {
                    this.CountdownString = (this._remainingSeconds - 1).ToString();
                }
            });

        return Task.CompletedTask;
    }
}
