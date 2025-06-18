using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace Olimpo.Controls;

public class Icon : ContentControl
{
    public Geometry Data 
    {
        get => (Geometry)GetValue(DataProperty);
        set => SetValue(DataProperty, value);
    }

    public static readonly AvaloniaProperty DataProperty = 
        AvaloniaProperty.Register<Icon, Geometry>(nameof(Data));
}
