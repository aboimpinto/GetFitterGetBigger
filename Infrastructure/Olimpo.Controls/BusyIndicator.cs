using Avalonia;
using Avalonia.Controls;

namespace Olimpo.Controls;

public class BusyIndicator : ContentControl
{
    public bool IsBusy 
    { 
        get => (bool)GetValue(IsBusyProperty);
        set => SetValue(IsBusyProperty, value);
    }

    public static readonly AvaloniaProperty IsBusyProperty = 
        AvaloniaProperty.Register<BusyIndicator, bool>(nameof(IsBusy));
}
