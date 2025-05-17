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
        // TODO [AboimPinto] 04.03.20222: Would be nice to inject the weak references to the ViewModels and create one reference each time it is needed
        var viewModel = ServiceCollectionManager.ServiceProvider.GetService<ViewModelBase>(viewToNavigate);

        if (viewModel is ILoadableViewModel)
        {
            await ((ILoadableViewModel)viewModel).LoadAsync(parameters);
        }

        this._navigatableView.CurrentOperation = viewModel;
        this.LastShownView = viewToNavigate;

        return true;
    }
}
