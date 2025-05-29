using Avalonia;
using Avalonia.Android;
using Avalonia.Controls.ApplicationLifetimes;
using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using Olimpo.NavigationManager;
using System.Diagnostics;
using Android.Content;
using System;

namespace GetFitterGetBigger.Android;

[Activity(
    Label = "GetFitterGetBigger.Android",
    Theme = "@style/MyTheme.NoActionBar",
    Icon = "@drawable/icon",
    MainLauncher = true,
    ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize | ConfigChanges.UiMode)]
public class MainActivity : AvaloniaMainActivity<App>, INavigatableView
{
    private PowerManager.WakeLock wakeLock;
    private ViewModelBase _currentOperation;

    public ViewModelBase CurrentOperation
    {
        get => _currentOperation;
        set => _currentOperation = value;
    }
    public bool WithBackTransaction { get; set; }

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

    protected override void OnResume()
    {
        base.OnResume();
        try
        {
            PowerManager powerManager = (PowerManager)GetSystemService(Context.PowerService);
            wakeLock = powerManager?.NewWakeLock(WakeLockFlags.ScreenBright, "GetFitterGetBigger:WakeLock");
            wakeLock?.Acquire();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error acquiring WakeLock: {ex}");
            // Handle the exception appropriately, e.g., log it or display an error message
        }
    }

    protected override void OnPause()
    {
        base.OnPause();
        try
        {
            wakeLock?.Release();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error releasing WakeLock: {ex}");
            // Handle the exception appropriately
        }
    }

    public override void OnBackPressed()
    {
        IHandlesBackButton? alternativeHandler = Avalonia.Application.Current switch
        {
            {
                ApplicationLifetime: ISingleViewApplicationLifetime
                {
                    MainView:
                    {
                        DataContext: INavigatableView
                        {
                            CurrentOperation: IHandlesBackButton hbh
                        }
                    }
                }
            } => hbh,
            _ => null
        };

        Action backPressAction = (alternativeHandler != null)
            ? new Action(() => alternativeHandler.OnBackButtonPressedAsync())
            : new Action(() => base.OnBackPressed());

        string viewModelTypeName = "Unknown or N/A";
        if (alternativeHandler != null)
        {
            var dataContextForLog = (Avalonia.Application.Current?.ApplicationLifetime as ISingleViewApplicationLifetime)
                                    ?.MainView
                                    ?.DataContext;
            viewModelTypeName = dataContextForLog?.GetType().FullName ?? "DataContext non-null, but type unknown";
        }

        System.Diagnostics.Debug.WriteLine(
            alternativeHandler != null
                ? $"Custom back navigation: Handler {alternativeHandler.GetType().FullName} found. ViewModel Context Type: {viewModelTypeName}."
                : "Default back navigation: No custom handler found (pattern did not match)."
        );

        backPressAction();
        return;
    }
}
