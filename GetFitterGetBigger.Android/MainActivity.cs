using Avalonia;
using Avalonia.Android;
using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Views;

namespace GetFitterGetBigger.Android;

[Activity(
    Label = "GetFitterGetBigger.Android",
    Theme = "@style/MyTheme.NoActionBar",
    Icon = "@drawable/icon",
    MainLauncher = true,
    ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize | ConfigChanges.UiMode)]
public class MainActivity : AvaloniaMainActivity<App>
{
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
}
