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
using Android.Telephony;
using Android;

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

    // Public method to request phone number permission
    public void RequestPhoneNumberPermission()
    {
        if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
        {
            var phoneNumbersPermission = CheckSelfPermission("android.permission.READ_PHONE_NUMBERS");
            if (phoneNumbersPermission != Permission.Granted)
            {
                System.Diagnostics.Debug.WriteLine("Requesting READ_PHONE_NUMBERS permission...");
                RequestPermissions(new string[] { "android.permission.READ_PHONE_NUMBERS" }, 102);
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("READ_PHONE_NUMBERS permission already granted");
                ExtractPhoneNumber();
            }
        }
    }

    // Method to extract phone number
    private void ExtractPhoneNumber()
    {
        try
        {
            var telephonyManager = (TelephonyManager)GetSystemService(TelephonyService);
            string phoneNumber = telephonyManager?.Line1Number;
            
            if (!string.IsNullOrEmpty(phoneNumber))
            {
                System.Diagnostics.Debug.WriteLine($"Phone Number: {phoneNumber}");
                // TODO: Use the phone number as needed in your app
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("Phone number not available or empty");
                // TODO: Handle case when phone number is not available
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error extracting phone number: {ex.Message}");
        }
    }

    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);
        Window.SetSoftInputMode(SoftInput.AdjustResize);

        // Extract the phone number
        try
        {
            // Check if we have any of the required permissions
            bool hasPermission = false;
            
            // Check for permissions only on Android M (API 23) and above
            if (Build.VERSION.SdkInt >= BuildVersionCodes.M)
            {
                System.Diagnostics.Debug.WriteLine($"Android version: {Build.VERSION.SdkInt}");
                
                var phoneStatePermission = CheckSelfPermission("android.permission.READ_PHONE_STATE");
                var smsPermission = CheckSelfPermission("android.permission.READ_SMS");
                var phoneNumbersPermission = Permission.Denied;
                
                // READ_PHONE_NUMBERS is only available on Android O (API 26) and above
                if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
                {
                    phoneNumbersPermission = CheckSelfPermission("android.permission.READ_PHONE_NUMBERS");
                }
                
                System.Diagnostics.Debug.WriteLine($"READ_PHONE_STATE permission: {phoneStatePermission}");
                System.Diagnostics.Debug.WriteLine($"READ_SMS permission: {smsPermission}");
                System.Diagnostics.Debug.WriteLine($"READ_PHONE_NUMBERS permission: {phoneNumbersPermission}");
                
                hasPermission = phoneStatePermission == Permission.Granted ||
                               smsPermission == Permission.Granted ||
                               phoneNumbersPermission == Permission.Granted;
            }
            else
            {
                // For older Android versions, permission is granted at install time
                hasPermission = true;
                System.Diagnostics.Debug.WriteLine("Android version < M, assuming permissions granted");
            }
            
            if (hasPermission)
            {
                // Get the phone number
                var telephonyManager = (TelephonyManager)GetSystemService(TelephonyService);
                string phoneNumber = telephonyManager?.Line1Number;
                
                if (!string.IsNullOrEmpty(phoneNumber))
                {
                    System.Diagnostics.Debug.WriteLine($"Phone Number: {phoneNumber}");
                    // TODO: Use the phone number as needed in your app
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("Phone number not available or empty");
                    System.Diagnostics.Debug.WriteLine("The phone number could not be retrieved from this device/carrier.");
                    System.Diagnostics.Debug.WriteLine("This is often due to carrier restrictions or device configuration.");
                    // TODO: Handle case when phone number is not available
                }
            }
            else
            {
                // Request permissions if not granted
                System.Diagnostics.Debug.WriteLine("No permissions granted, requesting...");
                
                // For Android O and above, request all three permissions
                // For Android M to N, only request READ_PHONE_STATE and READ_SMS
                var permissionsToRequest = Build.VERSION.SdkInt >= BuildVersionCodes.O
                    ? new string[] 
                      { 
                          "android.permission.READ_PHONE_STATE",
                          "android.permission.READ_SMS",
                          "android.permission.READ_PHONE_NUMBERS"
                      }
                    : new string[] 
                      { 
                          "android.permission.READ_PHONE_STATE",
                          "android.permission.READ_SMS"
                      };
                
                RequestPermissions(permissionsToRequest, 101);
                System.Diagnostics.Debug.WriteLine($"Requesting permissions: {string.Join(", ", permissionsToRequest)}");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error in phone number extraction: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
            // TODO: Handle error appropriately
        }
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

    public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
    {
        base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        
        if (requestCode == 101 || requestCode == 102) // Phone permissions request codes
        {
            // Check if any of the requested permissions was granted
            bool anyPermissionGranted = false;
            for (int i = 0; i < grantResults.Length; i++)
            {
                if (grantResults[i] == Permission.Granted)
                {
                    anyPermissionGranted = true;
                    System.Diagnostics.Debug.WriteLine($"Permission granted: {permissions[i]}");
                    break;
                }
            }
            
            if (anyPermissionGranted)
            {
                // Permission granted, extract phone number
                ExtractPhoneNumber();
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("Phone permissions denied by user");
                
                // If this was specifically the READ_PHONE_NUMBERS request
                if (requestCode == 102)
                {
                    System.Diagnostics.Debug.WriteLine("READ_PHONE_NUMBERS permission was denied.");
                    System.Diagnostics.Debug.WriteLine("You need to manually enable it in Settings:");
                    System.Diagnostics.Debug.WriteLine("Settings > Apps > GetFitterGetBigger > Permissions > Phone");
                }
                
                // TODO: Handle permission denial - maybe show a message to the user
            }
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
