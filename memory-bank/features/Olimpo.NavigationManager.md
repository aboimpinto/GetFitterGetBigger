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

**How it Works:**

1.  Views are associated with ViewModels.
2.  `NavigationManager` navigates to a specified ViewModel.
3.  `ViewLocator` creates the corresponding View.
4.  If the ViewModel implements `ILoadableViewModel`, `LoadAsync` is called.
