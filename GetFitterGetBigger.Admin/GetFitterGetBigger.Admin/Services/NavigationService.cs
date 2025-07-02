using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;

namespace GetFitterGetBigger.Admin.Services
{
    public interface INavigationService
    {
        void Initialize(NavigationManager navigationManager);
    }

    public class NavigationService : INavigationService, IDisposable
    {
        private NavigationManager? _navigationManager;
        private readonly IExerciseStateService _exerciseStateService;
        private string _lastSection = "";

        public NavigationService(IExerciseStateService exerciseStateService)
        {
            _exerciseStateService = exerciseStateService;
        }

        public void Initialize(NavigationManager navigationManager)
        {
            _navigationManager = navigationManager;
            _navigationManager.LocationChanged += OnLocationChanged;
            UpdateSection(_navigationManager.Uri);
        }

        private void OnLocationChanged(object? sender, LocationChangedEventArgs e)
        {
            UpdateSection(e.Location);
        }

        private void UpdateSection(string uri)
        {
            var currentSection = GetSectionFromUri(uri);

            // Clear stored exercise page if navigating away from exercises section
            if (_lastSection == "exercises" && currentSection != "exercises")
            {
                _exerciseStateService.ClearStoredPage();
            }

            _lastSection = currentSection;
        }

        private string GetSectionFromUri(string uri)
        {
            var path = new Uri(uri).LocalPath.ToLower();

            if (path.Contains("/exercises"))
                return "exercises";
            else if (path.Contains("/clients"))
                return "clients";
            else if (path.Contains("/workouts"))
                return "workouts";
            else if (path.Contains("/plans"))
                return "plans";
            else
                return "dashboard";
        }

        public void Dispose()
        {
            if (_navigationManager != null)
            {
                _navigationManager.LocationChanged -= OnLocationChanged;
            }
        }
    }
}