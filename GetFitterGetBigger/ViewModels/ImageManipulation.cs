using System;
using System.Reflection;
using Avalonia.Media.Imaging;

namespace GetFitterGetBigger.ViewModels;

public static class ImageManipulation
{
    public static Bitmap LoadImageFromAssets(string path)
    {
        try
        {
            var assembly = Assembly.GetExecutingAssembly();
            using (var stream = assembly.GetManifestResourceStream($"{assembly.GetName().Name}.Assets.{System.IO.Path.GetFileName(path)}"))
            {
                if (stream != null)
                {
                    return new Bitmap(stream);
                }
                else
                {
                    // Handle the case where the image is not found
                    // You might want to log an error or set a default image
                    System.Diagnostics.Debug.WriteLine($"Error: Image not found at '{path}'");
                    throw new InvalidOperationException();
                }
            }
        }
        catch (Exception ex)
        {
            // Handle any potential exceptions during image loading
            System.Diagnostics.Debug.WriteLine($"Error loading image: {ex.Message}");
            throw new InvalidOperationException();
        }
    }
}
