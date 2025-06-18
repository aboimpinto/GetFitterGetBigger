using System.Threading.Tasks;

namespace Olimpo.NavigationManager;

public interface IHandlesBackButton
{
    /// <summary>
    /// Called when the system back button is pressed, allowing the ViewModel to handle it.
    /// </summary>
    /// <returns>
    /// Task
    /// </returns>
    Task OnBackButtonPressedAsync();
}
