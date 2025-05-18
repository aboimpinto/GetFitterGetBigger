using System;
using System.Reactive.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Olimpo.NavigationManager;

namespace GetFitterGetBigger.ViewModels;

public partial class DashboardViewModel : ViewModelBase
{
    private readonly INavigationManager _navigationManager;

    private int _imageRotationCount = 3;
    private int _currentImage = 1;

    [ObservableProperty]
    private Bitmap _imageSource;

    [ObservableProperty]
    private CarouselInfoRecord _carouselInfo;

    public DashboardViewModel(INavigationManager navigationManager)
    {
        this._navigationManager = navigationManager;

        this.CarouselInfo = LoadTryGetStrongPlan();
        this.ImageSource = LoadImageFromAssets(this.CarouselInfo.ImageSource);

        Observable
            .Interval(TimeSpan.FromSeconds(3))
            .Subscribe(x =>
            {
                this._currentImage++;

                if (this._currentImage > this._imageRotationCount)
                {
                    this._currentImage = 1;
                }

                this.CarouselInfo = this._currentImage switch
                {
                    1 => LoadTryGetStrongPlan(),
                    2 => LoadTryGetDietPlan(),
                    3 => LoadTryReadyForBeachPlan(),
                    _ => LoadTryGetStrongPlan()
                };

                this.ImageSource = LoadImageFromAssets(this.CarouselInfo.ImageSource);
            });

    }

    [RelayCommand]
    public void ToggleHamburgerMenu()
    {

    }

    [RelayCommand]
    private async Task WorkoutShortcut()
    {
        await this._navigationManager.NavigateAsync("WorkoutsViewModel");
    }

    private static Bitmap LoadImageFromAssets(string path)
    {
        try
        {
            var assembly = Assembly.GetExecutingAssembly();
            using (var stream = assembly.GetManifestResourceStream(assembly.GetName().Name + "." + path.Replace('/', '.')))
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

    private static CarouselInfoRecord LoadTryGetStrongPlan() =>
        new(
            "TRY OUR GET STRONG PLAN",
            "Achieve your strength goals with expert guidance.",
            "Assets/TryGetStrongPlan.png", 1);

    private static CarouselInfoRecord LoadTryGetDietPlan() =>
        new(
            "TRY OUR DIET PLAN",
            "You Fitness journey starts in the Kitchen.",
            "Assets/TryOurDietPlan.png", 2);

    private static CarouselInfoRecord LoadTryReadyForBeachPlan() =>
        new(
            "TRY OUR READY FOR BEACH PLAN",
            "Ready to wear bikini this year?",
            "Assets/TryReadyForBeachPlan.png", 3);
}

public record CarouselInfoRecord(string Title, string SubTitle, string ImageSource, int Order);
