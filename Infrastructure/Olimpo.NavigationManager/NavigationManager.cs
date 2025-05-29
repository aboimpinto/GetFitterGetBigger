using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Olimpo.NavigationManager;

public class NavigationManager : INavigationManager
{
    private Stack<NavigationHistoryEntry> _navigationHistory = new();
    private INavigatableView _navigatableView;

    public string LastShownView { get; private set; }

    public void RegisterNavigatableView(INavigatableView navigatable)
    {
        this._navigatableView = navigatable;
    }

    public async Task<bool> GoBackAsync()
    {
        if (this._navigationHistory.Count > 1)
        {
            var previousEntry = this._navigationHistory.Peek();
            if (previousEntry.ViewName == LastShownView)
            {
                this._navigationHistory.Pop();
                previousEntry = this._navigationHistory.Peek();
                var navigationOptions = new NavigationOptions(
                    previousEntry.ViewName,
                    false,
                    true,
                    previousEntry.Parameters);
                await this.NavigateAsync(navigationOptions);
            }
            else
            {
                var navigationOptions = new NavigationOptions(
                    previousEntry.ViewName,
                    false,
                    true,
                    previousEntry.Parameters);
                await this.NavigateAsync(navigationOptions);
            }
            
            return true;
        }

        return false;
    }

    public async Task<bool> NavigateAsync(string viewToNavigate, IDictionary<string, object>? parameters = null)
    {
        var options = new NavigationOptions(
            viewToNavigate,
            true,
            false,
            parameters ?? new Dictionary<string, object>());

        return await this.NavigateAsync(options);
    }

    public async Task<bool> NavigateAsync(NavigationOptions options)
    {
        // TODO [AboimPinto] 04.03.20222: Would be nice to inject the weak references to the ViewModels and create one reference each time it is needed
        var viewModel = ServiceCollectionManager.ServiceProvider.GetService<ViewModelBase>(options.ViewName);

        if (viewModel is ILoadableViewModel model)
        {
            await model.LoadAsync(options.Parameters);
        }

        this._navigatableView.CurrentOperation = viewModel;
        this._navigatableView.WithBackTransaction = options.WithBackTransaction;
        this.LastShownView = options.ViewName;

        if (options.AddToHistory)
        {
            var navigationHistoryEntry = NavigationHistoryEntryHandler.CreateNew(options.ViewName, options.Parameters);

            if (this._navigationHistory.Any())
            {
                var currentHistoryEntry = this._navigationHistory.Peek();
                if (currentHistoryEntry.ViewName == options.ViewName)
                {
                    // DO NOTHING
                }
                else
                {
                    this._navigationHistory.Push(navigationHistoryEntry);
                }
            }
            else
            {
                this._navigationHistory.Push(navigationHistoryEntry);   
            }
        }

        return true;
    }

    public record NavigationOptions
    {
        public string ViewName { get; init; }
        public bool AddToHistory { get; init; }
        public bool WithBackTransaction { get; init; }
        public IDictionary<string, object> Parameters { get; init; }

        public NavigationOptions(
            string viewName,
            bool addToHistory = true,
            bool withBackTransaction = false)
            : this(viewName, addToHistory, withBackTransaction, new Dictionary<string, object>()) {}

        public NavigationOptions(
            string viewName,
            bool addToHistory,
            bool WithBackTransaction, 
            IDictionary<string, object> viewParameters)
        {
            this.ViewName = viewName;
            this.AddToHistory = addToHistory;
            this.WithBackTransaction = WithBackTransaction;
            this.Parameters = viewParameters;
        }
    }
}
