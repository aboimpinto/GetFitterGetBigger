Create a new View (Page/Screen) with its ViewModel following MVVM patterns for multi-platform development.

Instructions:
1. Ask for the view name, purpose, and target platforms
2. Create the ViewModel with proper MVVM patterns
3. Create the View with appropriate XAML/UI markup
4. Set up data binding and commands
5. Configure navigation if needed
6. Add platform-specific customizations if required
7. Create unit tests for the ViewModel

ViewModel requirements:
- Inherit from BaseViewModel or implement INotifyPropertyChanged
- Use ObservableCollection for lists
- Implement ICommand for user actions
- Handle loading states and errors
- Support offline scenarios if applicable
- No UI-specific code (testable)

View requirements:
- XAML for Xamarin.Forms/MAUI (shared UI)
- Platform-specific UI files if needed
- Proper data binding syntax
- Loading and error state UI
- Responsive layout for different screen sizes
- Accessibility attributes
- Platform-specific styling

Code structure:
```
/ViewModels/
  └── [Feature]ViewModel.cs
/Views/
  └── [Feature]Page.xaml
  └── [Feature]Page.xaml.cs
/Views/iOS/ (if platform-specific needed)
  └── [Feature]Page.xaml
/Views/Android/ (if platform-specific needed)
  └── [Feature]Page.xaml
```

The view should work seamlessly across all target platforms while respecting platform conventions.