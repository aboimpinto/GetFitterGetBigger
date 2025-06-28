# Olimpo.NavigationManager
Purpose: Facilitates navigation between different views within the application using a ViewModel-first approach. This manager handles not only forward navigation but also provides **advanced, conditional back navigation capabilities**, allowing for complex user flow management, history manipulation, and context-aware back actions.

Key Components:

* `INavigationManager`: Interface defining core navigation functions, including forward and advanced back navigation.
* `NavigationManager`: Implements `INavigationManager`, handling all navigation logic, history management, and conditional back navigation rules.
* `ViewLocator`: Dynamically creates Views based on ViewModel type, following the convention `MyViewModel` -> `MyView`.
* `ViewModelBase`: Base class for ViewModels, inheriting from `ObservableObject` for data binding support.
* `ILoadableViewModel`: Interface with a `LoadAsync` method, called before navigation (both forward and when restoring a ViewModel on back navigation) for ViewModel initialization and state loading.
* `NavigationHistoryEntry`: A class that encapsulates all necessary information about each step in the navigation sequence, including view details, parameters, and metadata for conditional back navigation.
`NavigationOptions`: A class used to pass optional parameters to `NavigateAsync` to control how a navigation entry is added to the history and how it should behave during back navigation.
* `IDialogService` **(Dependency)**: An interface (assumed to be available via DI) used by ViewModels or navigation logic to display asynchronous confirmation dialogs (e.g., before discarding unsaved data during a back navigation).


## Usage
### Registration
To register `Olimpo.NavigationManager` with the `ServiceCollection`, use the `AddNavigationManager` extension method provided by the `NavigationManagerServiceCollectionExtensions` class.

To make a ViewModel navigatable, you must register it in the ServiceCollection with a string key. This string key is used for navigation calls such as NavigateAsync("AnotherViewModel").

Here's an example of how Navigatable ViewModels are typically registered using `ServiceCollection`:
```csharp
serviceCollection.AddScoped<ViewModelBase, ExampleViewModel>("ExampleViewModel");
```


### Navigation (`NavigateAsync`)

Navigation is performed by calling `NavigateAsync` on the `INavigationManager` instance with a string key that matches one of the registered ViewModel string keys.

**Enhancement:** The `NavigateAsync` method can now accept an optional `NavigationOptions` parameter. This allows the calling ViewModel to provide metadata for the navigation entry, influencing its behavior in the history stack and during back navigation.

```csharp
public class NavigationOptions
{
    public string ViewName { get; set; } = string.Empty;
    public IDictionary<string, object> Parameters { get; set;  } = new Dictionary<string, object>();
    public bool AddToHistory { get; set; } = true;
    public string? EntryPointIdentifier { get; set; } = null;
    public string? CustomBackNavigationTarget { get; set; } = null;
    public bool SkipInBackNavigation { get; set; } = false;
    public Func<Task<bool>>? RequiresConfirmationOnBack { get; set; } = null;
    // This delegate, if provided (typically by the ViewModel being navigated to),
    // will be invoked by the NavigationManager before popping this entry
    // or entries above it during a GoBackAsync operation.
    // It allows the ViewModel to run custom logic, like showing a confirmation dialog.
}

// Example usage in a ViewModel
public class MyViewModel : ViewModelBase, ILoadableViewModel
{
    private readonly INavigationManager _navigationManager;

    public MyViewModel(INavigationManager navigationManager)
    {
        this._navigationManager = navigationManager;
    }

    public async Task LoadAsync(IDictionary<string, object>? parameters = null)
    {
        // Perform any initialization here
    }

    public void NavigateToAnotherPageWithCustomBackBehavior()
    {
        var options = new NavigationOptions
        {
            // Example: If going back from "CriticalEditView", skip it and go to "OverviewView"
            CustomBackNavigationTarget = "OverviewView",
            RequiresConfirmationOnBack = async () =>
            {
                // This lambda would typically use an IDialogService
                // bool confirmed = await _dialogService.ShowConfirmationAsync("Unsaved Changes", "Discard changes?");
                // return confirmed;
                return true; // Placeholder
            }
        };
        this._navigationManager.NavigateAsync("CriticalEditView", parameters: null, options: options);
    }

    public void NavigateToNormalPage()
    {
        this._navigationManager.NavigateAsync("ExampleViewModel");
    }
}

public class ExampleViewModel : ViewModelBase, ILoadableViewModel
{
    public async Task LoadAsync(IDictionary<string, object>? parameters = null) { /*... */ }
    // Another ViewModel
}
```

### Advanced Back Navigation
The `NavigationManager` now implements a sophisticated back navigation system, moving beyond simple LIFO stack popping.

#### Navigation History Stack
* The `NavigationManager` maintains an internal `List<NavigationHistoryEntry>` to store the sequence of navigated views.
* Each `NavigationHistoryEntry` object contains:
    * `ViewName (string)`: The key of the view/ViewModel.
    * `ViewModelStateType (Type)`: The concrete type of the ViewModel.
    * `NavigationParameters (IDictionary<string, object>?)`: Parameters passed during the NavigateAsync call, used for rehydrating ViewModel state.
    * `EntryPointIdentifier (string?)`: An optional tag identifying the context or path through which this view was entered (e.g., "FromDashboard", "FromWorkoutList").
    * `CustomBackNavigationTarget (string?)`: An optional, pre-defined target view name for back navigation. If set, GoBackAsync will attempt to navigate to this view, potentially popping multiple entries.
    * `SkipInBackNavigation (bool)`: If true, this entry will be automatically skipped (removed from history without being displayed) during a standard "GoBack" operation, unless it's the explicit target.
    * `RequiresConfirmationOnBack (Func<Task<bool>>?)`: A delegate that, if provided, will be invoked by `NavigationManager.GoBackAsync()`. This allows the associated ViewModel to prompt the user for confirmation (e.g., via an `IDialogService`) before the navigation proceeds, especially if data might be lost. If the delegate returns `false`, the back navigation is aborted.

#### `GoBackAsync()` Method
* A new method `Task<bool> GoBackAsync(object? parameter = null)` is available on `INavigationManager`.
* This method is responsible for:
    1. Checking if back navigation is possible (`CanGoBack` property).
    2. Consulting the metadata of the current and previous `NavigationHistoryEntry` items (and potentially a set of predefined rules or strategies) to determine the actual target view for back navigation.
    3. Invoking any `RequiresConfirmationOnBack` delegate for the current entry or entries being popped. If confirmation fails, back navigation is aborted.
    4. Updating the internal history stack by removing the current entry and any entries that are being skipped (implementing "popUpTo" or "skip" logic).
    5. Resolving and loading the target ViewModel (using `ILoadableViewModel` with its stored `NavigationParameters`).
    6. Displaying the target view.
    7. Returns `true` if navigation was successful, `false` otherwise (e.g., if confirmation was denied or if `CanGoBack` is false).

#### Conditional Back Navigation Logic
The `GoBackAsync()` method uses the metadata stored in `NavigationHistoryEntry` objects to implement conditional logic:

* `CustomBackNavigationTarget`: If the current history entry has this property set, `GoBackAsync` will attempt to find this target in the history and pop all intermediate entries.
* `SkipInBackNavigation`: Entries marked with this flag will be removed from the history during a pop sequence without being activated, unless they are the explicit target.
* `EntryPointIdentifier`: This can be used by custom rules within `NavigationManager` to decide the back navigation path based on how a particular view was reached.

This allows for scenarios such as:
* `A -> B -> C`: Back from `C` goes to `B`.
* `A -> B (skip) -> C`: Back from `C` goes to `A` (`B` is popped from history without being shown).
* `A -> D -> E (custom_target=A)`: Back from `E` goes to `A` (`D` is popped).


#### Asynchronous Confirmation Dialogs
* The `RequiresConfirmationOnBack` delegate in `NavigationHistoryEntry` allows ViewModels to inject custom confirmation logic into the back navigation flow.
* This delegate is expected to be asynchronous (`Func<Task<bool>>`).
* Typically, the implementation of this delegate within a ViewModel would use an injected `IDialogService` to show a confirmation dialog to the user (e.g., "Discard unsaved changes?").
* The `NavigationManager` awaits this delegate. If it returns `false`, `GoBackAsync` aborts the back navigation.

#### Handling System/Physical Back Button
ViewModels can still influence or override the system back button behavior by implementing the `IHandlesBackButton` interface. The application's shell or platform-specific code is responsible for coordinating this.

##### Implementing `IHandlesBackButton` (Optional Override)

A ViewModel that needs to completely customize or intercept the back button behavior before the NavigationManager's logic takes over can implement `IHandlesBackButton`.

* `Task<bool> OnBackButtonPressedAsync()`: This method is called by the application's platform code when the system back button is pressed and the current ViewModel (obtained from `INavigationManager.CurrentOperation`) implements this interface.
    * Return `true` if the back navigation was fully handled by the ViewModel (e.g., it showed its own dialog and decided not to navigate, or performed a custom navigation). No further action regarding back navigation should be taken by the platform code.
    * Return false if the ViewModel did not handle the back press or wishes to allow the NavigationManager's default back navigation logic to proceed.

```csharp
public interface IHandlesBackButton
{
    Task<bool> OnBackButtonPressedAsync();
}

// Example ViewModel
public class WorkoutInProgressViewModel : ViewModelBase, ILoadableViewModel, IHandlesBackButton
{
    private readonly INavigationManager _navigationManager;
    private readonly IDialogService _dialogService; // Assuming injected

    public WorkoutInProgressViewModel(INavigationManager navigationManager, IDialogService dialogService)
    {
        _navigationManager = navigationManager;
        _dialogService = dialogService;
    }

    public Task LoadAsync(IDictionary<string, object>? parameters = null) { /*... */ return Task.CompletedTask; }

    public async Task<bool> OnBackButtonPressedAsync()
    {
        // ViewModel-specific check: if a workout is running, prompt the user.
        if (IsWorkoutRunning()) // Some internal state
        {
            bool discard = await _dialogService.ShowConfirmationAsync("Workout Active", "Stop current workout and go back?");
            if (discard)
            {
                StopWorkout(); // Perform cleanup
                // Allow NavigationManager to proceed with its default back logic
                // by returning false. The RequiresConfirmationOnBack delegate on the
                // NavigationHistoryEntry for this ViewModel could also handle this,
                // this is just an alternative for ViewModel-first control.
                return false;
            }
            else
            {
                return true; // Workout not stopped, back navigation handled (aborted by ViewModel)
            }
        }
        return false; // No special handling, let NavigationManager.GoBackAsync proceed.
    }
}
```

##### Connecting to System Back Button in Platform Code

Your application's platform-specific code (e.g., in `MainActivity.cs` for Android, or `App.axaml.cs` / relevant Window for desktop/mobile apps) needs to:

* Capture the system back button event.
* Obtain the `INavigationManager` instance.
* Access `navigationManager.CurrentOperation` to get the current `ViewModelBase`.
* Check if the current ViewModel implements `IHandlesBackButton`.
* If it does, `await` its `OnBackButtonPressedAsync()` method.
    * If `OnBackButtonPressedAsync()` returns `true`, the back action is considered handled.
    * If `OnBackButtonPressedAsync()` returns `false` (or the ViewModel doesn't implement the interface), then the platform code should call await `navigationManager.GoBackAsync()`.
    > Note: Proper asynchronous handling in platform-specific back button handlers (which might be `void` like Android's `OnBackPressed`) requires care.


##### How it Works (Updated Flow)
1. Views are associated with ViewModels.
2. `NavigationManager.NavigateAsync(viewKey, parameters, options)` is called:
    * The target ViewModel is resolved via DI.
    * If the ViewModel implements `ILoadableViewModel`, its `LoadAsync(parameters)` method is called.
    * A `NavigationHistoryEntry` is created, incorporating data from `options` (like `CustomBackNavigationTarget`, `RequiresConfirmationOnBack` delegate, etc.) and added to the history stack if `options.AddToHistory` is true.
    * The `ViewLocator` creates the corresponding View, and it's displayed with the ViewModel as its `DataContext`.

3. When a system back button press is detected by the platform code (e.g., Activity, Window):
    * a. The platform code retrieves the `INavigationManager` instance.
    * b. It gets the `currentViewModel = navigationManager.CurrentOperation`.
    * c. It checks if `currentViewModel implements IHandlesBackButton`.
        * If yes, it calls `bool handledByViewModel = await ((IHandlesBackButton)currentViewModel).OnBackButtonPressedAsync();`.
        * If `handledByViewModel` is `true`, the back action is complete.
        * If `handledByViewModel` is `false` (or the interface is not implemented), the platform proceeds to the next step.
    * d. The platform code calls `await navigationManager.GoBackAsync()`.
        * `GoBackAsync` consults the history stack and NavigationHistoryEntry metadata.
        * It invokes any `RequiresConfirmationOnBack` delegate. If confirmation fails, `GoBackAsync` returns `false`, and navigation is aborted.
        * It pops the current entry and any intermediate "skipped" entries from the history.
        * It resolves the new target ViewModel, calls its `LoadAsync` with stored parameters, and displays the associated view.
        * `GoBackAsync` returns `true` if successful, `false` otherwise.
    * e. If `navigationManager.GoBackAsync()` returns `false` (and was not handled by `IHandlesBackButton`), it might indicate the history stack is empty or navigation was aborted. The platform may then decide to close the app or perform other default actions.
