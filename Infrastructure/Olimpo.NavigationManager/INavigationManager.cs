using System.Collections.Generic;
using System.Threading.Tasks;

namespace Olimpo.NavigationManager;

public interface INavigationManager
{
    string LastShownView { get; }

    void RegisterNavigatableView(INavigatableView navigatable);

    Task<bool> NavigateAsync(string viewToNavigate, IDictionary<string, object> parameters = null);
}
