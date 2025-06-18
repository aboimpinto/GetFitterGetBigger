using System.Collections.Generic;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using Olimpo;
using Olimpo.NavigationManager;
using GetFitterGetBigger.Events;
using GetFitterGetBigger.Workflows;

namespace GetFitterGetBigger.ViewModels;

public partial class MainViewModel : 
    ViewModelBase,
    INavigatableView,
    ILoadableViewModel
{
    private readonly IInitializationWorkflow _initializationWorkflow;
    private readonly INavigationManager _navigationManager;
    private readonly IEventAggregator _eventAggregator;

    [ObservableProperty]
    public ViewModelBase _currentOperation;

    [ObservableProperty]
    public bool _withBackTransaction;

    public MainViewModel(
        IInitializationWorkflow initializationWorkflow,
        INavigationManager navigationManager,
        IEventAggregator eventAggregator)
    {
        this._initializationWorkflow = initializationWorkflow;
        this._navigationManager = navigationManager;
        this._eventAggregator = eventAggregator;

        this._navigationManager.RegisterNavigatableView(this);

        this.WithBackTransaction = false;
    }

    public async Task LoadAsync(IDictionary<string, object>? parameters = null)
    {
        await this._eventAggregator.PublishAsync(new StartBootstrappingEvent());
    }
}
