## Olimpo.NavigationManager

**Purpose:** Facilitates navigation between different views within the application, using a ViewModel-first approach.

**Key Components:**

*   `INavigationManager`: Interface defining core navigation functions.
*   `NavigationManager`: Implements `INavigationManager`, handling navigation logic.
*   `INavigatableView`: Interface for Views to register with the `NavigationManager`.
*   `ViewLocator`: Dynamically creates Views based on ViewModel type, following the convention `MyViewModel` -> `MyView`.
*   `ViewModelBase`: Base class for ViewModels, inheriting from `ObservableObject` for data binding support.
*   `ILoadableViewModel`: Interface with a `LoadAsync` method, called before navigation for ViewModel initialization.

## Usage

### Registration

To register `Olimpo.NavigationManager` with the `ServiceCollection`, use the `AddNavigationManager` extension method provided by the `NavigationManagerServiceCollectionExtensions` class.


### Registering Navigatable ViewModels

To make a `ViewModel` navigatable, you must register it in the `ServiceCollection` with a string key. This string key is used for navigation calls such as `NavigateAsync("AnotherViewModel")`.

Here's an example of how Navigatable ViewModels are typically registered using `ServiceCollection`:

```csharp
    serviceCollection.AddScoped<ViewModelBase, ExampleViewModel>("ExampleViewModel");
```

### Navigation

Navigation is performed by calling `NavigateAsync` with a string key that matches one of the registered ViewModel string keys.

```csharp
public class MyViewModel : ViewModelBase, ILoadableViewModel
{
    private readonly INavigationManager _navigationManager;

    public MyViewModel(INavigationManager navigationManager)
    {
        this._navigationManager = navigationManager;
    }

    public async Task LoadAsync()
    {
        // Perform any initialization here
    }

    public void NavigateToAnotherPage()
    {
        this._navigationManager.NavigateAsync("ExampleViewModel");
    }
}

public class ExampleViewModel : ViewModelBase
{
    // Another ViewModel
}
```

### Handling Back Button Navigation
ViewModels can implement custom logic for the physical back button press (e.g., on Android devices or a UWP back button) by implementing the `IHandlesBackButton` interface. The application's shell or platform-specific code is responsible for checking this interface on the `CurrentViewModel` and coordinating the response.

1. Implementing `IHandlesBackButton`

A ViewModel that needs to customize the back button behavior should implement the `IHandlesBackButton` interface.

* `Task<bool> OnBackButtonPressedAsync()`: This method is called by the application's platform code when the system back button is pressed and the current ViewModel implements this interface.
    * Return `true` if the back navigation was fully handled by the ViewModel and no further (default) back navigation should occur.
    * Return `false` if the ViewModel did not handle the back press (or wishes to allow default back navigation to proceed via `INavigationManager.GoBackAsync()`).

```csharp
public interface IHandlesBackButton
{
    /// <summary>
    /// Called when the system back button is pressed, allowing the ViewModel to handle it.
    /// </summary>
    /// <returns>
    /// Task
    /// </returns>
    Task<bool> OnBackButtonPressedAsync();
}

// Example ViewModel
public class ConfirmExitViewModel : 
    ViewModelBase, 
    ILoadableViewModel, 
    IHandlesBackButton
{
    private readonly INavigationManager _navigationManager;

    public ConfirmExitViewModel(INavigationManager navigationManager)
    {
        this._navigationManager = navigationManager;
    }

    public Task LoadAsync()
    {
        // Load data
        return Task.CompletedTask;
    }

    public async Task OnBackButtonPressedAsync()
    {
        // At this point check if the Back is possible like, by example, if there is unsaved changes.

        await this._navigationManager.NavigateAsync("BackViewModel");
    }
}
```

2. Connecting to System Back Button in Platform Code

Your application's platform-specific code (e.g., in `MainActivity.cs` for Android, or `App.axaml.cs` / relevant `Window` for desktop/mobile Avalonia apps) needs to:

* Capture the system back button event.
* Obtain the `INavigationManager` instance.
* Access `navigationManager.CurrentViewModel`.
* Check if `CurrentViewModel` implements `IHandlesBackButton`.
* If it does, await its `OnBackButtonPressedAsync()` method.

> **Note**: Proper asynchronous handling in `OnBackPressed` (which is void) on Android requires care. You might launch a `Task` and not await it directly, or use an event-based mechanism if `OnBackButtonPressedAsync` involves lengthy operations or UI interactions that shouldn't block `OnBackPressed`. For simple logic, await might appear to work but can have subtleties.

**How it Works:**

1.  Views are associated with ViewModels.
2.  `NavigationManager` navigates to a specified ViewModel.
3.  `ViewLocator` creates the corresponding View.
4.  If the ViewModel implements `ILoadableViewModel`, `LoadAsync` is called.
5. When a system back button press is detected by the platform code (e.g., Activity, Window): 
    * **a.** The platform code retrieves the `INavigationManager` instance and its `CurrentViewModel`. 
    * **b.** It checks if the `CurrentViewModel` implements `IHandlesBackButton`. 
    * **c.** If it does, the platform code calls `OnBackButtonPressedAsync()` on the Current ViewModel that navigates to a defined ViewModel by the developer.
    * **d.** If the `CurrentViewModel` does not implement `IHandlesBackButton` the behaviour is the default for this case.
    * **e.** The `NavigationManager`'s `HandleBackButtonAsync` method will return `false` if the `CurrentViewModel` does not implement `IHandlesBackButton`, indicating that the back button press was not handled.
