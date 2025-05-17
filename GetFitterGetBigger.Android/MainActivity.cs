using Avalonia;
using Avalonia.Android;
using Avalonia.Controls.ApplicationLifetimes;
using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using AndroidX.Annotations;
using Olimpo;
using Olimpo.NavigationManager;
using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;

namespace GetFitterGetBigger.Android;

[Activity(
    Label = "GetFitterGetBigger.Android",
    Theme = "@style/MyTheme.NoActionBar",
    Icon = "@drawable/icon",
    MainLauncher = true,
    ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize | ConfigChanges.UiMode)]
public class MainActivity : AvaloniaMainActivity<App>, INavigatableView
{
    private ViewModelBase _currentOperation;

    public ViewModelBase CurrentOperation
    {
        get => _currentOperation;
        set => _currentOperation = value;
    }

    protected override AppBuilder CustomizeAppBuilder(AppBuilder builder)
    {
        return base.CustomizeAppBuilder(builder)
            .WithInterFont();
    }

    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);
        Window.SetSoftInputMode(SoftInput.AdjustResize);
    }

    public override void OnBackPressed()
    {
        var navigationManager = ServiceCollectionManager.ServiceProvider.GetService<INavigationManager>();
        if (navigationManager != null)
        {
            // Try to get the ViewModel from the MainView's DataContext
            if (Avalonia.Application.Current?.ApplicationLifetime is ISingleViewApplicationLifetime singleViewLifetime)
            {
                if (singleViewLifetime.MainView != null)
                {
                    var currentViewModel = singleViewLifetime.MainView.DataContext as ViewModelBase;
                    System.Diagnostics.Debug.WriteLine($"Current ViewModel: {currentViewModel?.GetType().FullName}");

                    var currentOperation = ((INavigatableView)currentViewModel).CurrentOperation;
                    if (currentOperation is IHandlesBackButton handlesBackButton)
                    {
                        handlesBackButton.OnBackButtonPressedAsync();
                        return;
                    }
                    else
                    {
                        base.OnBackPressed();
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("MainView is null");
                    base.OnBackPressed();
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("Not a single view application lifetime");
                base.OnBackPressed();
            }
        }
        else
        {
            System.Diagnostics.Debug.WriteLine("NavigationManager is null");
            base.OnBackPressed();
        }

        
    }
}
