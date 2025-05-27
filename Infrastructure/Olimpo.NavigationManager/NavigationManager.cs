using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Olimpo.NavigationManager;

public class NavigationManager : INavigationManager
{
    private INavigatableView _navigatableView;

    public string LastShownView { get; private set; }

    public void RegisterNavigatableView(INavigatableView navigatable)
    {
        this._navigatableView = navigatable;
    }

    public async Task<bool> NavigateAsync(string viewToNavigate, IDictionary<string, object>? parameters = null)
    {
        var options = new NavigationOptions
        {
            ViewName = viewToNavigate,
            Parameters = parameters ?? new Dictionary<string, object>()
        };

        return await this.NavigateAsync(options);
    }

    public async Task<bool> NavigateAsync(NavigationOptions options)
    {
        // TODO [AboimPinto] 04.03.20222: Would be nice to inject the weak references to the ViewModels and create one reference each time it is needed
        var viewModel = ServiceCollectionManager.ServiceProvider.GetService<ViewModelBase>(options.ViewName);

        if (viewModel is ILoadableViewModel)
        {
            await ((ILoadableViewModel)viewModel).LoadAsync(options.Parameters);
        }

        this._navigatableView.CurrentOperation = viewModel;
        this.LastShownView = options.ViewName;

        return true;
    }

    public class NavigationOptions
    {
        public string ViewName { get; set; } = string.Empty;
        public IDictionary<string, object> Parameters { get; set;  } = new Dictionary<string, object>();
        public bool AddToHistory { get; set; } = true;
        public string? EntryPointIdentifier { get; set; } = null;
        public string? CustomBackNavigationTarget { get; set; } = null;
        public bool SkipInBackNavigation { get; set; } = false;
        public Func<Task<bool>>? RequiresConfirmationOnBack { get; set; } = null;
        // Potentially a flag to indicate if the ViewModel instance should be cached
        // public bool CacheViewModelInstance { get; set; } = false;
    }
}
