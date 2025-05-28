using System.Collections.Generic;
using System.Threading.Tasks;
using static Olimpo.NavigationManager.NavigationManager;

namespace Olimpo.NavigationManager;

public interface INavigationManager
{
    string LastShownView { get; }

    void RegisterNavigatableView(INavigatableView navigatable);

    Task<bool> NavigateAsync(string viewToNavigate, IDictionary<string, object> parameters = null);

    Task<bool> NavigateAsync(NavigationOptions options);

    Task<bool> GoBackAsync();
}
